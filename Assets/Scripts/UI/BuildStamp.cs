using TMPro;
using UnityEngine;

public class BuildStamp : MonoBehaviour
{
    [SerializeField] TMP_Text label;

    // first 8 characters of this build's unique ID
    public static string BuildId
    {
        get
        {
            string g = Application.buildGUID;
            return string.IsNullOrEmpty(g) || g.Length < 8 ? "editor" : g.Substring(0, 8);
        }
    }

    void OnEnable()
    {
        double hz = Screen.currentResolution.refreshRateRatio.value;
        label.text =
            $"v{Application.version} ({BuildId})\n" +
            $"{SystemInfo.deviceModel}\n" +
            $"{SystemInfo.operatingSystem}\n" +
            $"RAM {SystemInfo.systemMemorySize} MB\n" +
            $"{Screen.width}x{Screen.height} @ {hz:0} Hz";
    }
}