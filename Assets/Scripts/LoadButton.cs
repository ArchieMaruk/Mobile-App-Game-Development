using UnityEngine;

public class LoadButton : MonoBehaviour
{
    public void Load(string scene) =>
        _ = SceneLoader.Instance.Load(scene, Application.exitCancellationToken);
}