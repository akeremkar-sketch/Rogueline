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

    // Returns true if successful, false if not enough coins
    public bool SpendCoins(int amount)
    {
        if (currentCoins < amount) return false;
        currentCoins -= amount;
        return true;
    }
}