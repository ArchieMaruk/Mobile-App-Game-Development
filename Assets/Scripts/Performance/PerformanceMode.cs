using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum PerfMode { Balanced, HighFps }

public class PerformanceMode : MonoBehaviour
{
    const string Key = "perf_mode";

    void Start() => Apply((PerfMode)PlayerPrefs.GetInt(Key, 0));

    public void Apply(PerfMode mode)
    {
        bool high = mode == PerfMode.HighFps;
        Application.targetFrameRate = high ? 120 : 60;

        var urp = UniversalRenderPipeline.asset;
        if (urp != null) urp.renderScale = high ? 0.8f : 1f;

        PlayerPrefs.SetInt(Key, (int)mode);
        PlayerPrefs.Save();
        Debug.Log($"perf_mode={mode} high={high}");
    }

    public void SetHighFps(bool on) =>
        Apply(on ? PerfMode.HighFps : PerfMode.Balanced);
}