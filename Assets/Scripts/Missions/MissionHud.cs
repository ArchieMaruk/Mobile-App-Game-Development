using System.Text;
using TMPro;
using UnityEngine;

public class MissionHud : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    readonly StringBuilder sb = new StringBuilder();

    void Start()
    {
        MissionManager.Instance.Changed += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (MissionManager.Instance) MissionManager.Instance.Changed -= Refresh;
    }

    void Refresh()
    {
        sb.Clear();
        foreach (var m in MissionManager.Instance.Active)
        {
            if (m.Done)
                sb.Append("<color=#2EA043>").Append(m.Label).Append("  +$").Append(m.reward).Append("</color>\n");
            else
                sb.Append(m.Label).Append("  ").Append(m.progress).Append('/').Append(m.target).Append('\n');
        }
        label.SetText(sb);
    }
}