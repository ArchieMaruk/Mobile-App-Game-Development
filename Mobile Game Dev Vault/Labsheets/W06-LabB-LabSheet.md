# Week 6, Lab B (2h): Telemetry hooks for balancing and Async Pack 1

**Module:** Mobile Game Development (A12581) · **Engine:** Unity 6.6 · **Platform:** Android (sideload) · **Date:** Wed 14 Oct 2026
**Feeds:** CA2 Playable Vertical Slice (due Sun 15 Nov 2026, 23:59); Async Pack 1 (due Sun 1 Nov 2026, 23:59) · **LOs:** LO2, LO3

> **Goal:** Add local telemetry (logcat and a file, no SDK) for the five events that explain your economy, write the half-page event map that CA2 requires, prove the wallet survives pause, force-stop and a low-memory kill, and take Async Pack 1 away for Reading Week.

## What you will complete today
1. `Telemetry` static class logging to `Debug.Log` and to `telemetry.log` in `Application.persistentDataPath`.
2. `session_start`, `level_start`, `level_complete`, `level_fail` and `upgrade_purchased` firing with parameters.
3. `docs/CA2/Economy_Telemetry_Map.md`, half a page.
4. Save/resume evidence: home and back, force-stop, `am kill`; the pulled log in `docs/CA2/telemetry/`.
5. Async Pack 1 brief read, due date in the journal; tag `v0.4.0-economy`.

## Pre-flight
- Lab A checkpoints: `Wallet`, `WalletSave`, `WalletPersistence`, `CoinShop`; coins survive a relaunch.
- `GameManager` state changes from Week 5 (`Playing`, `Paused`, `Won`, `Lost`).
- Phone plugged in; Git Bash for `grep`.
- Moodle open: the Async Pack 1 brief and reading links are posted there.

## Part A: Telemetry class and session_start (~20 min)
1. `Assets/Scripts/Telemetry/Telemetry.cs`:
   ```csharp
   using System.IO;
   using UnityEngine;
   public static class Telemetry
   {
       static readonly string session =
           System.Guid.NewGuid().ToString("N").Substring(0, 8);
       static readonly string path =
           Path.Combine(Application.persistentDataPath, "telemetry.log");

       public static void Log(string ev, params (string k, object v)[] p)
       {
           var sb = new System.Text.StringBuilder(128);
           sb.Append(Time.realtimeSinceStartup.ToString("F2"))
             .Append(' ').Append(session).Append(' ').Append(ev);
           foreach (var (k, v) in p)
               sb.Append(' ').Append(k).Append('=').Append(v);
           var line = sb.ToString();
           Debug.Log("[telemetry] " + line);       // logcat, today
           File.AppendAllText(path, line + "\n");  // survives relaunch
       }
   }
   ```
2. Rules: never call it from `Update` (the StringBuilder and the boxing of value parameters allocate, which is fine per event and wrong per frame). Parameters are numbers and ids, never free text. Event names are `snake_case`.
3. In `Bootstrap.Awake` (Week 5), after `targetFrameRate`:
   ```csharp
   Telemetry.Log("session_start",
       ("app_version", Application.version),
       ("device_model", SystemInfo.deviceModel),
       ("android", SystemInfo.operatingSystem));
   ```
4. Build to the phone and confirm:
   ```bash
   adb logcat -s Unity | grep telemetry
   ```
   *Expected: one `session_start` line with three parameters, a timestamp near 0.0 and an eight-character session id.*

## Part B: Wire the five events (~25 min)
1. In `GameManager`, on each state transition:
   - Entering `Playing` from the initial state: `level_start` with `level_id`, `attempt` (count relaunches of the same lane in this session), `coins` (from `Wallet`).
   - Entering `Won`: `level_complete` with `level_id`, `time_s` (`Time.time` since `level_start`), `coins_earned`, `score`.
   - Entering `Lost`: `level_fail` with `level_id`, `time_s`, `cause` (a short id such as `spike`, `timeout`, `hp`), `coins_earned`.
2. In `Wallet.TryUpgrade`, after the successful spend:
   ```csharp
   Telemetry.Log("upgrade_purchased", ("upgrade_id", "speed"),
       ("level", Level), ("cost", cost), ("coins_left", Coins));
   ```
   Capture `NextCost` into a local `cost` before `Level++`, or the logged cost will be the next one.
3. Optional if you tuned pacing by feel in Lab A: `coins_earned` with `source` and `amount` on every `Earn`. Keep it if the earn rate is a question; drop it if it floods the log.
4. Optional: `purchase_stub` with `product_id` inside `FakePurchaseProvider.PurchaseAsync`, so the stub shows up in the same timeline.
5. Play one full attempt on the phone with logcat open. Every state change must produce exactly one line.

*Expected result: `session_start`, `level_start`, then `level_complete` or `level_fail`, with `upgrade_purchased` wherever you pressed Upgrade.*

## Part C: Event map (~20 min)
1. Create `docs/CA2/Economy_Telemetry_Map.md`, half a page, one table:

   | Event | Parameters (type) | Fires when | Question it answers |
   |-------|-------------------|------------|---------------------|
   | `session_start` | `app_version` (string), `device_model` (string), `android` (string) | first frame of Boot | which build and device produced this log |
   | `level_start` | `level_id` (int), `attempt` (int), `coins` (int) | lane becomes playable | how many retries; entry wealth |
   | `level_complete` | `level_id`, `time_s` (float), `coins_earned` (int), `score` (int) | win state | run length and earn rate on success |
   | `level_fail` | `level_id`, `time_s`, `cause` (id), `coins_earned` | lose state | where and why players die; earn rate on failure |
   | `upgrade_purchased` | `upgrade_id` (id), `level` (int), `cost` (int), `coins_left` (int) | inside `TryUpgrade` on success | when the first upgrade lands; spend pacing |

2. Below the table: two sentences on where the data lives (logcat and `telemetry.log` in `persistentDataPath`), that nothing leaves the device, and that the same statement appears in `docs/CA1/privacy-statement.md`. Update the privacy statement now if it does not mention the file.
3. This map is a CA2 deliverable (`Docs/Economy_Telemetry_Map`); keep the names in the code and in the map identical.

## Part D: Save and resume proof (~20 min)
Run each step with the game on the Game scene and coins in the wallet. Record pass or fail per step in the journal.

1. Background and resume:
   ```bash
   adb shell input keyevent KEYCODE_HOME
   adb shell monkey -p com.yourname.mygame 1
   ```
   `monkey ... 1` relaunches the app and sends one random tap; use it only to bring the app back. Coins must be unchanged.
2. Force-stop and relaunch (this skips `OnApplicationQuit`; only saves made on change or on `OnApplicationPause(true)` survive):
   ```bash
   adb shell am force-stop com.yourname.mygame
   adb shell monkey -p com.yourname.mygame 1
   ```
3. Low-memory kill while backgrounded (Android does this on its own when RAM is short):
   ```bash
   adb shell input keyevent KEYCODE_HOME
   adb shell am kill com.yourname.mygame
   adb shell monkey -p com.yourname.mygame 1
   ```
4. Pull the log into the evidence folder:
   ```bash
   mkdir -p docs/CA2/telemetry
   adb pull \
     /sdcard/Android/data/com.yourname.mygame/files/telemetry.log \
     docs/CA2/telemetry/
   ```
   Open it: you should see several sessions (different session ids) and the events from Part B.
5. Any failure becomes a CA2 fix: the usual causes are saving only in `OnApplicationQuit`, or a HUD that reads its own cached value instead of `Wallet` after `Restore`.

## Part E: Pack 1 and journal (~10 min)
1. Open the **Async Pack 1: Monetisation Models and Ethics** brief on Moodle. Time: 1 to 1.5 hours, no coding. Read the linked pages (Play monetisation policy including odds disclosure for randomised items, one ratings-board note on loot boxes, one dark-patterns overview). Write a one-page reflection, then five bullets mapping your own economy loop to ethical patterns: what you do, what you avoid, why. Optional: an economy diagram with the telemetry events marked on it.
2. **Due Sun 1 Nov 2026, 23:59**, the end of Reading Week (26 to 30 Oct 2026, no classes; the October bank holiday on Mon 26 Oct falls inside it). Not graded on its own; it feeds Week 9 tuning and the CA3 dossier. Write the date in the journal now.
3. Journal: the five events, the save/resume results, the Pack 1 due date.
4. Commit and tag `v0.4.0-economy`.

## Troubleshooting
- **`telemetry.log` never appears** -> `persistentDataPath` on Android is `/storage/emulated/0/Android/data/<package>/files`; log the path once with `Debug.Log(Application.persistentDataPath)` and use exactly that in `adb pull`.
- **`adb pull` is refused (Permission denied)** -> some devices restrict `Android/data` for the shell user; read the path from logcat and try `adb shell cat <path>` or, on a Development Build, `adb shell run-as com.yourname.mygame`.
- **Two `level_start` lines per attempt** -> the state change fires twice (Retry plus scene load); log inside the state setter, guarded by `if (state == value) return;`.
- **`upgrade_purchased` shows the next cost** -> you read `NextCost` after `Level++`; capture it first.
- **Coins survive force-stop but not `am kill`** -> the last `Earn` happened after the last `OnApplicationPause(true)`; save on `Changed` as well, as in Lab A.
- **Log file grows without limit** -> fine for the semester; note it in the privacy statement, and truncate on `session_start` if it bothers you.

## Checkpoints to commit
1. `Assets/Scripts/Telemetry/Telemetry.cs` and the five call sites.
2. `docs/CA2/Economy_Telemetry_Map.md`.
3. `docs/CA2/telemetry/telemetry.log` (pulled from the device).
4. `docs/CA1/privacy-statement.md` updated for the telemetry file.
5. `docs/dev-journal.md`: save/resume results, Pack 1 due date Sun 1 Nov 2026; tag `v0.4.0-economy`.

## Exit ticket
Show a logcat line for upgrade_purchased with its parameters and the coin count surviving a force-stop; write the Pack 1 due date, Sun 1 Nov 2026, in your journal.

## Reference links
- Unity 6.6: `Application.persistentDataPath`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-persistentDataPath.html
- Unity 6.6: `MonoBehaviour.OnApplicationPause`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnApplicationPause.html
- Unity 6.6: `SystemInfo`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SystemInfo.html
- Unity 6.6: Android Logcat package: https://docs.unity3d.com/Packages/com.unity.mobile.android-logcat@latest
- Android: `adb` shell commands (`am`, `pm`, `input`, `monkey`): https://developer.android.com/tools/adb
- Android: UI/Application Exerciser Monkey: https://developer.android.com/studio/test/other-testing-tools/monkey
- Android: processes and app lifecycle (why `OnApplicationQuit` is unreliable): https://developer.android.com/guide/components/activities/process-lifecycle
- Google Play developer policy centre (Monetisation and Ads; read for Async Pack 1): https://play.google.com/about/developer-content-policy/
