# AUDIO_IDS — permanent sound ids

Ids are strings and permanent once shipped (like level ids). Add new ids at the end of a group and in `AudioIds.cs`.
An id with no clips in the bank is silent and gives no error, so a missing sound never breaks the game.
Create the bank with **Tools > Vehicle > Create Audio Bank**, drag clips into the entries, then run
**Tools > Vehicle > Apply Audio Import Settings To Bank** (mono, Vorbis; short effects Decompress On Load, loops Compressed In Memory, music Streaming).

| Id | Category | Played by | When | Notes |
| --- | --- | --- | --- | --- |
| `ui_click` | Sfx | UI | any button | UI calls `AudioManager.Instance.Play("ui_click")` |
| `ui_back` | Sfx | UI | back / close | |
| `engine_player` | Engine, loop | VehicleAudio | local car | 2D. Pitch `pitchMin`..`pitchMax` follows speed |
| `engine_other` | Engine, loop | VehicleAudio | other cars, only when close | 3D, at most `maxOtherEngines` voices |
| `skid_loop` | Sfx, loop | VehicleAudio | local car, handbrake while moving | volume = handbrake x speed |
| `impact_light` | Sfx | VehicleAudio | `VehicleImpactSensor.Impact`, speedRatio below `impactHardRatio` | |
| `impact_hard` | Sfx | VehicleAudio | same, speedRatio at or above `impactHardRatio` | |
| `destroyed` | Sfx | VehicleAudio | `VehicleHealth.Destroyed` | |
| `respawn` | Sfx | VehicleAudio | `VehicleHealth.Respawned` | |
| `recover` | Sfx | VehicleAudio | `VehicleRecovery.Recovered` | |
| `boost_start` | Sfx | VehicleAudio | `VehicleBoost.BoostStarted` | |
| `boost_end` | Sfx | VehicleAudio | `VehicleBoost.BoostEnded` | |
| `pickup` | Sfx | WorldAudio | `Pickup.Collected` | 2D for the local car, 3D for others |
| `traffic_knock` | Sfx | WorldAudio | `TrafficVehicle.Knocked` | 3D |
| `wrong_way_start` | Sfx | WorldAudio | `WrongWayChanged(true)` | needs the tracker bound |
| `wrong_way_end` | Sfx | WorldAudio | `WrongWayChanged(false)` | |
| `countdown_tick` | Sfx | MatchAudio | each whole second of the countdown | |
| `countdown_go` | Sfx | MatchAudio | state becomes Running | |
| `match_finish` | Sfx | MatchAudio | `MatchManager.Finished` | |
| `points_bump` | Sfx | MatchAudio | `ScoreReason.NpcBump`, local player | |
| `points_chain` | Sfx | MatchAudio | `ScoreReason.Chain` | |
| `points_ram` | Sfx | MatchAudio | `ScoreReason.RivalRam` | |
| `points_overtake` | Sfx | MatchAudio | `ScoreReason.Overtake` | |
| `points_crossing` | Sfx | MatchAudio | `ScoreReason.Crossing` | |
| `points_penalty` | Sfx | MatchAudio | `ScoreReason.Destroyed` | `Distance` and `WrongWay` give no chime (continuous) |

Music: `AudioManager.PlayMusic(id)` / `StopMusic()` with any entry of category Music. No music id is reserved yet.
PlayerPrefs keys: `VehicleSystem.Audio.Master`, `.Sfx`, `.Music`, `.Engine`, `.Muted`.

## Per-vehicle engines
Add entries such as `engine_cng` and `engine_cng_other` (category Engine, loop on, own clips and pitch range), then in the bank's
**Per-vehicle engines** list add the vehicle's `VehicleDefinition` and those two ids. Empty ids, unlisted vehicles, or an entry with no
clips yet all fall back to `engine_player` / `engine_other`. The car is matched to its definition by prefab name (without "(Clone)").
