using UnityEngine;

namespace VehicleSystem
{
    /// <summary>
    /// All the point values and scoring rules of a match, as a data asset (a ScriptableObject).
    /// Make one per mode or level (Create > Vehicle System > Scoring Rules) and drag it into MatchManager > Rules.
    /// The defaults below are the GDD v0.3 values, the same as the old fields on MatchManager.
    /// Not here on purpose: match length, countdown and the road sources; those belong to the scene or the room.
    /// </summary>
    [CreateAssetMenu(menuName = "Vehicle System/Scoring Rules", fileName = "ScoringRules")]
    public class ScoringRules : ScriptableObject
    {
        [Header("Distance")]
        public float metersPerReward = 100f;
        public int pointsPerReward = 10;
        [Tooltip("A jump bigger than this in one frame (a respawn) does not count as driven distance.")]
        public float teleportDistance = 15f;

        [Header("NPC side bumps")]
        public int fastBumpPoints = 20;
        public int fullThrottleBumpPoints = 30;

        [Header("Chain: paid bumps within the window")]
        public float chainWindow = 4f;
        public int chain2Bonus = 5;
        public int chain3Bonus = 10;
        public int chain4Bonus = 20;
        [Tooltip("From the 5th bump in a chain, EVERY further bump adds this.")]
        public int chain5PlusBonus = 30;

        [Header("Rivals (other players)")]
        public int rearRamPoints = 20;
        public int sideRamPoints = 25;
        public int overtakePoints = 10;
        [Tooltip("The same rival pays a ram only once in this many seconds (stops farming).")]
        public float ramCooldown = 6f;
        [Tooltip("The same rival pays an overtake only once in this many seconds.")]
        public float overtakeCooldown = 8f;
        [Tooltip("Meters ahead or behind a rival you must be before the order counts as changed.")]
        public float overtakeMargin = 2f;
        [Range(10f, 80f)] public float rammedAreaHalfAngle = 40f;
        [Tooltip("ON: a rival hit by another racer's nose at this speed tier or faster is DESTROYED, and the rammer is not hurt and is paid. OFF: rams only damage.")]
        public bool rivalKills = true;
        public SpeedTier rivalKillTier = SpeedTier.FullThrottle;
        [Tooltip("Nose to nose: the faster racer wins (survives, is paid this many points), the other is destroyed.")]
        public int headOnWinPoints = 30;
        [Tooltip("If both are within this share of top speed of each other nobody wins and the normal damage rules apply.")]
        [Range(0f, 0.5f)] public float headOnSpeedMargin = 0.1f;

        [Header("Penalty")]
        public int destroyedPenalty = 50;
        public bool allowNegativeScore = false;

        [Header("After the match (shown on the results screen)")]
        public int pointsPerCoin = 10;
        public int pointsPerCash = 40;

        /// <summary>Copy these values onto a MatchManager (what it uses while scoring).</summary>
        public void ApplyTo(MatchManager m)
        {
            if (m == null) return;
            m.metersPerReward = metersPerReward;
            m.pointsPerReward = pointsPerReward;
            m.teleportDistance = teleportDistance;

            m.fastBumpPoints = fastBumpPoints;
            m.fullThrottleBumpPoints = fullThrottleBumpPoints;

            m.chainWindow = chainWindow;
            m.chain2Bonus = chain2Bonus;
            m.chain3Bonus = chain3Bonus;
            m.chain4Bonus = chain4Bonus;
            m.chain5PlusBonus = chain5PlusBonus;

            m.rearRamPoints = rearRamPoints;
            m.sideRamPoints = sideRamPoints;
            m.overtakePoints = overtakePoints;
            m.ramCooldown = ramCooldown;
            m.overtakeCooldown = overtakeCooldown;
            m.overtakeMargin = overtakeMargin;
            m.rammedAreaHalfAngle = rammedAreaHalfAngle;
            m.rivalKills = rivalKills;
            m.rivalKillTier = rivalKillTier;
            m.headOnWinPoints = headOnWinPoints;
            m.headOnSpeedMargin = headOnSpeedMargin;

            m.destroyedPenalty = destroyedPenalty;
            m.allowNegativeScore = allowNegativeScore;

            m.pointsPerCoin = pointsPerCoin;
            m.pointsPerCash = pointsPerCash;
        }

        /// <summary>Copy the current numbers of a MatchManager into this asset (used by the editor tool).</summary>
        public void CaptureFrom(MatchManager m)
        {
            if (m == null) return;
            metersPerReward = m.metersPerReward;
            pointsPerReward = m.pointsPerReward;
            teleportDistance = m.teleportDistance;

            fastBumpPoints = m.fastBumpPoints;
            fullThrottleBumpPoints = m.fullThrottleBumpPoints;

            chainWindow = m.chainWindow;
            chain2Bonus = m.chain2Bonus;
            chain3Bonus = m.chain3Bonus;
            chain4Bonus = m.chain4Bonus;
            chain5PlusBonus = m.chain5PlusBonus;

            rearRamPoints = m.rearRamPoints;
            sideRamPoints = m.sideRamPoints;
            overtakePoints = m.overtakePoints;
            ramCooldown = m.ramCooldown;
            overtakeCooldown = m.overtakeCooldown;
            overtakeMargin = m.overtakeMargin;
            rammedAreaHalfAngle = m.rammedAreaHalfAngle;
            rivalKills = m.rivalKills;
            rivalKillTier = m.rivalKillTier;
            headOnWinPoints = m.headOnWinPoints;
            headOnSpeedMargin = m.headOnSpeedMargin;

            destroyedPenalty = m.destroyedPenalty;
            allowNegativeScore = m.allowNegativeScore;

            pointsPerCoin = m.pointsPerCoin;
            pointsPerCash = m.pointsPerCash;
        }
    }
}
