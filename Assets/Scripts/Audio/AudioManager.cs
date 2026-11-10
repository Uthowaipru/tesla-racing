using System;
using System.Collections.Generic;
using UnityEngine;

namespace VehicleSystem
{
    /// <summary>
    /// Plays every sound in the game from a fixed pool of AudioSources (no AudioSource is created during play).
    /// Sounds come from an AudioBank asset (Resources/VehicleSystem/AudioBank). Audio only listens to the game;
    /// it never changes game state. Created automatically by AudioBootstrap if the scene has none.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        public const string BankResourcePath = "VehicleSystem/AudioBank";

        const string KeyMaster = "VehicleSystem.Audio.Master";
        const string KeySfx = "VehicleSystem.Audio.Sfx";
        const string KeyMusic = "VehicleSystem.Audio.Music";
        const string KeyEngine = "VehicleSystem.Audio.Engine";
        const string KeyMuted = "VehicleSystem.Audio.Muted";

        public static AudioManager Instance { get; private set; }

        [Tooltip("Optional. If empty, Resources/VehicleSystem/AudioBank is loaded.")]
        [SerializeField] AudioBank bank;
        [Tooltip("Hard limit of one-shot sounds playing at once.")]
        [SerializeField, Range(4, 32)] int oneShotVoices = 16;
        [Tooltip("Pooled loop voices (engines, skid).")]
        [SerializeField, Range(2, 16)] int loopVoices = 8;
        [Tooltip("Loop voices kept free for the local player; other cars may not use the last ones.")]
        [SerializeField, Range(0, 8)] int loopReserve = 2;

        sealed class Voice
        {
            public AudioSource source;
            public Transform t;
            public int entry = -1;
            public float startTime;
            public float endTime;
            public int priority;
        }

        sealed class EntryState
        {
            public float lastTime = -999f;
            public int lastClip = -1;
        }

        Voice[] voices;
        AudioLoop[] loops;
        AudioSource musicSource;
        bool musicActive;
        float musicBase;

        readonly Dictionary<string, int> index = new Dictionary<string, int>();
        EntryState[] states = new EntryState[0];

        float master = 1f, sfx = 1f, music = 1f, engine = 1f;
        bool muted;
        bool dirty;
        float saveAt;

        Transform listener;
        float nextListenerCheck;

        public event Action VolumesChanged;

        /// <summary>Set by AudioBootstrap. May be null if the manager was placed by hand.</summary>
        public MatchAudio MatchSounds { get; internal set; }
        public WorldAudio WorldSounds { get; internal set; }

        public AudioBank Bank { get { return bank; } }

        // ───── Volumes (saved in PlayerPrefs) ─────

        public float MasterVolume { get { return master; } set { SetVolume(ref master, value, KeyMaster); } }
        public float SfxVolume { get { return sfx; } set { SetVolume(ref sfx, value, KeySfx); } }
        public float MusicVolume { get { return music; } set { SetVolume(ref music, value, KeyMusic); } }
        public float EngineVolume { get { return engine; } set { SetVolume(ref engine, value, KeyEngine); } }

        public bool Muted
        {
            get { return muted; }
            set
            {
                if (muted == value) return;
                muted = value;
                PlayerPrefs.SetInt(KeyMuted, muted ? 1 : 0);
                MarkDirty();
                if (muted) StopOneShots();
                RaiseVolumesChanged();
            }
        }

        void SetVolume(ref float field, float value, string key)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(field, value)) return;
            field = value;
            PlayerPrefs.SetFloat(key, value);
            MarkDirty();
            RaiseVolumesChanged();
        }

        void RaiseVolumesChanged()
        {
            Action handler = VolumesChanged;
            if (handler != null) handler();
        }

        void MarkDirty()
        {
            dirty = true;
            saveAt = Time.unscaledTime + 0.5f; // write once the slider stops moving
        }

        void Flush()
        {
            if (!dirty) return;
            dirty = false;
            PlayerPrefs.Save();
        }

        /// <summary>Master x category volume, or 0 when muted.</summary>
        public float GainFor(AudioCategory category)
        {
            if (muted) return 0f;
            switch (category)
            {
                case AudioCategory.Engine: return master * engine;
                case AudioCategory.Music: return master * music;
                default: return master * sfx;
            }
        }

        // ───── Life cycle ─────

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);

            master = PlayerPrefs.GetFloat(KeyMaster, 1f);
            sfx = PlayerPrefs.GetFloat(KeySfx, 1f);
            music = PlayerPrefs.GetFloat(KeyMusic, 1f);
            engine = PlayerPrefs.GetFloat(KeyEngine, 1f);
            muted = PlayerPrefs.GetInt(KeyMuted, 0) != 0;

            if (bank == null) bank = Resources.Load<AudioBank>(BankResourcePath);
            if (bank == null)
            {
                Debug.LogWarning("[Audio] No AudioBank found at Resources/" + BankResourcePath +
                                 ". The game stays silent. Use Tools > Vehicle > Create Audio Bank.");
                bank = ScriptableObject.CreateInstance<AudioBank>();
            }

            BuildPool();
            RebuildIndex();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Flush();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) Flush();
        }

        void OnApplicationQuit()
        {
            Flush();
        }

        void Update()
        {
            float now = Time.unscaledTime;
            if (dirty && now >= saveAt) Flush();

            if (musicActive) musicSource.volume = musicBase * GainFor(AudioCategory.Music);

            if (now >= nextListenerCheck)
            {
                nextListenerCheck = now + 1f;
                if (listener == null)
                {
                    Camera cam = Camera.main;
                    if (cam != null) listener = cam.transform;
                }
            }
        }

        void BuildPool()
        {
            voices = new Voice[oneShotVoices];
            for (int i = 0; i < voices.Length; i++)
            {
                AudioSource s = NewSource("Voice " + i);
                voices[i] = new Voice { source = s, t = s.transform };
            }

            loops = new AudioLoop[loopVoices];
            for (int i = 0; i < loops.Length; i++)
            {
                AudioSource s = NewSource("Loop " + i);
                s.loop = true;
                loops[i] = new AudioLoop { owner = this, source = s, t = s.transform };
            }

            musicSource = NewSource("Music");
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
        }

        AudioSource NewSource(string sourceName)
        {
            GameObject go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);
            AudioSource s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.rolloffMode = AudioRolloffMode.Linear;
            s.dopplerLevel = 0f;
            s.spatialBlend = 0f;
            return s;
        }

        void RebuildIndex()
        {
            index.Clear();
            int n = bank.entries.Count;
            states = new EntryState[n];
            for (int i = 0; i < n; i++)
            {
                states[i] = new EntryState();
                AudioBank.Entry e = bank.entries[i];
                if (e == null || string.IsNullOrEmpty(e.id)) continue;
                if (!index.ContainsKey(e.id)) index.Add(e.id, i); // first one wins
            }
        }

        /// <summary>Use another bank at runtime (tests, per-chapter banks).</summary>
        public void SetBank(AudioBank newBank)
        {
            if (newBank == null) return;
            StopOneShots();
            for (int i = 0; i < loops.Length; i++) ReleaseLoop(loops[i]);
            StopMusic();
            bank = newBank;
            RebuildIndex();
        }

        // ───── Queries ─────

        public bool HasSound(string id)
        {
            int idx;
            if (!TryGetEntry(id, out idx)) return false;
            AudioClip[] clips = bank.entries[idx].clips;
            return clips != null && clips.Length > 0;
        }

        bool TryGetEntry(string id, out int idx)
        {
            idx = -1;
            if (string.IsNullOrEmpty(id)) return false;
            return index.TryGetValue(id, out idx);
        }

        /// <summary>Position of the listener (main camera). False until a camera tagged MainCamera exists.</summary>
        public bool TryGetListenerPosition(out Vector3 position)
        {
            if (listener != null)
            {
                position = listener.position;
                return true;
            }
            position = default(Vector3);
            return false;
        }

        // ───── One-shot sounds ─────

        /// <summary>Plays a sound without position (UI, local player). Always 2D.</summary>
        public void Play(string id)
        {
            PlayInternal(id, default(Vector3), false);
        }

        /// <summary>Plays a sound at a world position. 3D if the bank entry is marked spatial, otherwise 2D.</summary>
        public void Play(string id, Vector3 position)
        {
            PlayInternal(id, position, true);
        }

        void PlayInternal(string id, Vector3 pos, bool hasPos)
        {
            if (muted) return;
            int idx;
            if (!TryGetEntry(id, out idx)) return;

            AudioBank.Entry e = bank.entries[idx];
            AudioClip[] clips = e.clips;
            if (clips == null || clips.Length == 0) return;
            if (e.loop || e.category == AudioCategory.Music) return; // loops: AcquireLoop, music: PlayMusic

            float now = Time.unscaledTime;
            EntryState st = states[idx];
            if (e.cooldown > 0f && now - st.lastTime < e.cooldown) return;

            bool spatial = e.spatial && hasPos;
            if (spatial && listener != null)
            {
                float max = e.maxDistance;
                if ((pos - listener.position).sqrMagnitude > max * max) return; // too far to hear: spend no voice
            }

            Voice free = null, oldestSame = null, oldestSteal = null;
            int same = 0;
            for (int i = 0; i < voices.Length; i++)
            {
                Voice v = voices[i];
                if (now >= v.endTime)
                {
                    if (free == null) free = v;
                    continue;
                }
                if (v.entry == idx)
                {
                    same++;
                    if (oldestSame == null || v.startTime < oldestSame.startTime) oldestSame = v;
                }
                if (v.priority <= e.priority && (oldestSteal == null || v.startTime < oldestSteal.startTime))
                    oldestSteal = v;
            }

            Voice voice;
            if (e.maxVoices > 0 && same >= e.maxVoices) voice = oldestSame;
            else voice = free != null ? free : oldestSteal;
            if (voice == null) return; // everything busier than this sound: drop it

            AudioClip clip = clips[PickClip(st, clips.Length)];
            if (clip == null) return;

            float pitch = e.pitchMin >= e.pitchMax ? e.pitchMin : UnityEngine.Random.Range(e.pitchMin, e.pitchMax);
            if (pitch < 0.1f) pitch = 0.1f;

            AudioSource s = voice.source;
            s.Stop();
            s.clip = clip;
            s.loop = false;
            s.pitch = pitch;
            s.volume = e.volume * GainFor(e.category);
            s.spatialBlend = spatial ? 1f : 0f;
            if (spatial)
            {
                s.minDistance = e.minDistance;
                s.maxDistance = e.maxDistance;
                voice.t.position = pos;
            }
            s.Play();

            voice.entry = idx;
            voice.startTime = now;
            voice.endTime = now + clip.length / pitch + 0.02f;
            voice.priority = e.priority;
            st.lastTime = now;
        }

        static int PickClip(EntryState st, int count)
        {
            if (count <= 1) { st.lastClip = 0; return 0; }
            int r;
            if (st.lastClip < 0 || st.lastClip >= count)
            {
                r = UnityEngine.Random.Range(0, count);
            }
            else
            {
                r = UnityEngine.Random.Range(0, count - 1); // skip the clip played last
                if (r >= st.lastClip) r++;
            }
            st.lastClip = r;
            return r;
        }

        void StopOneShots()
        {
            if (voices == null) return;
            for (int i = 0; i < voices.Length; i++)
            {
                voices[i].source.Stop();
                voices[i].endTime = 0f;
            }
        }

        // ───── Loops (engines, skid) ─────

        /// <summary>
        /// Takes a free loop voice for a bank entry. Returns null if the id has no clip or no voice is free.
        /// lowPriority = for other cars: limited by the bank (maxOtherEngines) and by the reserve for the local player.
        /// Call ReleaseLoop when done.
        /// </summary>
        public AudioLoop AcquireLoop(string id, bool spatial, bool lowPriority)
        {
            int idx;
            if (!TryGetEntry(id, out idx)) return null;
            AudioBank.Entry e = bank.entries[idx];
            AudioClip[] clips = e.clips;
            if (clips == null || clips.Length == 0) return null;

            AudioLoop pick = null;
            int free = 0, lowInUse = 0;
            for (int i = 0; i < loops.Length; i++)
            {
                AudioLoop l = loops[i];
                if (!l.inUse)
                {
                    free++;
                    if (pick == null) pick = l;
                }
                else if (l.lowPriority)
                {
                    lowInUse++;
                }
            }
            if (pick == null) return null;
            if (lowPriority && (free <= loopReserve || lowInUse >= bank.maxOtherEngines)) return null;

            AudioClip clip = clips[PickClip(states[idx], clips.Length)];
            if (clip == null) return null;

            AudioSource s = pick.source;
            s.Stop();
            s.clip = clip;
            s.loop = true;
            s.volume = 0f;
            s.pitch = e.pitchMin;
            s.spatialBlend = spatial ? 1f : 0f;
            s.minDistance = e.minDistance;
            s.maxDistance = e.maxDistance;
            s.time = UnityEngine.Random.value * clip.length * 0.99f; // engines of different cars do not phase-lock
            s.Play();

            pick.inUse = true;
            pick.lowPriority = lowPriority;
            pick.entry = idx;
            pick.baseVolume = e.volume;
            pick.category = e.category;
            pick.PitchMin = e.pitchMin;
            pick.PitchMax = e.pitchMax;
            return pick;
        }

        public void ReleaseLoop(AudioLoop loop)
        {
            if (loop == null || !loop.inUse) return;
            loop.source.Stop();
            loop.source.clip = null;
            loop.inUse = false;
            loop.lowPriority = false;
            loop.entry = -1;
        }

        // ───── Music ─────

        public void PlayMusic(string id)
        {
            int idx;
            if (!TryGetEntry(id, out idx)) return;
            AudioBank.Entry e = bank.entries[idx];
            AudioClip[] clips = e.clips;
            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[PickClip(states[idx], clips.Length)];
            if (clip == null) return;
            if (musicActive && musicSource.isPlaying && musicSource.clip == clip) return;

            musicBase = e.volume;
            musicActive = true;
            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.pitch = 1f;
            musicSource.volume = musicBase * GainFor(AudioCategory.Music);
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicActive = false;
            if (musicSource != null)
            {
                musicSource.Stop();
                musicSource.clip = null;
            }
        }
    }
}
