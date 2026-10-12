using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    public bool IsLoading { get; private set; }

    [SerializeField] Slider bar;
    [SerializeField] GameObject loadingScreen;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        loadingScreen.SetActive(false);
    }

    public async Awaitable Load(string scene, CancellationToken ct)
    {
        if (IsLoading) return;
        IsLoading = true;
        var sw = Stopwatch.StartNew();
        bar.value = 0f;
        loadingScreen.SetActive(true);

        var op = SceneManager.LoadSceneAsync(scene);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
        {
            bar.value = op.progress / 0.9f;
            await Awaitable.NextFrameAsync(ct);
        }
        bar.value = 1f;
        op.allowSceneActivation = true;
        await op;

        loadingScreen.SetActive(false);
        IsLoading = false;
        Debug.Log($"[perf] scene {scene} loaded in {sw.ElapsedMilliseconds} ms");
    }
}