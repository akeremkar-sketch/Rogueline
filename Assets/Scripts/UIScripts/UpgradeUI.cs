using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance { get; private set; }

    [SerializeField] private GameObject      panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private UpgradeButton[] buttons; // 4 buttons
    [SerializeField] private UnityEngine.UI.Button statsButton;

    private static readonly StatType[] ALL_STATS      = (StatType[])System.Enum.GetValues(typeof(StatType));
    private readonly        List<StatType> _available = new List<StatType>(ALL_STATS.Length);

    public bool IsShowing => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);

        if (statsButton != null)
            statsButton.onClick.AddListener(() => StatsPanel.Instance?.OpenFromUpgrade());
    }

    private void OnDestroy()
    {
        if (statsButton != null)
            statsButton.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        // Clear selection every frame — Space/Return should never trigger a button
        if (IsShowing && EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void Show(int level = 0)
    {
        if (panel == null) return;

        panel.SetActive(true);
        Time.timeScale = 0f;

        if (titleText != null)
            titleText.text = string.Format(Loc.Get("upgrade_level_up"), level > 0 ? level : LevelSystem.Instance.CurrentLevel);

        GenerateOptions();
        EventSystem.current?.SetSelectedGameObject(null); // Prevent Space/Return from triggering buttons
    }

    // When returning from the stats panel — do not regenerate options
    public void Reopen()
    {
        if (panel == null) return;
        panel.SetActive(true);
        Time.timeScale = 0f;
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void HideTemporarily()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void GenerateOptions()
    {
        StatType[] picked = PickUniqueStats(4);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < picked.Length)
            {
                buttons[i].gameObject.SetActive(true);
                buttons[i].Setup(picked[i], RollValue(), OnUpgradeSelected);
            }
            else
            {
                // Hide button if no options remain due to maxed stats
                buttons[i].gameObject.SetActive(false);
            }
        }
    }

    // Generate a value between 1-5 based on chance
    // Higher Luck increases the probability of a higher value (best-of-2)
    private int RollValue()
    {
        int roll = Random.Range(1, 6);

        // Luck: 0pt=0%, 150pt=40%, 300pt=80% best-of-2 chance
        float rerollChance = Mathf.Clamp01(PlayerStats.Instance.Luck / 3f * 0.8f);

        if (Random.value < rerollChance)
            roll = Mathf.Max(roll, Random.Range(1, 6));

        return roll + LevelSystem.Instance.CurrentLevel;
    }

    private void OnUpgradeSelected(StatType stat, int value)
    {
        PlayerStats.Instance.ModifyStat(stat, value);
        panel.SetActive(false);
        Time.timeScale = 1f;
        LevelSystem.Instance?.OnUpgradeChosen();
    }

    private StatType[] PickUniqueStats(int count)
    {
        // Filter out maxed stats — reuse list to avoid allocation
        _available.Clear();
        foreach (StatType s in ALL_STATS)
            if (!PlayerStats.Instance.IsStatMaxed(s))
                _available.Add(s);

        // Fisher-Yates shuffle
        for (int i = _available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_available[i], _available[j]) = (_available[j], _available[i]);
        }

        int take = Mathf.Min(count, _available.Count);
        StatType[] result = new StatType[take];
        for (int i = 0; i < take; i++)
            result[i] = _available[i];

        return result;
    }
}
