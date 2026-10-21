
# Economy Telemetry Map – Whopper

| Event | Parameters (type) | Fires when | Question it answers |
|-------|-------------------|------------|---------------------|
| `session_start` | `app_version` (string), `device_model` (string), `android` (string) | first frame of Boot | which build and device produced this log |
| `level_start` | `level_id` (int), `attempt` (int), `coins` (int) | Game scene starts | how many retries; entry wealth |
| `level_complete` | `level_id` (int), `time_s` (float), `coins_earned` (int), `score` (int) | win state | run length and earn rate on success |
| `level_fail` | `level_id` (int), `time_s` (float), `cause` (id), `coins_earned` (int) | lose state | where and why players die; earn rate on failure |
| `upgrade_purchased` | `upgrade_id` (id), `level` (int), `cost` (int), `coins_left` (int) | inside `TryUpgrade` on success | when the first upgrade lands; spend pacing |

The data is written to logcat and to `telemetry.log` in `Application.persistentDataPath` on the phone.
Nothing leaves the device, and the same statement is in `docs/CA1/privacy-statement.md`