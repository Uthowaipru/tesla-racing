using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VehicleSystem
{
    public enum AudioCategory { Sfx = 0, Engine = 1, Music = 2 }

    /// <summary>
    /// All sounds of the game as data. Add a sound by adding an entry (or filling clips of an existing id);
    /// no code change needed. Runtime state (cooldown timers, voices) lives in AudioManager, not here.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioBank", menuName = "Vehicle/Audio Bank")]
    public class AudioBank : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string id;
            public AudioCategory category = AudioCategory.Sfx;
            [Tooltip("One clip is picked at random (never the same twice in a row).")]
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume = 1f;
            public float pitchMin = 1f;
            public float pitchMax = 1f;
            [Tooltip("3D sound at a world position. Ignored when the caller gives no position.")]
            public bool spatial;
            [Tooltip("Loops are used through AcquireLoop (engines, skid). Music uses PlayMusic.")]
            public bool loop;
            [Tooltip("Most copies of this id playing at once. The oldest is replaced. 0 = no limit.")]
            [Min(0)] public int maxVoices = 4;
            [Tooltip("Seconds before this id may play again.")]
            [Min(0f)] public float cooldown = 0.05f;
            [Min(0.1f)] public float minDistance = 3f;
            [Min(1f)] public float maxDistance = 40f;
            [Tooltip("When all voices are busy, a sound may replace a playing one of equal or lower priority.")]
            [Range(0, 10)] public int priority = 5;
        }

        /// <summary>Which engine entries a vehicle uses. Vehicles not listed use engine_player / engine_other.</summary>
        [Serializable]
        public class VehicleEngine
        {
            [Tooltip("The vehicle's definition (the same asset as in the Vehicle Catalog; its id is permanent).")]
            public VehicleDefinition vehicle;
            [Tooltip("Entry id when the local player drives it. Empty = engine_player.")]
            public string playerEngineId;
            [Tooltip("Entry id when another car drives it. Empty = engine_other.")]
            public string otherEngineId;
        }

        public List<Entry> entries = new List<Entry>();

        [Header("Per-vehicle engines (optional)")]
        public List<VehicleEngine> vehicleEngines = new List<VehicleEngine>();

        [Header("Engine tuning")]
        [Tooltip("Speed (m/s) at which the engine reaches its highest pitch.")]
        public float engineReferenceSpeed = 30f;
        [Range(0f, 1f)] public float engineIdleVolume = 0.35f;
        [Tooltip("How fast pitch and volume follow the car. Higher = snappier.")]
        public float engineResponse = 6f;
        [Tooltip("Other cars only get an engine voice inside this distance (metres).")]
        public float otherEngineRange = 40f;
        [Tooltip("Most engine voices given to cars that are not the local player.")]
        [Min(0)] public int maxOtherEngines = 3;

        [Header("Impacts")]
        [Tooltip("Hits slower than this speedRatio make no sound.")]
        [Range(0f, 1f)] public float impactMinRatio = 0.05f;
        [Tooltip("Hits at or above this speedRatio use impact_hard, below use impact_light.")]
        [Range(0f, 1f)] public float impactHardRatio = 0.5f;

        /// <summary>Engine ids for the car whose spawned prefab has this name (without "(Clone)"). False if the vehicle is not listed.</summary>
        public bool TryGetVehicleEngine(string prefabName, out string playerId, out string otherId)
        {
            playerId = null;
            otherId = null;
            for (int i = 0; i < vehicleEngines.Count; i++)
            {
                VehicleEngine v = vehicleEngines[i];
                if (v == null || v.vehicle == null || v.vehicle.prefab == null) continue;
                if (v.vehicle.prefab.name != prefabName) continue;
                playerId = v.playerEngineId;
                otherId = v.otherEngineId;
                return true;
            }
            return false;
        }

        public int IndexOf(string id)
        {
            if (string.IsNullOrEmpty(id)) return -1;
            for (int i = 0; i < entries.Count; i++)
                if (entries[i] != null && entries[i].id == id) return i;
            return -1;
        }

        /// <summary>Adds every default id that is missing (clips stay empty). Returns how many were added.</summary>
        public int AddMissingDefaults()
        {
            int before = entries.Count;

            // id, category, spatial, loop, maxVoices, cooldown, pitchMin, pitchMax, volume, priority
            Ensure(AudioIds.UiClick, AudioCategory.Sfx, false, false, 2, 0.03f, 1f, 1f, 0.8f, 9);
            Ensure(AudioIds.UiBack, AudioCategory.Sfx, false, false, 2, 0.03f, 1f, 1f, 0.8f, 9);

            Ensure(AudioIds.EnginePlayer, AudioCategory.Engine, false, true, 1, 0f, 0.8f, 2.0f, 0.8f, 8);
            Ensure(AudioIds.EngineOther, AudioCategory.Engine, true, true, 3, 0f, 0.8f, 1.7f, 0.6f, 3);
            Ensure(AudioIds.SkidLoop, AudioCategory.Sfx, false, true, 1, 0f, 0.9f, 1.1f, 0.7f, 6);
            Ensure(AudioIds.ImpactLight, AudioCategory.Sfx, true, false, 3, 0.08f, 0.9f, 1.1f, 0.8f, 5);
            Ensure(AudioIds.ImpactHard, AudioCategory.Sfx, true, false, 3, 0.10f, 0.85f, 1.05f, 1f, 7);
            Ensure(AudioIds.Destroyed, AudioCategory.Sfx, true, false, 2, 0.20f, 0.95f, 1.05f, 1f, 9);
            Ensure(AudioIds.Respawn, AudioCategory.Sfx, true, false, 2, 0.20f, 1f, 1f, 0.8f, 6);
            Ensure(AudioIds.Recover, AudioCategory.Sfx, true, false, 2, 0.20f, 1f, 1f, 0.8f, 6);
            Ensure(AudioIds.BoostStart, AudioCategory.Sfx, true, false, 2, 0.10f, 1f, 1f, 0.9f, 7);
            Ensure(AudioIds.BoostEnd, AudioCategory.Sfx, true, false, 2, 0.10f, 1f, 1f, 0.7f, 5);

            Ensure(AudioIds.Pickup, AudioCategory.Sfx, true, false, 3, 0.05f, 0.95f, 1.05f, 0.9f, 7);
            Ensure(AudioIds.TrafficKnock, AudioCategory.Sfx, true, false, 4, 0.06f, 0.9f, 1.1f, 0.8f, 4);
            Ensure(AudioIds.WrongWayStart, AudioCategory.Sfx, false, false, 1, 0.5f, 1f, 1f, 0.9f, 8);
            Ensure(AudioIds.WrongWayEnd, AudioCategory.Sfx, false, false, 1, 0.5f, 1f, 1f, 0.7f, 6);

            Ensure(AudioIds.CountdownTick, AudioCategory.Sfx, false, false, 1, 0.2f, 1f, 1f, 0.9f, 10);
            Ensure(AudioIds.CountdownGo, AudioCategory.Sfx, false, false, 1, 0.2f, 1f, 1f, 1f, 10);
            Ensure(AudioIds.MatchFinish, AudioCategory.Sfx, false, false, 1, 0.5f, 1f, 1f, 1f, 10);
            Ensure(AudioIds.PointsBump, AudioCategory.Sfx, false, false, 2, 0.08f, 0.95f, 1.05f, 0.6f, 5);
            Ensure(AudioIds.PointsChain, AudioCategory.Sfx, false, false, 2, 0.10f, 1f, 1f, 0.7f, 6);
            Ensure(AudioIds.PointsRam, AudioCategory.Sfx, false, false, 2, 0.10f, 1f, 1f, 0.8f, 6);
            Ensure(AudioIds.PointsOvertake, AudioCategory.Sfx, false, false, 2, 0.10f, 1f, 1f, 0.7f, 6);
            Ensure(AudioIds.PointsCrossing, AudioCategory.Sfx, false, false, 2, 0.10f, 1f, 1f, 0.8f, 6);
            Ensure(AudioIds.PointsPenalty, AudioCategory.Sfx, false, false, 2, 0.20f, 1f, 1f, 0.8f, 7);

            return entries.Count - before;
        }

        void Ensure(string id, AudioCategory category, bool spatial, bool loop, int maxVoices,
                    float cooldown, float pitchMin, float pitchMax, float volume, int priority)
        {
            if (IndexOf(id) >= 0) return;
            entries.Add(new Entry
            {
                id = id,
                category = category,
                clips = new AudioClip[0],
                volume = volume,
                pitchMin = pitchMin,
                pitchMax = pitchMax,
                spatial = spatial,
                loop = loop,
                maxVoices = maxVoices,
                cooldown = cooldown,
                priority = priority
            });
        }

        [ContextMenu("Add Missing Default Entries")]
        void AddMissingDefaultsMenu()
        {
            AddMissingDefaults();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
