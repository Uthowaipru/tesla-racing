using UnityEngine;

namespace VehicleSystem
{
    /// <summary>
    /// Tiny shared flags that gameplay scripts (camera, input) need to read without knowing about the UI.
    /// PauseMenu sets IsPaused; everything else only reads it. Lives in Core so nothing depends upward.
    /// </summary>
    public static class GameState
    {
        /// <summary>True while the pause menu is open.</summary>
        public static bool IsPaused { get; private set; }

        /// <summary>Called by PauseMenu only.</summary>
        public static void SetPaused(bool paused) => IsPaused = paused;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => IsPaused = false;
    }
}
