# Week 7, Lab A (2h): Adaptive performance and the frame-rate toggle

**Module:** Mobile Game Development (A12581) · **Engine:** Unity 6.6 · **Platform:** Android (sideload) · **Date:** Mon 19 Oct 2026
**Feeds:** CA2 Playable Vertical Slice (due Sun 15 Nov 2026, 23:59) · **LOs:** LO1, LO3

> **Goal:** Integrate the Unity Adaptive Performance package with its Android provider, log the thermal and bottleneck events it raises, and ship a Balanced / High FPS toggle that changes `Application.targetFrameRate` and the URP render scale.

## What you will complete today
1. Adaptive Performance installed, the Android provider enabled, and `[AP]` lines visible in logcat from your phone.
2. A bootstrap script that subscribes to `ThermalEvent` and `PerformanceBottleneckChangeEvent` and logs each one.
3. `PerformanceMode.cs` plus a UI toggle (Balanced / High FPS) that persists in `PlayerPrefs`.
4. A release build on your phone with screenshots proving both modes change the frame rate and render scale.
5. A `perf-modes.md` entry in your CA2 Baseline Sheet folder.

## Pre-flight
- Your Week 6 build runs on your phone; telemetry events (`session_start` and friends) appear in `adb logcat -s Unity`.
- Unity 6.6 with Android Build Support; USB debugging enabled; `adb devices` lists your phone.
- URP project with at least one URP Asset assigned (**Edit > Project Settings > Graphics**).
- Your FPS overlay from Week 3 (or the Profiler attached to the device) so you can read frame time on the phone.
- Phone charged above 50% and unplugged for the verification step.

## Part A: Install and enable the provider (~15 min)
1. **Window > Package Manager**, switch the dropdown to **Unity Registry**, search *Adaptive Performance*, click **Install**.
2. **Edit > Project Settings > Adaptive Performance**. On the **Android** tab tick **Android Provider**. Unity downloads `com.unity.adaptiveperformance.google.android` automatically.
3. In the same settings panel:
   - **Initialize Adaptive Performance on Startup**: on.
   - **Logging**: on for this lab (switch it off before the CA2 build).
   - **Indexer Settings > Active**: on. Leave the Thermal and Performance action delays at their defaults.
   - **Scaler Settings**: leave every scaler off today. You add scalers only after you can read the events.
4. Optional Editor test: **Window > General > Device Simulator**. With the **Simulator** provider ticked on the desktop tab, an **Adaptive Performance** panel lets you drag the thermal level and see events without a phone.

*Expected result: the Console shows "[Adaptive Performance] ... provider loaded" on Play in the Editor (simulator) and, later, on the phone.*

## Part B: Log thermal and bottleneck events (~15 min)
1. Create `Assets/Scripts/Performance/AdaptiveBootstrap.cs` and attach it to a persistent object in your boot scene (the same object that carries your telemetry logger).

```csharp
using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class AdaptiveBootstrap : MonoBehaviour
{
    IAdaptivePerformance ap;

    void Start()
    {
        ap = Holder.Instance;
        if (ap == null || !ap.Active)
        {
            Debug.Log("[AP] provider inactive on this device");
            return;
        }
        ap.ThermalStatus.ThermalEvent += OnThermal;
        ap.PerformanceStatus.PerformanceBottleneckChangeEvent += OnBottleneck;
        var m = ap.ThermalStatus.ThermalMetrics;
        Debug.Log($"[AP] active warning={m.WarningLevel} level={m.TemperatureLevel:0.00}");
    }

    void OnDestroy()
    {
        if (ap == null) return;
        ap.ThermalStatus.ThermalEvent -= OnThermal;
        ap.PerformanceStatus.PerformanceBottleneckChangeEvent -= OnBottleneck;
    }

    void OnThermal(ThermalMetrics m)
    {
        Debug.Log($"[AP] thermal warning={m.WarningLevel} " +
                  $"level={m.TemperatureLevel:0.00} trend={m.TemperatureTrend:0.00}");
    }

    void OnBottleneck(PerformanceBottleneckChangeEventArgs e)
    {
        Debug.Log($"[AP] bottleneck={e.PerformanceBottleneck}");
    }
}
```

2. Build and run once (**File > Build Profiles > Android > Build And Run**, Development Build ticked for now) and watch the phone:

```
adb logcat -s Unity | findstr "[AP]"
```

*Expected result: an `[AP] active ...` line at start-up. Thermal events only fire when the level changes, so you may see none until Part D; the bottleneck event usually fires within a few seconds (`TargetFrameRate` when you are hitting your cap).*

## Part C: Build the Balanced / High FPS toggle (~30 min)
1. Create `Assets/Scripts/Performance/PerformanceMode.cs` with the deck code (Part 4 slide). Add this convenience method below `Apply` so a uGUI Toggle can call it directly:

```csharp
    public void SetHighFps(bool on) =>
        Apply(on ? PerfMode.HighFps : PerfMode.Balanced);
```

2. Attach `PerformanceMode` to the same persistent boot object as `AdaptiveBootstrap`.
3. In your pause or settings menu (not the HUD), add **GameObject > UI > Toggle**. Label it *High FPS*. Touch target at least 48 dp: set the Toggle's RectTransform height to 64 px at your reference resolution.
4. On the Toggle's **On Value Changed (Boolean)** event, drag the boot object in and pick **PerformanceMode > SetHighFps** from the *Dynamic bool* section at the top of the list, not the static one.
5. When the menu opens, initialise the toggle without firing the event:

```csharp
    toggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("perf_mode", 0) == 1);
```

6. Check how many URP Assets you have. If **Edit > Project Settings > Quality** assigns a different URP Asset per quality level, `UniversalRenderPipeline.asset` returns the active one, which is what you want. Setting `renderScale` on it changes the asset in the Editor too, so reset it to 1.0 before committing.

*Expected result: in the Editor, flipping the toggle prints `perf_mode=HighFps high=True` and the Game view resolution changes if you enabled Stats.*

## Part D: Build, sideload, verify both modes (~15 min)
1. Build a release-signed APK (Development Build off if you are using an FPS overlay; keep it on if you need the Profiler) and install:

```
adb install -r Builds/MyGame-0.7.0-arm64.apk
```

2. Unplug the phone. Open the menu, leave the toggle off (Balanced). Read frame time on the overlay or in the Profiler for 30 seconds. Screenshot with the overlay visible.
3. Turn the toggle on (High FPS). Read again for 30 seconds. Screenshot.
4. In logcat, confirm the `perf_mode=` line for each flip and note any `[AP] bottleneck=` change: High FPS with render scale 0.8 often moves you from `GPU` to `TargetFrameRate` or `CPU`.
5. Check what the panel actually does. On a 60 Hz phone High FPS reads 60 with a lower render scale. On a 90 or 120 Hz phone you should see the rate climb; if it does not, check the system **Game Mode** (Battery mode caps FPS) and the Android display refresh setting.

*Expected result: two screenshots showing different frame times or resolutions, and a logcat excerpt with both `perf_mode=` lines.*

## Part E: Record and commit (~10 min)
1. Create `/docs/CA2/baseline/perf-modes.md` with this table, filled in for your phone:

| Mode | targetFrameRate | Render scale | Frame time avg (ms) | 99th (ms) | Bottleneck | Panel Hz |
|------|-----------------|--------------|---------------------|-----------|------------|----------|
| Balanced (default) | 60 | 1.0 | | | | |
| High FPS | 120 | 0.8 | | | | |

2. Save the two screenshots as `/docs/CA2/baseline/perf-modes/balanced.png` and `high-fps.png`, and the logcat excerpt as `/docs/CA2/baseline/logcat-ap-events.txt`.
3. Switch **Logging** off in the Adaptive Performance settings if you are about to make a CA2 candidate build; otherwise leave it on for Lab B.

## Troubleshooting
- **Adaptive Performance does not appear in Package Manager** -> the dropdown is on *In Project*; switch to *Unity Registry* and clear the search box.
- **`Holder.Instance` is null on the phone** -> *Initialize on Startup* is unticked, or the Android Provider is not ticked on the Android tab. Re-check both, then rebuild (a settings change needs a new build).
- **`ap.Active` is false** -> Android below 11 has no thermal headroom API; the provider loads but reports nothing. Note it in the Baseline Sheet and rely on the Profiler.
- **No thermal events after ten minutes** -> the phone is cool and the level has not changed. Events fire on change only; poll `ThermalMetrics` if you need a value now.
- **Render scale change not visible** -> you edited a URP Asset that is not the active one. Print `UniversalRenderPipeline.asset.name` and check *Quality* assignments.
- **High FPS still shows 60 on a 120 Hz phone** -> system Game Mode is on Battery, or the display is set to *Standard* refresh. Change both in Android settings and try again.
- **Toggle fires `Apply` on menu open** -> you set `isOn` instead of `SetIsOnWithoutNotify`.

## Checkpoints to commit
1. `Assets/Scripts/Performance/AdaptiveBootstrap.cs` and `PerformanceMode.cs`.
2. The settings menu scene or prefab with the High FPS toggle.
3. `/docs/CA2/baseline/perf-modes.md` with the table filled in.
4. `/docs/CA2/baseline/perf-modes/balanced.png`, `high-fps.png`, `/docs/CA2/baseline/logcat-ap-events.txt`.
5. `ProjectSettings/AdaptivePerformance*.asset` files (they are generated by the package; commit them so the provider setting travels with the repo).

## Exit ticket
Post one logcat line showing a thermal or bottleneck event from your phone, and a screenshot of the toggle in each mode, in the Moodle forum.

## Reference links
- Adaptive Performance package manual: https://docs.unity3d.com/Packages/com.unity.adaptiveperformance@latest
- Unity and ADPF (Android developer guide): https://developer.android.com/games/engines/unity/unity-adpf
- ADPF overview: https://developer.android.com/games/optimize/adpf
- ADPF thermal API: https://developer.android.com/games/optimize/adpf/thermal
- ADPF performance hint API: https://developer.android.com/games/optimize/adpf/performance-hint-api
- Game Mode FPS throttling: https://developer.android.com/games/optimize/adpf/gamemode/fps-throttling
- `Application.targetFrameRate`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-targetFrameRate.html
- `ScalableBufferManager`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/ScalableBufferManager.html
- URP Asset (render scale): https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest
- Display refresh rate on Android: https://developer.android.com/games/optimize/display-refresh-rate-change
- Unity 6.6 manual index: https://docs.unity3d.com/6000.6/Documentation/Manual/index.html
