using UnityEngine;

namespace VehicleSystem
{
    /// <summary>
    /// A pooled looping voice (engine, skid). Obtained from AudioManager.AcquireLoop, returned with ReleaseLoop.
    /// Never created or destroyed during play.
    /// </summary>
    public sealed class AudioLoop
    {
        internal AudioManager owner;
        internal AudioSource source;
        internal Transform t;
        internal bool inUse;
        internal bool lowPriority;
        internal int entry = -1;
        internal float baseVolume;
        internal AudioCategory category;

        /// <summary>Pitch at the quiet end / loud end of the entry (from the bank).</summary>
        public float PitchMin { get; internal set; }
        public float PitchMax { get; internal set; }
        public bool InUse { get { return inUse; } }

        /// <summary>level01 is 0..1 of the entry's own volume; the manager's master/category volumes and Mute are applied here.</summary>
        public void Set(float level01, float pitch)
        {
            if (!inUse) return;
            source.volume = baseVolume * Mathf.Clamp01(level01) * owner.GainFor(category);
            source.pitch = pitch;
        }

        public void SetPosition(Vector3 position)
        {
            if (inUse) t.position = position;
        }
    }
}
