using UnityEngine;
using UnityEngine.SceneManagement;

namespace VehicleSystem
{
    /// <summary>
    /// World sounds that need a scene object: the wrong-way warning. Listens to WrongWayTracker only.
    /// (Pickups and knocked traffic are heard through the cars: see VehicleAudio.)
    /// Lives on the AudioManager object.
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldAudio : MonoBehaviour
    {
        WrongWayTracker tracker;

        /// <summary>Point the listener at a tracker. Game may call this instead of waiting for the lookup.</summary>
        public void SetWrongWayTracker(WrongWayTracker value)
        {
            if (ReferenceEquals(tracker, value)) return;
            Unsubscribe();
            tracker = value;
            Subscribe();
        }

        /// <summary>
        /// One lookup per scene load (never per frame), the same lookup WrongWayTracker does for its own road.
        /// See change request 3: a static hook would replace this.
        /// </summary>
        public void RefreshBinding()
        {
            WrongWayTracker found = FindFirstObjectByType<WrongWayTracker>();
            if (found != null) SetWrongWayTracker(found);
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
            if (tracker != null) tracker.WrongWayChanged += OnWrongWayChanged;
        }

        void Unsubscribe()
        {
            if ((object)tracker != null) tracker.WrongWayChanged -= OnWrongWayChanged;
        }

        void OnWrongWayChanged(bool wrongWay)
        {
            AudioManager am = AudioManager.Instance;
            if (am == null) return;
            am.Play(wrongWay ? AudioIds.WrongWayStart : AudioIds.WrongWayEnd);
        }
    }
}
