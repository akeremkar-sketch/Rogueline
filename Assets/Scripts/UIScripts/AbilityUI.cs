using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Updates the ability panel in the HUD.
/// Automatically finds the MovementAbilityBase component on the Player.
/// </summary>
public class AbilityUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image           cooldownFill;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI statusText;

    private MovementAbilityBase ability;

    private void OnEnable()  => LocalizationManager.OnLanguageChanged += OnLanguageChanged;
    private void OnDisable() => LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    private void OnLanguageChanged() => ability = null; // force re-set on next Update

    private void Update()
    {
        // Find the enabled ability once Player is initialized
        if (ability == null || !ability.enabled)
        {
            ability = null;
            if (PlayerController.Instance != null)
                foreach (var a in PlayerController.Instance.GetComponents<MovementAbilityBase>())
                    if (a.enabled) { ability = a; break; }
            if (ability == null) return;
            if (abilityNameText != null)
                abilityNameText.text = string.Format(Loc.Get("abilitybar_keybind"), ability.AbilityName);
        }

        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = ability.UIFillPercent;
            cooldownFill.color      = ability.BarColor;
        }

        if (statusText != null)
            statusText.text = ability.StatusText;
    }
}
