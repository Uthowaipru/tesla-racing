using UnityEngine;

namespace VehicleSystem
{
    /// <summary>
    /// Sounds of one car: engine loop, skid loop (local player), impacts, boost, destroyed, respawn, recover.
    /// Added to every car by AudioBootstrap. Only reads the car; never changes it.
    /// The local player's car is 2D; other cars are 3D with a short range and only get an engine voice when close.
    /// </summary>
    [DisallowMultipleComponent]
    public class VehicleAudio : MonoBehaviour
    {
        VehicleController vehicle;
        VehicleDriver driver;
        VehicleHealth health;
        VehicleBoost boost;
        VehicleImpactSensor sensor;
        VehicleRecovery recovery;

        AudioLoop engine;
        AudioLoop skid;
        bool engineIsLocal;
        float pitch01;
        float engineLevel;
        float skidLevel;
        float nextEngineTry;
        float nextSkidTry;
        int lastBatteries;
        int lastRepairs;
        int lastKnockId;
        float lastKnockTime = -999f;

        string enginePlayerId = AudioIds.EnginePlayer;
        string engineOtherId = AudioIds.EngineOther;

        bool IsLocal { get { return driver != null && driver.IsLocalPlayer; } }

        void Awake()
        {
            vehicle = GetComponent<VehicleController>();
            driver = FindOn<VehicleDriver>();
            health = FindOn<VehicleHealth>();
            boost = FindOn<VehicleBoost>();
            sensor = FindOn<VehicleImpactSensor>();
            recovery = FindOn<VehicleRecovery>();
            ResolveEngineIds();
        }

        // Per-vehicle engine sound: the car's prefab name is matched to a VehicleDefinition listed in the bank.
        // Done once at attach time. Unlisted vehicles, or empty ids, keep the defaults.
        void ResolveEngineIds()
        {
            AudioManager am = AudioManager.Instance;
            if (am == null || am.Bank == null) return;

            string prefabName = gameObject.name;
            if (prefabName.EndsWith("(Clone)")) prefabName = prefabName.Substring(0, prefabName.Length - 7).TrimEnd();

            string playerId, otherId;
            if (!am.Bank.TryGetVehicleEngine(prefabName, out playerId, out otherId)) return;
            if (!string.IsNullOrEmpty(playerId)) enginePlayerId = playerId;
            if (!string.IsNullOrEmpty(otherId)) engineOtherId = otherId;
        }

        // Looked up once at attach time, never per frame.
        T FindOn<T>() where T : Component
        {
            T c = GetComponent<T>();
            if (c == null) c = GetComponentInChildren<T>(true);
            if (c == null) c = GetComponentInParent<T>();
            return c;
        }

        void OnEnable()
        {
            if (sensor != null) sensor.Impact += OnImpact;
            if (health != null)
            {
                health.Destroyed += OnDestroyed;
                health.Respawned += OnRespawned;
            }
            if (boost != null)
            {
                boost.BoostStarted += OnBoostStarted;
                boost.BoostEnded += OnBoostEnded;
            }
            if (recovery != null) recovery.Recovered += OnRecovered;
        }

        void OnDisable()
        {
            if (sensor != null) sensor.Impact -= OnImpact;
            if (health != null)
            {
                health.Destroyed -= OnDestroyed;
                health.Respawned -= OnRespawned;
            }
            if (boost != null)
            {
                boost.BoostStarted -= OnBoostStarted;
                boost.BoostEnded -= OnBoostEnded;
            }
            if (recovery != null) recovery.Recovered -= OnRecovered;

            AudioManager am = AudioManager.Instance;
            if (am != null)
            {
                am.ReleaseLoop(engine);
                am.ReleaseLoop(skid);
            }
            engine = null;
            skid = null;
            engineLevel = 0f;
            skidLevel = 0f;
        }

        // ───── One-shot events ─────

        void PlayHere(string id)
        {
            AudioManager am = AudioManager.Instance;
            if (am == null) return;
            if (IsLocal) am.Play(id);
            else am.Play(id, transform.position);
        }

        void OnImpact(ImpactInfo info)
        {
            AudioManager am = AudioManager.Instance;
            if (am == null) return;
            AudioBank bank = am.Bank;

            if (info.other != null)
            {
                TrafficVehicle traffic = info.other.GetComponentInParent<TrafficVehicle>();
                if (traffic != null) OnTrafficHit(am, traffic, info);
            }

            float ratio = info.speedRatio;
            if (ratio < bank.impactMinRatio) return;
            string id = ratio >= bank.impactHardRatio ? AudioIds.ImpactHard : AudioIds.ImpactLight;

            if (IsLocal) am.Play(id);
            else am.Play(id, info.point == Vector3.zero ? transform.position : info.point);
        }

        // TrafficVehicle.Knocked is a per-vehicle event with no static hook, so the car that hits it reports the knock
        // (same speed rule as TrafficVehicle: impact faster than its knockSpeed). One sound per vehicle per second.
        void OnTrafficHit(AudioManager am, TrafficVehicle traffic, ImpactInfo info)
        {
            if (info.impactSpeed < traffic.knockSpeed) return;
            float now = Time.unscaledTime;
            int id = traffic.GetInstanceID();
            if (id == lastKnockId && now - lastKnockTime < 1f) return;
            lastKnockId = id;
            lastKnockTime = now;
            am.Play(AudioIds.TrafficKnock, traffic.transform.position);
        }

        void OnDestroyed(ImpactInfo info) { PlayHere(AudioIds.Destroyed); }
        void OnRespawned() { PlayHere(AudioIds.Respawn); }
        void OnBoostStarted() { PlayHere(AudioIds.BoostStart); }
        void OnBoostEnded() { PlayHere(AudioIds.BoostEnd); }
        void OnRecovered() { PlayHere(AudioIds.Recover); }

        // ───── Loops ─────

        void Update()
        {
            AudioManager am = AudioManager.Instance;
            if (am == null || vehicle == null) return;
            AudioBank bank = am.Bank;

            bool local = IsLocal;
            float now = Time.unscaledTime;
            float dt = Time.unscaledDeltaTime;

            CheckPickups(am, now);

            bool silent = GameState.IsPaused || (health != null && health.IsDestroyed);

            UpdateEngine(am, bank, local, silent, now, dt);
            UpdateSkid(am, local, silent, now, dt);
        }

        // Pickup.Collected is a per-pickup event with no static hook, so a pickup is heard as its effect:
        // the car's battery count or repair count going up during a match. Read-only; no subscription needed.
        void CheckPickups(AudioManager am, float now)
        {
            int batteries = boost != null ? boost.Batteries : 0;
            int repairs = recovery != null ? recovery.ManualResetsLeft : 0;

            MatchAudio match = am.MatchSounds;
            bool allowed = match == null || !match.HasMatch || (match.IsRunning && now - match.RunningSince > 0.5f);

            if (allowed && (batteries > lastBatteries || repairs > lastRepairs)) PlayHere(AudioIds.Pickup);

            lastBatteries = batteries;
            lastRepairs = repairs;
        }

        void UpdateEngine(AudioManager am, AudioBank bank, bool local, bool silent, float now, float dt)
        {
            // The local player may be assigned after this component starts: switch loop when that changes.
            if (engine != null && engineIsLocal != local)
            {
                am.ReleaseLoop(engine);
                engine = null;
            }

            Vector3 listenerPos;
            bool haveListener = am.TryGetListenerPosition(out listenerPos);

            if (engine == null)
            {
                if (now < nextEngineTry) return;
                nextEngineTry = now + 0.5f;

                if (local)
                {
                    engine = am.AcquireLoop(enginePlayerId, false, false);
                    if (engine == null && enginePlayerId != AudioIds.EnginePlayer)
                        engine = am.AcquireLoop(AudioIds.EnginePlayer, false, false); // its own entry has no clip yet
                }
                else
                {
                    float range = bank.otherEngineRange;
                    bool near = !haveListener || (transform.position - listenerPos).sqrMagnitude <= range * range;
                    if (near)
                    {
                        engine = am.AcquireLoop(engineOtherId, true, true);
                        if (engine == null && engineOtherId != AudioIds.EngineOther)
                            engine = am.AcquireLoop(AudioIds.EngineOther, true, true);
                    }
                }
                engineIsLocal = local;
                if (engine == null) return;
            }
            else if (!local && haveListener)
            {
                float range = bank.otherEngineRange * 1.25f; // a little hysteresis so it does not flicker at the edge
                if ((transform.position - listenerPos).sqrMagnitude > range * range)
                {
                    am.ReleaseLoop(engine);
                    engine = null;
                    return;
                }
            }

            float refSpeed = bank.engineReferenceSpeed < 0.1f ? 0.1f : bank.engineReferenceSpeed;
            float speed01 = Mathf.Clamp01(Mathf.Abs(vehicle.ForwardSpeed) / refSpeed);
            float throttle = Mathf.Clamp01(Mathf.Abs(vehicle.ThrottleInput));

            float targetPitch01 = Mathf.Clamp01(speed01 + throttle * 0.15f);
            float targetLevel = silent ? 0f : Mathf.Lerp(bank.engineIdleVolume, 1f, Mathf.Clamp01(speed01 * 0.7f + throttle * 0.3f));

            float k = 1f - Mathf.Exp(-bank.engineResponse * dt);
            pitch01 += (targetPitch01 - pitch01) * k;
            engineLevel += (targetLevel - engineLevel) * k;

            if (!local) engine.SetPosition(transform.position);
            engine.Set(engineLevel, Mathf.Lerp(engine.PitchMin, engine.PitchMax, pitch01));
        }

        void UpdateSkid(AudioManager am, bool local, bool silent, float now, float dt)
        {
            if (!local)
            {
                if (skid != null)
                {
                    am.ReleaseLoop(skid);
                    skid = null;
                }
                return;
            }

            if (skid == null)
            {
                if (now < nextSkidTry) return;
                nextSkidTry = now + 1f;
                skid = am.AcquireLoop(AudioIds.SkidLoop, false, false);
                if (skid == null) return;
            }

            float refSpeed = am.Bank.engineReferenceSpeed < 0.1f ? 0.1f : am.Bank.engineReferenceSpeed;
            float speed01 = Mathf.Clamp01(Mathf.Abs(vehicle.ForwardSpeed) / refSpeed);
            float target = (silent || !vehicle.IsGrounded) ? 0f : Mathf.Clamp01(vehicle.HandbrakeAmount) * speed01;

            float k = 1f - Mathf.Exp(-12f * dt);
            skidLevel += (target - skidLevel) * k;
            skid.Set(skidLevel, 1f);
        }
    }
}
