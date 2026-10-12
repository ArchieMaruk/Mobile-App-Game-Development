using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
    }

    async Awaitable Start() =>
        await SceneLoader.Instance.Load("Menu", Application.exitCancellationToken);
}