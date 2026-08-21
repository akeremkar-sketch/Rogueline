using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int       coinValue    = 1;
    [SerializeField] private AudioClip[] collectSounds;

    private GameObject _sourcePrefab;

    public void Init(GameObject sourcePrefab)
    {
        _sourcePrefab = sourcePrefab;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (collectSounds != null && collectSounds.Length > 0)
            AudioManager.Instance?.PlaySFX(collectSounds[Random.Range(0, collectSounds.Length)], SoundCategory.Pickup);
        float coinGain  = PlayerStats.Instance != null ? PlayerStats.Instance.CoinGain : 1f;
        int finalValue  = Mathf.RoundToInt(coinValue * coinGain);
        CoinManager.Instance.AddCoins(finalValue);
        GameManager.Instance?.RegisterCoin(finalValue);

        GetComponent<Collectible>()?.ReturnOrDestroy();
    }
}
