using TMPro;
using UnityEngine;

public class ResultUI : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text scoreText;

    void Start()
    {
        titleText.text = GameManager.LastWon ? "You Escaped!" : "You got served...";
        scoreText.text = "Score: " + GameManager.LastScore;
    }
}