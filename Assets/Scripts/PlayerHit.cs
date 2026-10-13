using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    [SerializeField] float pickupScore = 50f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hazard"))
        {
            Haptics.Pulse();
            GameManager.Instance.SetState(GameManager.State.Lost);
        }
        else if (other.CompareTag("Pickup"))
        {
            GameManager.Instance.AddScore(pickupScore);
            if (Wallet.Instance != null) Wallet.Instance.Earn(Wallet.Instance.CoinValue);
            if (other.TryGetComponent(out Hazard h)) h.Despawn();
        }
    }
}