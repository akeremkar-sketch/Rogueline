using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] protected float maxHealth = 50f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float enemyDamage = 10f;
    [SerializeField] protected float attackRange = 0.5f; // set to 0 for ranged enemies
    [SerializeField] protected float attackCooldown = 1f;

    [Header("Drop Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int xpDropValue  = 30;
    [SerializeField] private int coinDropAmount = 1; // base coin amount (luck multiplier is applied)

    [Header("Knockback")]
    [SerializeField] protected float hitKnockbackForce   = 3f;
    [SerializeField] protected float deathKnockbackForce = 8f;

    [Header("UI")]
    [SerializeField] private EnemyHealthBar healthBar;

    [Header("Performance")]
    [Tooltip("Enemies this far from the player on the Y axis stop moving (0 = disabled)")]
    [SerializeField] private float sleepDistanceY = 25f;

    [Header("Blood FX")]
    [SerializeField] private GameObject[] bloodFXPrefabs;
    [SerializeField] [Range(0f, 1f)] private float bloodSpawnChance = 1f;
    [SerializeField] private float bloodSizeMultiplier = 1f;

    [Header("Sounds")]
    [SerializeField] protected AudioClip hitSound;
    [SerializeField] protected AudioClip deathSound;
    [SerializeField] protected AudioClip attackSound;

    protected float currentHealth;
    protected float attackTimer = 0f;
    protected float attackRangeSq;
    protected Transform player;
    protected Animator animator;
    protected Rigidbody2D rb;
    private SpriteRenderer sr;
    protected bool isDead = false;

    // Timers for resetting Animator bools
    private float hitAnimTimer       = 0f;
    protected float attackAnimTimer = 0f;
    private const float hitAnimDuration      = 0.15f;
    private const float hitKnockbackDuration = 0.15f;
    protected virtual float AttackAnimDuration => 0.4f; // override per enemy to match clip length

    protected float knockbackTimer  = 0f;
    private float _hitSoundCooldown = 0f;

    // Burn (burn damage)
    private float burnDps   = 0f;
    private float burnTimer = 0f;

    protected virtual void Start()
    {
        if (GameManager.Instance != null)
        {
            float m = GameManager.Instance.EnemyScalingMultiplier;
            maxHealth   *= m;
            enemyDamage *= m;
        }

        currentHealth = maxHealth;
        attackRangeSq = attackRange * attackRange;
        if (PlayerController.Instance != null)
            player = PlayerController.Instance.transform;
        animator = GetComponent<Animator>();
        rb       = GetComponent<Rigidbody2D>();
        sr       = GetComponent<SpriteRenderer>();
        CacheAnimatorParams();

        gameObject.layer = LayerMask.NameToLayer("Enemy");

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.Register(this);

        if (healthBar != null)
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    protected virtual void Update()
    {
        if (player == null || isDead) return;

        // Animator bool reset
        if (hitAnimTimer > 0f)
        {
            hitAnimTimer -= Time.deltaTime;
            if (hitAnimTimer <= 0f)
                SetAnimatorBool("IsHit", false);
        }
        if (_hitSoundCooldown > 0f) _hitSoundCooldown -= Time.deltaTime;
        if (attackAnimTimer > 0f)
        {
            attackAnimTimer -= Time.deltaTime;
            if (attackAnimTimer <= 0f)
                ResetAttackAnimation();
        }

        // Burn damage — triggerHitAnim: false so DoT never locks the hit animation
        if (burnTimer > 0f)
        {
            float tick = Mathf.Min(Time.deltaTime, burnTimer);
            burnTimer -= Time.deltaTime;
            TakeDamage(burnDps * tick, Vector2.zero, triggerHitAnim: false);
        }

        float sqrDist = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;

        if (attackRangeSq > 0f)
        {
            if (sqrDist <= attackRangeSq)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    AttackPlayer();
                }
            }
            else
            {
                attackTimer = 0f;
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if (player == null || isDead) return;

        // Sleep: enemies far from player skip movement calculation — major performance gain
        if (sleepDistanceY > 0f &&
            Mathf.Abs(transform.position.y - player.position.y) > sleepDistanceY)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // Knockback aktifken normal hareketi durdur
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        float sqrDist    = toPlayer.sqrMagnitude;

        if (sqrDist > attackRangeSq)
        {
            Vector2 direction = toPlayer.normalized;
            rb.velocity = direction * moveSpeed;
            UpdateMoveAnimation(direction, false);
        }
        else
        {
            rb.velocity = Vector2.zero;
            UpdateMoveAnimation(Vector2.zero, true);
        }
    }

    protected virtual void AttackPlayer()
    {
        PlayAttackAnimation();
        AudioManager.Instance?.PlaySFX(attackSound, SoundCategory.Combat);

        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.TakeDamage(enemyDamage);
    }

    // Override in each enemy to set their own animation params
    protected virtual void UpdateMoveAnimation(Vector2 direction, bool inAttackRange) { }

    protected virtual void PlayAttackAnimation()
    {
        attackAnimTimer = AttackAnimDuration;
        SetAnimatorBool("IsAttacking", true);
    }

    protected virtual void ResetAttackAnimation() => SetAnimatorBool("IsAttacking", false);

    // Flip sprite to face the player — all enemies call this in FixedUpdate
    protected void UpdateSpriteFlip()
    {
        if (player != null && sr != null)
            sr.flipX = player.position.x < transform.position.x;
    }

    // Pre-allocated damage strings — avoids string allocation on every TakeDamage call
    private static readonly string[] _dmgStrings = new string[10000];
    private static string IntToStr(int v)
    {
        if ((uint)v < (uint)_dmgStrings.Length)
            return _dmgStrings[v] ??= v.ToString();
        return v.ToString();
    }

    private HashSet<int> _animatorParams;
    private static readonly Dictionary<string, int> _hashCache = new Dictionary<string, int>();

    private static int GetHash(string param)
    {
        if (!_hashCache.TryGetValue(param, out int hash))
        {
            hash = Animator.StringToHash(param);
            _hashCache[param] = hash;
        }
        return hash;
    }

    private void CacheAnimatorParams()
    {
        _animatorParams = new HashSet<int>();
        if (animator == null) return;
        foreach (var p in animator.parameters)
            _animatorParams.Add(p.nameHash);
    }

    protected void SetAnimatorFloat(string param, float value)
    {
        if (animator == null) return;
        int hash = GetHash(param);
        if (_animatorParams.Contains(hash))
            animator.SetFloat(hash, value);
    }

    protected void SetAnimatorBool(string param, bool value)
    {
        if (animator == null) return;
        int hash = GetHash(param);
        if (_animatorParams.Contains(hash))
            animator.SetBool(hash, value);
    }

    public void ApplyBurn(float dps, float duration)
    {
        burnDps   = dps;
        burnTimer = Mathf.Max(burnTimer, duration); // update if duration is extended
    }

    public virtual void TakeDamage(float incomingDamage, Vector2 knockbackDir = default, bool triggerHitAnim = true, CritType crit = CritType.None)
    {
        if (isDead) return;

        float actualDamage = Mathf.Min(incomingDamage, currentHealth);
        GameManager.Instance?.RegisterDamageDealt(actualDamage);

        currentHealth -= actualDamage;

        // Floating damage number (skip tiny burn ticks to avoid spam)
        if (actualDamage >= 1f)
        {
            string dmgStr = IntToStr(Mathf.RoundToInt(actualDamage));
            switch (crit)
            {
                case CritType.Mega:
                    FloatingTextManager.Instance?.Show(
                        dmgStr + "\nMEGA CRIT!",
                        transform.position,
                        new Color(0.22f, 0f, 0f),
                        sizeMultiplier: 1.06f);
                    break;
                case CritType.Super:
                    FloatingTextManager.Instance?.Show(
                        dmgStr + "\nSUPER CRIT!",
                        transform.position,
                        new Color(0.75f, 0.08f, 0f),
                        sizeMultiplier: 1.03f);
                    break;
                case CritType.Normal:
                    FloatingTextManager.Instance?.Show(
                        dmgStr + "\nCRIT!",
                        transform.position,
                        new Color(1f, 0.2f, 0.2f),
                        sizeMultiplier: 1.0f);
                    break;
                default:
                    FloatingTextManager.Instance?.Show(
                        dmgStr,
                        transform.position,
                        new Color(1f, 0.45f, 0f));
                    break;
            }
        }

        // DoT effects (burn, bleed, poison) skip the hit animation
        if (triggerHitAnim)
        {
            SetAnimatorBool("IsHit", true);
            hitAnimTimer = hitAnimDuration;
            if (actualDamage >= 1f && _hitSoundCooldown <= 0f)
            {
                AudioManager.Instance?.PlaySFX(hitSound, SoundCategory.Hit);
                _hitSoundCooldown = 0.25f;
            }

            SpawnBloodFX();
        }

        // Hit knockback — brief push in the damage direction
        if (knockbackDir != Vector2.zero && rb != null && PlayerStats.Instance != null)
        {
            float force = hitKnockbackForce * PlayerStats.Instance.Knockback;
            if (force > 0.01f)
            {
                rb.velocity     = Vector2.zero;
                rb.AddForce(knockbackDir * force, ForceMode2D.Impulse);
                knockbackTimer  = hitKnockbackDuration;
            }
        }

        if (healthBar != null)
            healthBar.UpdateHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    private void SpawnBloodFX()
    {
        if (bloodFXPrefabs == null || bloodFXPrefabs.Length == 0) return;
        if (Random.value > bloodSpawnChance) return;

        int idx = Random.Range(0, bloodFXPrefabs.Length);
        GameObject prefab = bloodFXPrefabs[idx];
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, transform.position, Quaternion.identity);
        go.transform.localScale = prefab.transform.localScale * bloodSizeMultiplier;

        // Match enemy's render layer so blood appears on top
        var bloodSr = go.GetComponent<SpriteRenderer>();
        if (bloodSr != null && sr != null)
        {
            bloodSr.sortingLayerName = sr.sortingLayerName;
            bloodSr.sortingOrder     = sr.sortingOrder + 1;
        }
    }

    private void OnDestroy()
    {
        // Safe unregister on chunk despawn or unexpected destroy
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.Unregister(this);
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        AudioManager.Instance?.PlaySFX(deathSound, SoundCategory.Combat);

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.Unregister(this);

        GameManager.Instance?.RegisterKill();

        SetAnimatorBool("IsDead", true);

        // Death knockback — hurl enemy away from player
        if (rb != null && player != null && PlayerStats.Instance != null)
        {
            float force = deathKnockbackForce * PlayerStats.Instance.Knockback;
            if (force > 0.01f)
            {
                Vector2 deathDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
                rb.velocity = Vector2.zero;
                rb.AddForce(deathDir * force, ForceMode2D.Impulse);
            }
        }

        if (coinPrefab != null)
            DropSystem.Instance?.SpawnCoins(coinPrefab, transform.position, coinDropAmount);

        if (xpOrbPrefab != null)
            DropSystem.Instance?.SpawnXP(xpOrbPrefab, transform.position, xpDropValue);

        Destroy(gameObject, 1f);
    }
}
