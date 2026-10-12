using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum State { Playing, Paused, Won, Lost }

    public static GameManager Instance { get; private set; }
    public static int LastScore;

    [SerializeField] float runLength = 120f;
    [SerializeField] float startSpeed = 10f;
    [SerializeField] float maxSpeed = 20f;

    public State Current { get; private set; } = State.Playing;
    public float Score { get; private set; }
    public float Elapsed { get; private set; }
    public float Progress => Mathf.Clamp01(Elapsed / runLength);
    public float Speed => Mathf.Lerp(startSpeed, maxSpeed, Progress);
    public event Action<State> StateChanged;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void OnEnable()  => LifecycleGuard.PausedChanged += OnPaused;
    void OnDisable() => LifecycleGuard.PausedChanged -= OnPaused;

    void Update()
    {
        if (Current != State.Playing) return;
        Elapsed += Time.deltaTime;
        Score += Time.deltaTime * 10f;
        if (Elapsed >= runLength) SetState(State.Won);
    }

    public void AddScore(float amount) => Score += amount;

    void OnPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        if (paused && Current == State.Playing) SetState(State.Paused);
        else if (!paused && Current == State.Paused) SetState(State.Playing);
    }

    public void SetState(State s)
    {
        if (s == Current) return;
        Current = s;
        StateChanged?.Invoke(s);
        if (s == State.Won || s == State.Lost)
        {
            LastScore = (int)Score;
            SceneManager.LoadScene("Result");
        }
    }
}