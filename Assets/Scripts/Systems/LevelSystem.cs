using UnityEngine;
using UnityEngine.Events;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int baseXPRequired = 100;
    [SerializeField] private float xpMultiplierPerLevel = 1.2f;

    public int CurrentLevel { get; private set; } = 1;
    public int CurrentXP { get; private set; } = 0;
    public int XPRequired { get; private set; }

    // Level atlandığında diğer sistemler dinleyebilir
    public UnityEvent OnLevelUp;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        XPRequired = baseXPRequired;
    }

    public void AddXP(int amount)
    {
        // XP gain bonusunu uygula
        int bonusXP = Mathf.RoundToInt(amount * PlayerStats.Instance.XPGain);
        CurrentXP += bonusXP;

        if (CurrentXP >= XPRequired)
            LevelUp();
    }

    private void LevelUp()
    {
        CurrentXP -= XPRequired;
        CurrentLevel++;

        // Her level için gereken XP artar
        XPRequired = Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpMultiplierPerLevel, CurrentLevel - 1));

        Debug.Log($"Level Up! Yeni level: {CurrentLevel}");
        OnLevelUp?.Invoke();
    }

    public float GetXPPercent() => (float)CurrentXP / XPRequired;
}