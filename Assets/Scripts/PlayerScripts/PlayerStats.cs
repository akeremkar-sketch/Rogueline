using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float moveSpeedMultiplier = 1f;

    [Header("Health")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float healthMultiplier = 1f;

    [Header("Combat")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float attackSpeedMultiplier = 1f;

    [Header("Pickup")]
    [SerializeField] private float basePickupRadius = 1.5f;
    [SerializeField] private float pickupRadiusMultiplier = 1f;

    [Header("Economy")]
    [SerializeField] private float xpGainMultiplier = 1f;
    [SerializeField] private float coinGainMultiplier = 1f;

    [Header("Defense")]
    [SerializeField] private float baseDefense = 0f;
    [SerializeField] private float defenseMultiplier = 1f;

    [Header("Luck")]
    [SerializeField] private float baseLuck = 0f;
    [SerializeField] private float luckMultiplier = 1f;

    // Public erişim noktaları
    public float MoveSpeed => baseMoveSpeed * moveSpeedMultiplier;
    public float MaxHealth => baseMaxHealth * healthMultiplier;
    public float Damage => baseDamage * damageMultiplier;
    public float AttackSpeed => baseAttackSpeed * attackSpeedMultiplier;
    public float PickupRadius => basePickupRadius * pickupRadiusMultiplier;
    public float XPGain => xpGainMultiplier;
    public float CoinGain => coinGainMultiplier;
    public float Defense => baseDefense * defenseMultiplier;
    public float Luck => baseLuck * luckMultiplier;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Stat artırma metodları (item veya level up ile çağrılacak)
    public void AddMoveSpeedBonus(float percent) => moveSpeedMultiplier += percent;
    public void AddHealthBonus(float percent) => healthMultiplier += percent;
    public void AddDamageBonus(float percent) => damageMultiplier += percent;
    public void AddAttackSpeedBonus(float percent) => attackSpeedMultiplier += percent;
    public void AddPickupRadiusBonus(float percent) => pickupRadiusMultiplier += percent;
    public void AddXPGainBonus(float percent) => xpGainMultiplier += percent;
    public void AddCoinGainBonus(float percent) => coinGainMultiplier += percent;
    public void AddDefenseBonus(float percent) => defenseMultiplier += percent;
    public void AddLuckBonus(float percent) => luckMultiplier += percent;

    // Run bitince sıfırla
    public void ResetStats()
    {
        moveSpeedMultiplier = 1f;
        healthMultiplier = 1f;
        damageMultiplier = 1f;
        attackSpeedMultiplier = 1f;
        pickupRadiusMultiplier = 1f;
        xpGainMultiplier = 1f;
        coinGainMultiplier = 1f;
        defenseMultiplier = 1f;
        luckMultiplier = 1f;
    }
}