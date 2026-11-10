namespace VehicleSystem
{
    /// <summary>Small read-only helpers shared by the audio scripts.</summary>
    internal static class AudioUtil
    {
        /// <summary>True when this car is driven by the local player. Not for per-frame use on many cars (cache the driver instead).</summary>
        public static bool IsLocalPlayer(VehicleController vehicle)
        {
            if (vehicle == null) return false;
            VehicleDriver driver = vehicle.GetComponentInParent<VehicleDriver>();
            return driver != null && driver.IsLocalPlayer;
        }
    }
}
