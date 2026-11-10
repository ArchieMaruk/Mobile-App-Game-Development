using System;
using System.Collections.Generic;
using UnityEngine;

public enum MissionType { Distance, Pickups, Jumps, Slides, LaneChanges }

[Serializable]
public class Mission
{
    public string text;      // {0} is replaced by the target
    public MissionType type;
    public int target;
    public int reward;
    [NonSerialized] public int progress;

    public bool Done => progress >= target;
    public string Label => string.Format(text, target);
}

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [SerializeField] int perRun = 3;
    [SerializeField] List<Mission> presets = new List<Mission>
    {
        new Mission { text = "Run {0} m",              type = MissionType.Distance,    target = 500,  reward = 50 },
        new Mission { text = "Run {0} m",              type = MissionType.Distance,    target = 1200, reward = 150 },
        new Mission { text = "Collect {0} pickups",    type = MissionType.Pickups,     target = 10,   reward = 50 },
        new Mission { text = "Collect {0} pickups",    type = MissionType.Pickups,     target = 25,   reward = 120 },
        new Mission { text = "Jump {0} times",         type = MissionType.Jumps,       target = 10,   reward = 40 },
        new Mission { text = "Jump {0} times",         type = MissionType.Jumps,       target = 25,   reward = 100 },
        new Mission { text = "Slide {0} times",        type = MissionType.Slides,      target = 8,    reward = 40 },
        new Mission { text = "Change lanes {0} times", type = MissionType.LaneChanges, target = 30,   reward = 40 },
    };

    public readonly List<Mission> Active = new List<Mission>();
    public event Action Changed;

    void Awake()
    {
        Instance = this;
        PickMissions();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Update()
    {
        // distance is a total, not a count, so set it instead of adding
        int meters = (int)GameManager.Instance.Distance;
        foreach (var m in Active)
            if (m.type == MissionType.Distance && !m.Done && meters > m.progress)
                Advance(m, meters - m.progress);
    }

    void PickMissions()
    {
        var pool = new List<Mission>(presets);
        var usedTypes = new HashSet<MissionType>();
        while (Active.Count < perRun && pool.Count > 0)
        {
            int r = UnityEngine.Random.Range(0, pool.Count);
            Mission p = pool[r];
            pool.RemoveAt(r);
            if (!usedTypes.Add(p.type)) continue;   // one mission per type
            Active.Add(new Mission { text = p.text, type = p.type, target = p.target, reward = p.reward });
        }
    }

    public void Report(MissionType type, int amount = 1)
    {
        foreach (var m in Active)
            if (m.type == type && !m.Done) Advance(m, amount);
    }

    void Advance(Mission m, int amount)
    {
        m.progress += amount;
        if (m.Done && Wallet.Instance) Wallet.Instance.Earn(m.reward);
        Changed?.Invoke();
    }
}