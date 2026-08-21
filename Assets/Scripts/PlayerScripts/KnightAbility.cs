using UnityEngine;

/// <summary>
/// Knight: Press Space to activate charge rush. Deactivates when fuel runs out.
/// </summary>
public class KnightAbility : MovementAbilityBase
{
    [SerializeField] private float     speedMultiplier       = 1.7f;
    [SerializeField] private float     maxFuelDuration       = 3f;
    [SerializeField] private float     chargeDamageMultiplier = 2f;
    [SerializeField] private AudioClip activateSound;

    private bool  isActive      = false;
    private float fuelRemaining = 0f;

    public override string AbilityName   => Loc.Get("ability_charge_rush");
    public override float  UIFillPercent => isActive ? fuelRemaining / maxFuelDuration : 1f - CooldownPercent;
    public override string StatusText    => isActive ? string.Format(Loc.Get("status_active"), fuelRemaining.ToString("F1")) : base.StatusText;

    protected override void Update()
    {
        base.Update();

        if (!isActive)
        {
            if (Input.GetKeyDown(KeyCode.Space) && IsReady && !IsAnyUIOpen)
            {
                isActive = true;
                fuelRemaining = maxFuelDuration;
                AudioManager.Instance?.PlaySFX(activateSound);
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.AbilitySpeedMultiplier  = speedMultiplier;
                    PlayerController.Instance.AbilityDamageMultiplier = chargeDamageMultiplier;
                }
            }
        }
        else
        {
            fuelRemaining -= Time.deltaTime;

            if (fuelRemaining <= 0f)
                Deactivate();
        }
    }

    private void OnDisable() => Deactivate();

    private void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.AbilitySpeedMultiplier  = 1f;
            PlayerController.Instance.AbilityDamageMultiplier = 1f;
        }
        StartCooldown();
    }
}
