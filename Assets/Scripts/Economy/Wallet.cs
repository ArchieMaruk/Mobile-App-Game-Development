using UnityEngine;

public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    public int Coins { get; private set; }
    public int Level { get; private set; }
    public event System.Action Changed;
    public int NextCost => (int)(100 * Mathf.Pow(1.5f, Level));
    public int CoinValue => 5 + 5 * Level;   // 5, 10, 15... per pickup

    void Awake() => Instance = this;

    public void Earn(int n) { Coins += n; Changed?.Invoke(); }

    public void Restore(int c, int l)
    {
        Coins = c; Level = l; Changed?.Invoke();
    }

    public bool TryUpgrade()
    {
        int cost = NextCost;              // grab the cost BEFORE Level goes up
        if (Coins < cost) return false;
        Coins -= cost;
        Level++;
        Telemetry.Log("upgrade_purchased",
            ("upgrade_id", "coin_value"), ("level", Level),
            ("cost", cost), ("coins_left", Coins));
        Changed?.Invoke();
        return true;
    }
}