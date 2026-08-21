using UnityEngine;

public enum StatType
{
    MoveSpeed, MaxHealth, Damage, AttackSpeed,
    AttackCount, PickupRadius, XPGain, CoinGain,
    Defense, Luck, LifeSteal, LifeRegen, Dodge, Difficulty, Knockback, CritChance
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    private const float MIN_STAT = -100f;
    private const float MAX_STAT = 300f;

    [Header("Base Values")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float basePickupRadius = 1.5f;

    // Stat points: -100 to 300, 0 = baseline, 100 = +100% (2x)
    [Header("Stat Points (-100 to 300)")]
    [SerializeField] private float _moveSpeed    = 0f;
    [SerializeField] private float _maxHealth    = 0f;
    [SerializeField] private float _damage       = 0f;
    [SerializeField] private float _attackSpeed  = 0f;
    [SerializeField] private float _attackCount  = 0f; // every 100 points = +1 extra attack
    [SerializeField] private float _pickupRadius = 0f;
    [SerializeField] private float _xpGain       = 0f;
    [SerializeField] private float _coinGain     = 0f;
    [SerializeField] private float _defense      = 0f;
    [SerializeField] private float _luck         = 0f;
    [SerializeField] private float _lifeSteal    = 0f;
    [SerializeField] private float _lifeRegen    = 0f;
    [SerializeField] private float _dodge        = 0f;
    [SerializeField] private float _difficulty   = 0f;
    [SerializeField] private float _knockback    = 0f;
    [SerializeField] private float _critChance   = 0f;

    // Raw point values (for UI)
    public float MoveSpeedPoints    => _moveSpeed;
    public float MaxHealthPoints    => _maxHealth;
    public float DamagePoints       => _damage;
    public float AttackSpeedPoints  => _attackSpeed;
    public float AttackCountPoints  => _attackCount;
    public float PickupRadiusPoints => _pickupRadius;
    public float XPGainPoints       => _xpGain;
    public float CoinGainPoints     => _coinGain;
    public float DefensePoints      => _defense;
    public float LuckPoints         => _luck;
    public float LifeStealPoints    => _lifeSteal;
    public float LifeRegenPoints    => _lifeRegen;
    public float DodgePoints        => _dodge;
    public float DifficultyPoints   => _difficulty;
    public float KnockbackPoints    => _knockback;
    public float CritChancePoints   => _critChance;

    // Game values (converted via formula)
    // Multiplier: 0 pts = 1.0x, 100 pts = 2.0x, -100 pts = 0.0x, 300 pts = 4.0x
    public float SlowMultiplier { get; private set; } = 1f;
    public void SetSlowMultiplier(float value) => SlowMultiplier = Mathf.Clamp01(value);
    public float MoveSpeed    => baseMoveSpeed    * Mathf.Max(0.2f, Multiplier(_moveSpeed)) * SlowMultiplier;
    public float MaxHealth    => baseMaxHealth    * Multiplier(_maxHealth);
    public float Damage       => Multiplier(_damage);             // weapon.baseDamage * this value
    public float AttackSpeed  => baseAttackSpeed  * Multiplier(_attackSpeed);
    public float PickupRadius => basePickupRadius * Multiplier(_pickupRadius);
    public float XPGain       => Multiplier(_xpGain);
    public float CoinGain     => Multiplier(_coinGain);
    public int   AttackCount  => 1 + Mathf.FloorToInt(Mathf.Max(0f, _attackCount) / 10f);

    // Defense: 300 pts = 75% damage reduction (upper limit), -100 = takes 25% extra damage
    public float Defense     => Mathf.Clamp(_defense / 400f, -0.25f, 0.75f);

    // Luck: 100 pts = 1.0 (can be used as a drop rate multiplier)
    public float Luck        => _luck / 100f;

    // LifeSteal: 300 pts = 30% of damage dealt is returned as health
    public float LifeSteal   => Mathf.Clamp(_lifeSteal / 300f, 0f, 0.3f);

    // LifeRegen: 100 pts = 2 HP per second (cannot be negative)
    public float LifeRegenPerSecond => Mathf.Max(0f, _lifeRegen * 0.02f);

    // Dodge: 300 pts = 50% dodge chance (upper limit)
    public float DodgeChance => Mathf.Clamp(_dodge / 600f, 0f, 0.5f);

    // Difficulty: 0-300 pts, 100 pts = 1.0 unit (used for Chunk scaling)
    public float Difficulty  => Mathf.Max(0f, _difficulty / 100f);

    // Knockback: 0 pts = 1x base force, 100 pts = 2x, -100 pts = 0x (no knockback)
    public float Knockback   => Mathf.Max(0f, Multiplier(_knockback));

    // CritChance: 0-100 pts = 0-100% normal crit chance (2x damage)
    // 100-200 pts = overflow → 0-100% mega crit chance (3x damage, replaces normal crit)
    // 0-100 pts  → %0-50 normal crit  (2x)
    // 100-200 pts → 0-50% super crit   (3x)
    // 200-300 pts → %0-50 mega crit    (4x)
    public float NormalCritChance => Mathf.Clamp(_critChance / 200f, 0f, 0.5f);
    public float SuperCritChance  => Mathf.Clamp(Mathf.Max(0f, _critChance - 100f) / 200f, 0f, 0.5f);
    public float MegaCritChance   => Mathf.Clamp(Mathf.Max(0f, _critChance - 200f) / 200f, 0f, 0.5f);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Stat modification (called from upgrade UI)
    public void ModifyStat(StatType stat, float amount)
    {
        switch (stat)
        {
            case StatType.MoveSpeed:    _moveSpeed    = Clamp(_moveSpeed    + amount); break;
            case StatType.MaxHealth:    _maxHealth    = Clamp(_maxHealth    + amount); break;
            case StatType.Damage:       _damage       = Clamp(_damage       + amount); break;
            case StatType.AttackSpeed:  _attackSpeed  = Clamp(_attackSpeed  + amount); break;
            case StatType.AttackCount:  _attackCount  = Clamp(_attackCount  + amount); break;
            case StatType.PickupRadius: _pickupRadius = Clamp(_pickupRadius + amount); break;
            case StatType.XPGain:       _xpGain       = Clamp(_xpGain       + amount); break;
            case StatType.CoinGain:     _coinGain     = Clamp(_coinGain     + amount); break;
            case StatType.Defense:      _defense      = Clamp(_defense      + amount); break;
            case StatType.Luck:         _luck         = Clamp(_luck         + amount); break;
            case StatType.LifeSteal:    _lifeSteal    = Clamp(_lifeSteal    + amount); break;
            case StatType.LifeRegen:    _lifeRegen    = Clamp(_lifeRegen    + amount); break;
            case StatType.Dodge:        _dodge        = Clamp(_dodge        + amount); break;
            case StatType.Difficulty:   _difficulty   = Clamp(_difficulty   + amount); break;
            case StatType.Knockback:    _knockback    = Clamp(_knockback    + amount); break;
            case StatType.CritChance:   _critChance   = Clamp(_critChance + amount); break;
        }
    }

    public void ResetStats()
    {
        _moveSpeed = _maxHealth = _damage = _attackSpeed = _attackCount = 0f;
        _pickupRadius = _xpGain = _coinGain = _defense = _luck = 0f;
        _lifeSteal = _lifeRegen = _dodge = _difficulty = _knockback = _critChance = 0f;
        SetSlowMultiplier(1f);
    }

    public float GetStatPoints(StatType stat) => stat switch
    {
        StatType.MoveSpeed    => _moveSpeed,
        StatType.MaxHealth    => _maxHealth,
        StatType.Damage       => _damage,
        StatType.AttackSpeed  => _attackSpeed,
        StatType.AttackCount  => _attackCount,
        StatType.PickupRadius => _pickupRadius,
        StatType.XPGain       => _xpGain,
        StatType.CoinGain     => _coinGain,
        StatType.Defense      => _defense,
        StatType.Luck         => _luck,
        StatType.LifeSteal    => _lifeSteal,
        StatType.LifeRegen    => _lifeRegen,
        StatType.Dodge        => _dodge,
        StatType.Difficulty   => _difficulty,
        StatType.Knockback    => _knockback,
        StatType.CritChance   => _critChance,
        _                     => 0f
    };

    public bool IsStatMaxed(StatType stat) => GetStatPoints(stat) >= MAX_STAT;

    private float Clamp(float value) => Mathf.Clamp(value, MIN_STAT, MAX_STAT);
    private float Multiplier(float points) => Mathf.Max(0f, 1f + points / 100f);
}
