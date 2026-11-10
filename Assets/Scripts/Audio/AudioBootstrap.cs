using UnityEngine;

namespace VehicleSystem
{
    /// <summary>
    /// Attaches audio to the game at startup without any other module changing:
    /// creates the AudioManager if the scene has none, adds VehicleAudio to every car
    /// (cars already in the scene and cars enabled later), and makes sure MatchAudio / WorldAudio exist.
    /// Same pattern as the rendering integration's drift bootstrap.
    /// </summary>
    internal static class AudioBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            VehicleController.VehicleEnabled -= OnVehicleEnabled;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            AudioManager manager = EnsureManager();

            VehicleController.VehicleEnabled -= OnVehicleEnabled; // never subscribe twice
            VehicleController.VehicleEnabled += OnVehicleEnabled;

            var all = VehicleController.All;
            for (int i = 0; i < all.Count; i++) Attach(all[i]);

            if (manager.MatchSounds != null) manager.MatchSounds.RefreshBinding();
            if (manager.WorldSounds != null) manager.WorldSounds.RefreshBinding();
        }

        static AudioManager EnsureManager()
        {
            AudioManager manager = AudioManager.Instance;
            if (manager == null)
            {
                GameObject go = new GameObject("AudioManager");
                manager = go.AddComponent<AudioManager>(); // Awake sets Instance and DontDestroyOnLoad
            }

            manager.MatchSounds = Ensure<MatchAudio>(manager.gameObject);
            manager.WorldSounds = Ensure<WorldAudio>(manager.gameObject);
            return manager;
        }

        static T Ensure<T>(GameObject go) where T : Component
        {
            T component;
            if (!go.TryGetComponent(out component)) component = go.AddComponent<T>();
            return component;
        }

        static void OnVehicleEnabled(VehicleController vehicle)
        {
            Attach(vehicle);
        }

        static void Attach(VehicleController vehicle)
        {
            if (vehicle == null) return;
            VehicleAudio existing;
            if (!vehicle.TryGetComponent(out existing)) vehicle.gameObject.AddComponent<VehicleAudio>();
        }
    }
}
