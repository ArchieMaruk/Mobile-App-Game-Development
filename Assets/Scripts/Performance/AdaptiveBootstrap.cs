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