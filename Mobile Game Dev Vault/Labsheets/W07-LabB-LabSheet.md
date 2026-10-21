# Week 7, Lab B (2h): Thermal soak: evidence of sustained performance

**Module:** Mobile Game Development (A12581) · **Engine:** Unity 6.6 · **Platform:** Android (sideload) · **Date:** Wed 21 Oct 2026
**Feeds:** CA2 Playable Vertical Slice (due Sun 15 Nov 2026, 23:59) · **LOs:** LO1, LO3

> **Goal:** Run a 10-minute thermal soak of your slice on your own phone, capture temperature, throttling and frame time before and after, and write the evidence into the CA2 Baseline Sheet. Leave knowing what Reading Week and Async Pack 1 require.

## What you will complete today
1. `ThermalOverlay.cs` showing frame time and Adaptive Performance thermal metrics on the phone.
2. A "before" capture: battery temperature and 60 seconds of Profiler frame-time data.
3. A 10-minute soak in Balanced mode with overlay screenshots at minutes 0, 5 and 10 and logcat recording.
4. An "after" capture and the first throttling timestamp.
5. The thermal section of your Baseline Sheet filled in and committed.

## Pre-flight
- Lab A build on your phone with the Adaptive Performance provider active and the Balanced / High FPS toggle working.
- Adaptive Performance **Logging** still on; Development Build on so the Profiler can attach.
- TextMeshPro in the project (**Window > TextMeshPro > Import TMP Essential Resources** if the dialog has never appeared).
- Profile Analyzer package installed (**Package Manager > Unity Registry > Profile Analyzer**) for percentiles.
- Phone at 60% or more, unplugged, all other apps closed, brightness fixed at about 50%, case off if it has one.

## Part A: Add the overlay and build (~10 min)
1. In your HUD canvas add **GameObject > UI > Text - TextMeshPro**, anchor it top-left inside the safe area, font size 20, white with a dark outline. Name it `ThermalLabel`.
2. Create `Assets/Scripts/Performance/ThermalOverlay.cs` with the deck code (Part 3 slide) and drag `ThermalLabel` into its `label` field.
3. Guard it behind a debug flag if you like, but keep it in the build until CA3: markers need it during the showcase.
4. **File > Build Profiles > Android > Build**, Development Build on, Autoconnect Profiler on. Install with `adb install -r`.

*Expected result: the phone shows something like `16.8 ms  NoWarning` and `temp 0.32  trend 0.00`. If it shows `(AP inactive)`, see Troubleshooting and continue with the Profiler only.*

## Part B: Capture "before" (~15 min)
1. With the game closed, read the battery temperature (tenths of a degree Celsius, so `312` means 31.2 C):

```
adb shell dumpsys battery | findstr temperature
```

2. Write it into the sheet as **start temperature**, with the time.
3. Launch the game. **Window > Analysis > Profiler**, pick your phone in the target dropdown (Autoconnect should have done it). Enable only **CPU Usage** and **Rendering**; disable the rest to keep overhead low.
4. Play the main scene along a fixed route for 60 seconds while recording. Stop recording and **Save** the capture as `docs/CA2/baseline/thermal-soak/before.data`.
5. Open **Window > Analysis > Profile Analyzer**, pull the current Profiler data in, and read the frame time **Mean** and the **99th percentile** (the Frame Summary panel shows both).
6. Screenshot the overlay: this is your minute-0 shot.

*Expected result: a before row with temperature, average and 99th-percentile frame time, and the overlay level. Typical phones sit well inside 16.7 ms here; that is the honeymoon.*

## Part C: Soak: 10 minutes in Balanced mode (~35 min)
1. Start a logcat recording in a terminal and leave it running for the whole soak:

```
adb logcat -c
adb logcat -v time -s Unity > docs/CA2/baseline/thermal-soak/logcat-soak.txt
```

2. Confirm the toggle is on **Balanced** (default). Start a timer. Play continuously along the same route; do not open menus for more than a few seconds.
3. At **minute 5** take a screenshot of the overlay (phone screenshot, not the Editor). Keep playing.
4. At **minute 10** take the final screenshot and stop playing but leave the app in the foreground.
5. If the phone throttles hard (the overlay reads `Throttling`, or frame time doubles), note the minute. Do not stop early: the collapse is the evidence.
6. Only if you finish with more than 15 minutes of lab time left: let the phone cool for five minutes, then repeat with **High FPS** on. Otherwise High FPS is a Reading Week task.

*Expected result: three overlay screenshots, a running logcat file, and a note of the first minute at which `WarningLevel` left `NoWarning`.*

## Part D: Capture "after" (~15 min)
1. Straight after minute 10, read the temperature again with the same `dumpsys battery` command. Write the **10-minute delta** (end minus start).
2. Attach the Profiler again and record 60 seconds along the same route. Save as `after.data`. Read Mean and 99th percentile in Profile Analyzer and use its **Compare** tab with `before.data` to get the difference per frame.
3. Stop the logcat recording (Ctrl+C). Search it for the thermal events:

```
findstr "[AP] thermal" docs/CA2/baseline/thermal-soak/logcat-soak.txt
```

4. Copy the first `ThrottlingImminent` line and the first `Throttling` line (if any) with their timestamps. If none appeared, write "not observed in 10 min" and the maximum `level` value you saw.

*Expected result: an after row that is comparable to the before row: same scene, same route, same mode.*

## Part E: Write it up and commit (~15 min)
1. Add or complete the **Thermals** section of your Baseline Sheet (`/docs/CA2/baseline/Performance_Baseline.md`):

| Field | Value |
|-------|-------|
| Build ID and mode | v0.7.0 (xxxxxxxx), Balanced |
| Start temperature (C) | |
| End temperature (C) and 10-minute delta | |
| Overlay level at minute 0 / 5 / 10 | |
| First `ThrottlingImminent` (minute, logcat time) | |
| `Throttling` observed (yes/no, minute) | |
| Frame time before: mean / 99th (ms) | |
| Frame time after: mean / 99th (ms) | |
| Conditions | unplugged, brightness 50%, case off, ambient about 21 C |

2. Save the three screenshots as `minute-00.png`, `minute-05.png`, `minute-10.png` in `/docs/CA2/baseline/thermal-soak/`.
3. Commit everything. Then open your dev journal and write two lines: what the sustained budget on your phone is, and one change you will make over Reading Week because of it.
4. Reading Week reminders, in the journal too:
   - **No classes 26 to 30 Oct 2026.** Back on **Mon 2 Nov** for Week 8 Lab A (peer distribution: bring a release build).
   - **Async Pack 1: Monetisation Models & Ethics**, due **Sun 1 Nov 2026** on Moodle. One-page reflection plus five bullets mapping your economy loop to ethical patterns.
   - **CA2 due Sun 15 Nov 2026, 23:59.** Draft the reflective journal bullets for Weeks 5 to 7 while they are fresh.

## Troubleshooting
- **Overlay shows `(AP inactive)`** -> Android below 11, or the provider is not initialised. Continue with the Profiler and `dumpsys battery`; write "AP inactive" in the sheet. The temperature and frame-time evidence still counts.
- **`dumpsys battery` prints no temperature line** -> some OEM builds hide it; use `adb shell cat /sys/class/thermal/thermal_zone0/temp` (millidegrees) and note which source you used.
- **Profiler will not connect** -> Development Build was off, or the phone and PC are on different networks. Use USB: `adb forward tcp:34999 localabstract:Unity-com.setu.mygame`, then pick *Direct connection* in the Profiler target list.
- **Frame time looks worse in the Editor than on the phone** -> you are profiling the Editor. Check the Profiler target dropdown says your device model.
- **The phone charged during the soak** -> the delta is invalid; note it and repeat over Reading Week.
- **No thermal events at all and the phone is barely warm** -> your scene is not heavy enough to be evidence of sustained load; soak in your busiest level, not the menu.

## Checkpoints to commit
1. `Assets/Scripts/Performance/ThermalOverlay.cs` and the HUD label.
2. `/docs/CA2/baseline/Performance_Baseline.md` with the Thermals section complete.
3. `/docs/CA2/baseline/thermal-soak/before.data`, `after.data`, `logcat-soak.txt`.
4. `/docs/CA2/baseline/thermal-soak/minute-00.png`, `minute-05.png`, `minute-10.png`.
5. Journal entry with the sustained budget and the Reading Week plan; Pack 1 deadline noted.

## Exit ticket
Post your 10-minute temperature delta and the 99th-percentile frame time before and after in the Moodle forum, and note Pack 1 (due Sun 1 Nov) in your journal.

## Reference links
- Adaptive Performance package manual: https://docs.unity3d.com/Packages/com.unity.adaptiveperformance@latest
- ADPF thermal API: https://developer.android.com/games/optimize/adpf/thermal
- Unity Profiler on a device: https://docs.unity3d.com/6000.6/Documentation/Manual/profiler-profiling-applications.html
- Profile Analyzer package: https://docs.unity3d.com/Packages/com.unity.performance.profile-analyzer@latest
- `dumpsys` (battery section): https://developer.android.com/tools/dumpsys
- `adb` reference: https://developer.android.com/tools/adb
- Android frame pacing: https://developer.android.com/games/sdk/frame-pacing
- Unity 6.6 manual index: https://docs.unity3d.com/6000.6/Documentation/Manual/index.html
