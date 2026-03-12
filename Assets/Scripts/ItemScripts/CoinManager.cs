using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private int currentCoins = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
    }

    public int GetCoins() => currentCoins;
}