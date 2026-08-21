using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsPanel : MonoBehaviour
{
    public static StatsPanel Instance { get; private set; }

    [SerializeField] private GameObject      panel;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button          backButton;
    [SerializeField] private KeyCode         toggleKey = KeyCode.Tab;

    [Header("Ability UI")]
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private Image           abilityCooldownFill;
    [SerializeField] private TextMeshProUGUI abilityStatusText;

    public enum Caller { Game, UpgradePanel }
    private Caller _caller = Caller.Game;
    public bool IsOpen { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);

        if (backButton != null)
            backButton.onClick.AddListener(Close);
    }

    private void OnDestroy()
    {
        if (backButton != null) backButton.onClick.RemoveListener(Close);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && IsOpen)
        {
            Close();
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            if (IsOpen)
                Close();
            else if (UpgradeUI.Instance != null && UpgradeUI.Instance.IsShowing)
                OpenFromUpgrade();
            else
                Open(Caller.Game);
        }
    }

    // When opened via Tab
    public void Toggle()
    {
        if (IsOpen) Close();
        else        Open(Caller.Game);
    }

    // When opened from the Level Up button
    public void OpenFromUpgrade()
    {
        UpgradeUI.Instance?.HideTemporarily();
        Open(Caller.UpgradePanel);
    }

    private void Open(Caller caller)
    {
        _caller = caller;
        IsOpen  = true;
        panel.SetActive(true);
        Time.timeScale = 0f;
        Refresh();
    }

    private void Close()
    {
        IsOpen = false;
        panel.SetActive(false);

        if (_caller == Caller.UpgradePanel)
        {
            // Return to the Level Up panel (game is still paused, options remain the same)
            UpgradeUI.Instance?.Reopen();
        }
        else
        {
            // Return to the game
            Time.timeScale = 1f;
        }
    }

    private void Refresh()
    {
        RefreshAbility();
        if (statsText == null || PlayerStats.Instance == null) return;

        PlayerStats s = PlayerStats.Instance;

        statsText.text =
            string.Format(Loc.Get("statspanel_damage"),        $"{s.DamagePoints:+0;-0;0}",       $"{s.Damage:F2}") + "\n" +
            string.Format(Loc.Get("statspanel_attack_speed"),  $"{s.AttackSpeedPoints:+0;-0;0}",  $"{s.AttackSpeed:F2}") + "\n" +
            string.Format(Loc.Get("statspanel_attack_count"),  $"{s.AttackCountPoints:+0;-0;0}",  $"{s.AttackCount}") + "\n" +
            string.Format(Loc.Get("statspanel_move_speed"),    $"{s.MoveSpeedPoints:+0;-0;0}",    $"{s.MoveSpeed:F1}") + "\n" +
            string.Format(Loc.Get("statspanel_max_health"),    $"{s.MaxHealthPoints:+0;-0;0}",    $"{s.MaxHealth:F0}") + "\n" +
            string.Format(Loc.Get("statspanel_defense"),       $"{s.DefensePoints:+0;-0;0}",      $"{s.Defense * 100:F0}") + "\n" +
            string.Format(Loc.Get("statspanel_luck"),          $"{s.LuckPoints:+0;-0;0}",         $"{s.Luck:F2}") + "\n" +
            string.Format(Loc.Get("statspanel_life_steal"),    $"{s.LifeStealPoints:+0;-0;0}",    $"{s.LifeSteal * 100:F0}") + "\n" +
            string.Format(Loc.Get("statspanel_life_regen"),    $"{s.LifeRegenPoints:+0;-0;0}",    $"{s.LifeRegenPerSecond:F1}") + "\n" +
            string.Format(Loc.Get("statspanel_dodge"),         $"{s.DodgePoints:+0;-0;0}",        $"{s.DodgeChance * 100:F0}") + "\n" +
            string.Format(Loc.Get("statspanel_xp_gain"),       $"{s.XPGainPoints:+0;-0;0}",       $"{s.XPGain:F2}") + "\n" +
            string.Format(Loc.Get("statspanel_coin_gain"),     $"{s.CoinGainPoints:+0;-0;0}",     $"{s.CoinGain:F2}") + "\n" +
            string.Format(Loc.Get("statspanel_pickup_radius"), $"{s.PickupRadiusPoints:+0;-0;0}", $"{s.PickupRadius:F1}") + "\n" +
            string.Format(Loc.Get("statspanel_difficulty"),    $"{s.DifficultyPoints:+0;-0;0}",   $"{s.Difficulty:F2}") + "\n" +
            string.Format(Loc.Get("statspanel_knockback"),     $"{s.KnockbackPoints:+0;-0;0}",    $"{s.Knockback:F2}") + "\n" +
            string.Format(Loc.Get("statspanel_crit_chance"),   $"{s.CritChancePoints:+0;-0;0}",   $"{Mathf.Min(s.NormalCritChance + s.SuperCritChance + s.MegaCritChance, 1f) * 100:F0}");
    }

    private void RefreshAbility()
    {
        var ability = GetActiveAbility();
        if (ability == null) return;

        if (abilityNameText     != null) abilityNameText.text           = string.Format(Loc.Get("abilitybar_keybind"), ability.AbilityName);
        if (abilityCooldownFill != null) abilityCooldownFill.fillAmount = ability.UIFillPercent;
        if (abilityCooldownFill != null) abilityCooldownFill.color      = ability.BarColor;
        if (abilityStatusText   != null) abilityStatusText.text         = ability.StatusText;
    }

    private static MovementAbilityBase GetActiveAbility()
    {
        if (PlayerController.Instance == null) return null;
        foreach (var a in PlayerController.Instance.GetComponents<MovementAbilityBase>())
            if (a.enabled) return a;
        return null;
    }
}
