using UnityEngine;

public class MenuReady : MonoBehaviour
{
    static bool logged;

    void Update()
    {
        if (logged) return;
        logged = true;
        Debug.Log($"[perf] interactive {Time.realtimeSinceStartup:F2}s");
        enabled = false;
    }
}