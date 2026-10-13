using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WalletHud : MonoBehaviour
{
    [SerializeField] TMP_Text coinsLabel;
    [SerializeField] Button upgradeButton;   // leave empty in Game
    [SerializeField] TMP_Text upgradeLabel;  // leave empty in Game
    Wallet wallet;

    void OnEnable()
    {
        wallet = Wallet.Instance;
        if (wallet == null) return;
        wallet.Changed += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (wallet != null) wallet.Changed -= Refresh;
    }

    void Refresh()
    {
        coinsLabel.SetText("{0}", wallet.Coins);
        if (upgradeButton == null) return;
        upgradeLabel.SetText("Upgrade ({0})", wallet.NextCost);
        upgradeButton.interactable = wallet.Coins >= wallet.NextCost;
    }

    public void OnUpgradePressed()
    {
        if (wallet != null) wallet.TryUpgrade();
    }
}