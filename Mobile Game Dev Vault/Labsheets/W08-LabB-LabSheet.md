# Week 8, Lab B (2h): QA evidence: logcat, ANRs and a test plan

**Module:** Mobile Game Development (A12581) · **Engine:** Unity 6.6 · **Platform:** Android (sideload) · **Date:** Wed 4 Nov 2026
**Feeds:** CA3 Performance & Quality Dossier + Final Build (due Fri 4 Dec 2026); Baseline Sheet numbers also go into CA2 (due Sun 15 Nov 2026) · **LOs:** LO2, LO4

> **Goal:** Measure fps stability, memory and start-up time on your own phone, learn to filter logcat for ANRs, exceptions and kills, and run the six-scenario resilience test plan with evidence per row.

## What you will complete today
1. PSS memory, cold-start and warm-start times recorded in the Baseline Sheet with the build ID.
2. A saved logcat capture of the whole test run in `/docs/CA3/qa/`.
3. The six-scenario test plan filled in: resume, no network, rotation, battery saver, low RAM, cold and warm start.
4. The worst failure either fixed or filed in `KnownIssues.md` with a repro and a logcat line.

## Pre-flight
- Build 0.8.0 (or newer) from Lab A installed on your phone, with the build stamp on the About screen.
- Two terminals open with `adb` on the path; `adb devices` lists the phone.
- Your package name to hand (**Edit > Project Settings > Player > Android > Other Settings > Package Name**); replace `com.setu.mygame` below with it.
- Android Logcat package installed in Unity (**Package Manager > Unity Registry > Android Logcat**) as a second viewer.
- Save file from a previous session present on the phone (play once and quit) so resume tests have state to restore.

## Part A: Measure memory and start times (~10 min)
1. Find your launch activity name (Unity 6 uses `UnityPlayerActivity` unless you chose GameActivity in Player Settings):

```
adb shell cmd package resolve-activity --brief com.setu.mygame
```

2. **Cold start**: force-stop, then launch with timing. `TotalTime` is milliseconds from intent to first frame; add your own loading time to interactive from the telemetry `level_start` timestamp.

```
adb shell am force-stop com.setu.mygame
adb shell am start -W com.setu.mygame/com.unity3d.player.UnityPlayerActivity
```

3. **Warm start**: press Home on the phone, wait ten seconds, run the same `am start -W` line. Note `TotalTime` again.
4. **Memory**: play for five minutes in your busiest scene, then:

```
adb shell dumpsys meminfo com.setu.mygame | findstr "TOTAL PSS"
```

5. Write all three into the Baseline Sheet next to the Week 3 numbers, with the build ID and date.

*Expected result: cold start noticeably longer than warm; PSS in MB well under the phone's RAM with headroom for the OS.*

## Part B: Set up the logcat capture (~15 min)
1. Terminal 1 records everything for the evidence pack:

```
adb logcat -c
adb logcat -v time > docs/CA3/qa/logcat-w08-testplan.txt
```

2. Terminal 2 watches only what matters, live. Start the app first so `pidof` returns a value:

```
adb logcat -s ActivityManager:E AndroidRuntime:E DEBUG:F Unity:E
```

   or, to see everything from your process only (Android 7 and later):

```
adb logcat --pid=$(adb shell pidof com.setu.mygame)
```

3. Learn the three signatures before you provoke them:
   - **ANR**: `ActivityManager: ANR in com.setu.mygame` followed by the reason (`Input dispatching timed out`). Main thread blocked 5 s.
   - **Managed exception**: `Unity   : NullReferenceException: ...` then stack lines with `at ...`. IL2CPP carries on; state may be corrupt.
   - **Native crash**: `DEBUG   : Fatal signal 11 (SIGSEGV)` then a tombstone. Low-memory kills show no crash at all: the process simply ends and `ActivityManager` logs a kill.
4. On Windows, `findstr /i "exception error"` replaces `grep -iE "exception|error"` when you want to filter the Unity tag.

*Expected result: two terminals, one silent file recorder and one live filter; the live filter shows nothing while the game idles cleanly.*

## Part C: Run the six scenarios (~40 min)
Create `/docs/CA3/qa/test-plan.md` with this table and fill one row per scenario as you go. Evidence is one logcat line or one screenshot per row.

| # | Scenario | Trigger | Expected | Pass/Fail | Evidence |
|---|----------|---------|----------|-----------|----------|
| 1 | Resume / background | Home, wait 30 s, relaunch; then force-stop and relaunch | Paused, audio silent, resumes in place; save intact after force-stop | | |
| 2 | No network | Airplane mode on before launch, and again mid-play | No hang, no unhandled exception; online features fail quietly | | |
| 3 | Rotation | Rotate mid-play, or verify the locked orientation holds | Safe areas respected; no stretched UI; no restart | | |
| 4 | Battery saver | Enable, play 2 minutes | Frame cap respected; playable; overlay shows the change | | |
| 5 | Low RAM | Trim memory, background, return | Returns without a black screen; if killed, relaunch restores the save | | |
| 6 | Cold and warm start | Part A commands | TotalTime recorded; loading screen never hangs | | |

Triggers from the terminal (undo each one afterwards):

```
adb shell input keyevent KEYCODE_HOME
adb shell cmd connectivity airplane-mode enable
adb shell cmd connectivity airplane-mode disable
adb shell settings put system accelerometer_rotation 0
adb shell settings put system user_rotation 1
adb shell settings put system user_rotation 0
adb shell settings put global low_power 1
adb shell settings put global low_power 0
adb shell am send-trim-memory com.setu.mygame RUNNING_CRITICAL
```

Notes per scenario:
1. **Resume**: do the 30-second Home cycle ten times in a row; the CA2 checklist asks for ten with no soft lock. Then `am force-stop` and relaunch: the save must load.
2. **No network**: if your game never touches the network this is a one-line pass; write it down anyway.
3. **Rotation**: if you locked orientation in Player Settings, confirm the phone's auto-rotate does not override it; screenshot the safe-area layout.
4. **Battery saver**: on some OEM builds `low_power 1` does not engage the system UI; toggle Battery Saver in the phone's quick settings instead and note it.
5. **Low RAM**: after `send-trim-memory`, background the app, open the camera and a browser for a minute, return. Watch Terminal 2 for a kill line.
6. **Cold and warm start**: already measured in Part A; paste the two `TotalTime` values.

*Expected result: six rows with a pass or fail each and a piece of evidence; at least one row usually fails on the first run, and that is the point.*

## Part D: Fix or file the worst failure (~15 min)
1. Pick the failure with the highest player impact (a soft lock beats a layout glitch).
2. If you can fix it in under 15 minutes (a missing `OnApplicationPause` branch, a null check, a `LoadSceneAsync` instead of `LoadScene`), fix it, rebuild, re-run that one row.
3. Otherwise file it in `KnownIssues.md` with: scenario, steps to reproduce, expected versus actual, the logcat line, build ID, device.

*Expected result: either a green row that was red, or a Known Issue a stranger could reproduce.*

## Part E: Commit the evidence (~10 min)
1. Stop Terminal 1 (Ctrl+C). Confirm the file has content and starts with the timestamp of your first scenario.
2. Undo settings on the phone: `low_power 0`, airplane mode disable, `accelerometer_rotation 1`.
3. Commit `test-plan.md`, the logcat file, screenshots, `KnownIssues.md` and the updated Baseline Sheet.

## Troubleshooting
- **`pidof` returns nothing** -> the app is not running, or the package name is wrong; check with `adb shell pm list packages | findstr setu`.
- **`am start -W` says "Error type 3: Activity class does not exist"** -> wrong activity; use the name from `resolve-activity --brief` (GameActivity builds use `UnityPlayerGameActivity`).
- **Logcat file is empty** -> the redirect was created in a shell that did not have `adb` on the path, or you ran `-c` after starting the recorder. Run `-c` first, then the recorder.
- **`cmd connectivity` not found** -> Android below 11; use `adb shell settings put global airplane_mode_on 1` followed by `adb shell am broadcast -a android.intent.action.AIRPLANE_MODE`.
- **Rotation does nothing** -> `accelerometer_rotation` is still 1 (auto). Set it to 0 first, then `user_rotation`.
- **Exceptions appear only in the release build** -> a Development Build hides ordering bugs behind slower frames; test on release builds and use `Debug.Log` breadcrumbs around the failing call.

## Checkpoints to commit
1. `/docs/CA3/qa/test-plan.md` with six rows complete.
2. `/docs/CA3/qa/logcat-w08-testplan.txt`.
3. `/docs/CA3/qa/screens/` with one screenshot per scenario that needed one.
4. `KnownIssues.md` at the repo root, updated.
5. `/docs/CA2/baseline/Performance_Baseline.md` with PSS, cold start and warm start rows.

## Exit ticket
Post your six-row test plan table with pass or fail, and one logcat line you filtered, in the Moodle forum.

## Reference links
- ANRs (Android vitals): https://developer.android.com/topic/performance/vitals/anr
- App start-up time: https://developer.android.com/topic/performance/vitals/launch-time
- `logcat` command-line tool: https://developer.android.com/tools/logcat
- `adb` reference: https://developer.android.com/tools/adb
- `dumpsys` reference: https://developer.android.com/tools/dumpsys
- Android Logcat package: https://docs.unity3d.com/Packages/com.unity.mobile.android-logcat@latest
- `MonoBehaviour.OnApplicationPause`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnApplicationPause.html
- `SceneManager.LoadSceneAsync`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html
- Unity 6.6 manual index: https://docs.unity3d.com/6000.6/Documentation/Manual/index.html
