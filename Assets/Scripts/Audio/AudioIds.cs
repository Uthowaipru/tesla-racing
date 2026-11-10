namespace VehicleSystem
{
    /// <summary>
    /// Permanent sound ids. Never rename a value once shipped (see Audio/AUDIO_IDS.md).
    /// Add new ids at the end of a group.
    /// </summary>
    public static class AudioIds
    {
        // UI (called by RC.UI)
        public const string UiClick = "ui_click";
        public const string UiBack = "ui_back";

        // Vehicle
        public const string EnginePlayer = "engine_player";
        public const string EngineOther = "engine_other";
        public const string SkidLoop = "skid_loop";
        public const string ImpactLight = "impact_light";
        public const string ImpactHard = "impact_hard";
        public const string Destroyed = "destroyed";
        public const string Respawn = "respawn";
        public const string Recover = "recover";
        public const string BoostStart = "boost_start";
        public const string BoostEnd = "boost_end";

        // World
        public const string Pickup = "pickup";
        public const string TrafficKnock = "traffic_knock";
        public const string WrongWayStart = "wrong_way_start";
        public const string WrongWayEnd = "wrong_way_end";

        // Match
        public const string CountdownTick = "countdown_tick";
        public const string CountdownGo = "countdown_go";
        public const string MatchFinish = "match_finish";
        public const string PointsBump = "points_bump";
        public const string PointsChain = "points_chain";
        public const string PointsRam = "points_ram";
        public const string PointsOvertake = "points_overtake";
        public const string PointsCrossing = "points_crossing";
        public const string PointsPenalty = "points_penalty";
    }
}
