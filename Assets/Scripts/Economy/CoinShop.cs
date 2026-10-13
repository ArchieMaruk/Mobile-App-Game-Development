using UnityEngine;

public class CoinShop : MonoBehaviour
{
    IPurchaseProvider store = new FakePurchaseProvider();

    public async void OnBuyPack()   // Button handler: async void is OK here
    {
        if (await store.PurchaseAsync("coins_500") && Wallet.Instance != null)
            Wallet.Instance.Earn(500);
    }
}