using UnityEngine;

public class RandomModel : MonoBehaviour
{
    [SerializeField] GameObject[] models;

    void OnEnable()
    {
        int pick = Random.Range(0, models.Length);
        for (int i = 0; i < models.Length; i++)
            models[i].SetActive(i == pick);
    }
}   