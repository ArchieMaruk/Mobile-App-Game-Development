using UnityEngine;

public interface IPurchaseProvider
{
    Awaitable<bool> PurchaseAsync(string productId);
}