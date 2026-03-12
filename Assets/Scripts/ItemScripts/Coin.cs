using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // BUG FIX: CoinGain multiplier'ı artık uygulanıyor
            int finalValue = Mathf.RoundToInt(coinValue * PlayerStats.Instance.CoinGain);
            CoinManager.Instance.AddCoins(finalValue);
            Destroy(gameObject);
        }
    }
}
