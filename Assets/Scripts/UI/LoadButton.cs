using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadButton : MonoBehaviour
{
    public void Load(string scene)
    {
        if (SceneLoader.Instance != null)
            _ = SceneLoader.Instance.Load(scene, Application.exitCancellationToken);
        else
            SceneManager.LoadScene(scene);   // fallback when Boot wasn't run
    }
}