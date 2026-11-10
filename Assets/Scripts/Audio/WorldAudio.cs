using UnityEngine;

namespace VehicleSystem
{
    /// <summary>
    /// World sounds: pickups, traffic knocked, wrong-way warning. Listens to World events only.
    /// Lives on the AudioManager object.
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldAudio : MonoBehaviour
    {
        WrongWayTracker tracker;

        void OnEnable()
        {
            // ASSUMPTION (not compiled): Pickup.Collected and TrafficVehicle.Knocked are static events.
            //Pickup.Collected += OnPickupCollected;
            //TrafficVehicle.Knocked += OnTrafficKnocked;
            SubscribeTracker();
        }

        void OnDisable()
        {
            //Pickup.Collected -= OnPickupCollected;
            //TrafficVehicle.Knocked -= OnTrafficKnocked;
            UnsubscribeTracker();
        }

        /// <summary>Called by VehicleAudio when the local car carries a tracker; Game/UI may call it too.</summary>
        public void SetWrongWayTracker(WrongWayTracker value)
        {
            if (ReferenceEquals(tracker, value)) return;
            UnsubscribeTracker();
            tracker = value;
            SubscribeTracker();
        }

        void SubscribeTracker()
        {
            if (tracker != null) tracker.WrongWayChanged += OnWrongWayChanged;
        }

        void UnsubscribeTracker()
        {
            if ((object)tracker != null) tracker.WrongWayChanged -= OnWrongWayChanged;
        }

        void OnPickupCollected(Pickup pickup, VehicleController collector)
        {
            AudioManager am = AudioManager.Instance;
            if (am == null) return;

            if (AudioUtil.IsLocalPlayer(collector)) am.Play(AudioIds.Pickup);
            else if (pickup != null) am.Play(AudioIds.Pickup, pickup.transform.position);
        }

        void OnTrafficKnocked(TrafficVehicle traffic)
        {
            AudioManager am = AudioManager.Instance;
            if (am == null || traffic == null) return;
            am.Play(AudioIds.TrafficKnock, traffic.transform.position);
        }

        void OnWrongWayChanged(bool wrongWay)
        {
            AudioManager am = AudioManager.Instance;
            if (am == null) return;
            am.Play(wrongWay ? AudioIds.WrongWayStart : AudioIds.WrongWayEnd);
        }
    }
}
