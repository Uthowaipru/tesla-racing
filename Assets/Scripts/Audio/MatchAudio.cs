using UnityEngine;
using UnityEngine.SceneManagement;

namespace VehicleSystem
{
    /// <summary>
    /// Match sounds: countdown ticks and go, points chimes (local player only), finish jingle.
    /// Listens to MatchManager events only. Lives on the AudioManager object.
    /// </summary>
    [DisallowMultipleComponent]
    public class MatchAudio : MonoBehaviour
    {
        MatchManager match;
        int lastTick = -1;

        /// <summary>True once a MatchManager has been found.</summary>
        public bool HasMatch { get { return match != null; } }
        public bool IsRunning { get { return match != null && match.State == MatchState.Running; } }
        /// <summary>Time.unscaledTime when the match last became Running.</summary>
        public float RunningSince { get; private set; }

        /// <summary>Point the listener at a match. Call this from the game if the match is created after the scene loads.</summary>
        public void Bind(MatchManager manager)
        {
            if (ReferenceEquals(match, manager)) return;
            Unsubscribe();
            match = manager;
            lastTick = -1;
            Subscribe();
        }

        /// <summary>
        /// One lookup per scene load (never per frame). MatchManager has no static Instance yet;
        /// see change request: a MatchManager.Instance or static "enabled" event would replace this.
        /// </summary>
        public void RefreshBinding()
        {
            MatchManager found = FindFirstObjectByType<MatchManager>();
            if (found != null) Bind(found);
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Subscribe();
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Unsubscribe();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshBinding();
        }

        void Subscribe()
        {
            if (match == null) return;
            match.StateChanged += OnStateChanged;
            match.PointsAwarded += OnPointsAwarded;
            match.Finished += OnFinished;
        }

        void Unsubscribe()
        {
            if ((object)match == null) return; // also fine if the Unity object was destroyed
            match.StateChanged -= OnStateChanged;
            match.PointsAwarded -= OnPointsAwarded;
            match.Finished -= OnFinished;
        }

        void Update()
        {
            if (match == null || match.State != MatchState.Countdown) return;

            int n = Mathf.CeilToInt(match.CountdownLeft);
            if (n <= 0 || n == lastTick) return;
            lastTick = n;

            AudioManager am = AudioManager.Instance;
            if (am != null) am.Play(AudioIds.CountdownTick);
        }

        void OnStateChanged(MatchState state)
        {
            AudioManager am = AudioManager.Instance;
            if (am == null) return;

            if (state == MatchState.Countdown) lastTick = -1;
            else if (state == MatchState.Running)
            {
                RunningSince = Time.unscaledTime;
                am.Play(AudioIds.CountdownGo);
            }
        }

        void OnPointsAwarded(MatchPlayer player, int points, ScoreReason reason)
        {
            if (match == null || player != match.LocalPlayer) return;

            string id;
            switch (reason)
            {
                case ScoreReason.NpcBump: id = AudioIds.PointsBump; break;
                case ScoreReason.Chain: id = AudioIds.PointsChain; break;
                case ScoreReason.RivalRam: id = AudioIds.PointsRam; break;
                case ScoreReason.Overtake: id = AudioIds.PointsOvertake; break;
                case ScoreReason.Crossing: id = AudioIds.PointsCrossing; break;
                case ScoreReason.Destroyed: id = AudioIds.PointsPenalty; break;
                default: return; // Distance and WrongWay are continuous: no chime
            }

            AudioManager am = AudioManager.Instance;
            if (am != null) am.Play(id);
        }

        void OnFinished(MatchResult result)
        {
            AudioManager am = AudioManager.Instance;
            if (am != null) am.Play(AudioIds.MatchFinish);
        }
    }
}
