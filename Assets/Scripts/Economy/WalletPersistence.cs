using UnityEngine;

[RequireComponent(typeof(Wallet))]
public class WalletPersistence : MonoBehaviour
{
    Wallet wallet;

    void Awake()
    {
        wallet = GetComponent<Wallet>();
        WalletSave.Load(wallet);
        wallet.Changed += Save;
    }

    void OnDestroy() => wallet.Changed -= Save;
    void Save() => WalletSave.Save(wallet);

    void OnApplicationPause(bool paused) { if (paused) Save(); }
}