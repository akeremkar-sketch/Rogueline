using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI coinText;

    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
    }

    private void Update()
    {
        speedText.text = $"Speed: {PlayerStats.Instance.MoveSpeed:F1}";
        healthText.text = $"HP: {playerHealth.GetCurrentHealth():F0}/{playerHealth.GetMaxHealth():F0}";
        levelText.text = $"Level: {LevelSystem.Instance.CurrentLevel}";
        xpText.text = $"XP: {LevelSystem.Instance.CurrentXP}/{LevelSystem.Instance.XPRequired}";
        coinText.text = $"Coins: {CoinManager.Instance.GetCoins()}";
    }
}