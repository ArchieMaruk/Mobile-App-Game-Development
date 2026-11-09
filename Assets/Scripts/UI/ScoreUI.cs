using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    int shown = -1;

    void Update()
    {
        int meters = (int)GameManager.Instance.Distance;
        if (meters == shown) return;   // only update when the number changes
        shown = meters;
        label.SetText("{0} m", meters);
    }
}