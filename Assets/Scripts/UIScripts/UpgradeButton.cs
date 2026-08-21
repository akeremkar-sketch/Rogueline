using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Button button;

    private StatType _stat;
    private int _value;
    private Action<StatType, int> _callback;

    public void Setup(StatType stat, int value, Action<StatType, int> callback)
    {
        _stat     = stat;
        _value    = value;
        _callback = callback;

        if (statNameText != null) statNameText.text = GetStatName(stat);
        if (valueText    != null) valueText.text    = $"+{value}";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);

            // Prevent Space/Return from triggering this button
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
        }
    }

    private void OnClick()
    {
        _callback?.Invoke(_stat, _value);
    }

    private string GetStatName(StatType stat)
    {
        return stat switch
        {
            StatType.MoveSpeed    => Loc.Get("stat_move_speed"),
            StatType.MaxHealth    => Loc.Get("stat_max_health"),
            StatType.Damage       => Loc.Get("stat_damage"),
            StatType.AttackSpeed  => Loc.Get("stat_attack_speed"),
            StatType.AttackCount  => Loc.Get("stat_attack_count"),
            StatType.PickupRadius => Loc.Get("stat_pickup_radius"),
            StatType.XPGain       => Loc.Get("stat_xp_gain"),
            StatType.CoinGain     => Loc.Get("stat_coin_gain"),
            StatType.Defense      => Loc.Get("stat_defense"),
            StatType.Luck         => Loc.Get("stat_luck"),
            StatType.LifeSteal    => Loc.Get("stat_life_steal"),
            StatType.LifeRegen    => Loc.Get("stat_life_regen"),
            StatType.Dodge        => Loc.Get("stat_dodge"),
            StatType.Knockback    => Loc.Get("stat_knockback"),
            StatType.CritChance   => Loc.Get("stat_crit_chance"),
            StatType.Difficulty   => Loc.Get("stat_difficulty"),
            _                     => stat.ToString()
        };
    }
}
