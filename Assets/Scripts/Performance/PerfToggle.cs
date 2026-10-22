using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class PerfToggle : MonoBehaviour
{
    void Awake()
    {
        var toggle = GetComponent<Toggle>();
        var perf = FindAnyObjectByType<PerformanceMode>();

        // show the saved setting without triggering it
        toggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("perf_mode", 0) == 1);

        if (perf != null)
            toggle.onValueChanged.AddListener(perf.SetHighFps);
    }
}