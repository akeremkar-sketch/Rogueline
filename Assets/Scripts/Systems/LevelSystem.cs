using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int   baseXPRequired      = 100;
    [SerializeField] private float xpMultiplierPerLevel = 1.2f;
    [SerializeField] private int   maxLevel             = 30;

    [Header("Sounds")]
    [SerializeField] private AudioClip levelUpSound;

    private AudioSource _audioSource;

    public int CurrentLevel { get; private set; } = 1;
    public int CurrentXP { get; private set; } = 0;
    public int XPRequired { get; private set; }

    // Other systems can listen for when a level is gained
    public UnityEvent OnLevelUp;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        XPRequired = baseXPRequired;

        _audioSource                    = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake        = false;
        _audioSource.spatialBlend       = 0f;
        _audioSource.ignoreListenerPause = true;
    }

    public bool IsMaxLevel => CurrentLevel >= maxLevel;

    public void AddXP(int amount)
    {
        if (IsMaxLevel) return;

        float xpGain = PlayerStats.Instance != null ? PlayerStats.Instance.XPGain : 1f;
        int bonusXP = Mathf.RoundToInt(amount * xpGain);
        CurrentXP += bonusXP;

        while (CurrentXP >= XPRequired && !IsMaxLevel)
            LevelUp();
    }

    private readonly Queue<int> pendingLevels = new Queue<int>();

    private void LevelUp()
    {
        CurrentXP -= XPRequired;
        CurrentLevel++;

        XPRequired = Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpMultiplierPerLevel, CurrentLevel - 1));

        if (levelUpSound != null) _audioSource.PlayOneShot(levelUpSound);
        GameManager.Instance?.RegisterLevelUp();
        OnLevelUp?.Invoke();

        pendingLevels.Enqueue(CurrentLevel);
        if (pendingLevels.Count == 1)
            UpgradeUI.Instance?.Show(CurrentLevel);
    }

    public void OnUpgradeChosen()
    {
        pendingLevels.Dequeue();
        if (pendingLevels.Count > 0)
            UpgradeUI.Instance?.Show(pendingLevels.Peek());
    }

    public float GetXPPercent() => (float)CurrentXP / XPRequired;
}