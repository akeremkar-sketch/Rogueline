using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [SerializeField] private float startingHealthPercent = 1f;
    [SerializeField] private Color dodgeTextColor        = Color.cyan;

    [Header("Hit Feedback")]
    [SerializeField] private float iFrameDuration  = 0.4f;
    [SerializeField] private Color hitFlashColor   = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float hitFlashTime    = 0.12f;

    [Header("Sounds")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    private float          currentHealth;
    private bool           isDead      = false;
    private bool           _invincible = false;
    private SpriteRenderer _sr;
    private Color          _originalColor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentHealth = PlayerStats.Instance.MaxHealth * startingHealthPercent;
        _sr           = GetComponentInChildren<SpriteRenderer>();
        if (_sr != null) _originalColor = _sr.color;
    }

    public void ReinitializeHealth()
    {
        currentHealth = PlayerStats.Instance.MaxHealth * startingHealthPercent;
    }

    private void Update()
    {
        if (isDead || PlayerStats.Instance == null) return;

        float regen = PlayerStats.Instance.LifeRegenPerSecond;
        if (regen > 0f) Heal(regen * Time.deltaTime);

        if (burnTimer > 0f)
        {
            float tick = Mathf.Min(Time.deltaTime, burnTimer);
            burnTimer -= Time.deltaTime;
            TakeDamage(burnDps * tick);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || PlayerStats.Instance == null) return;
        if (_invincible) return;

        // Dodge
        if (PlayerStats.Instance.DodgeChance > 0f &&
            Random.value < PlayerStats.Instance.DodgeChance)
        {
            FloatingTextManager.Instance?.Show(Loc.Get("status_dodge"), transform.position, dodgeTextColor);
            return;
        }

        float reducedDamage = damage * (1f - PlayerStats.Instance.Defense);
        reducedDamage = Mathf.Max(reducedDamage, 0f);

        AudioManager.Instance?.PlaySFX(hitSound, SoundCategory.Critical);
        GameManager.Instance?.RegisterDamageTaken(reducedDamage);
        currentHealth -= reducedDamage;

        StartCoroutine(HitFeedback());

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            isDead = true;
            Die();
        }
    }

    private IEnumerator HitFeedback()
    {
        _invincible = true;

        // Flash red
        if (_sr != null) _sr.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashTime);
        if (_sr != null) _sr.color = _originalColor;

        // Remaining iFrame duration
        yield return new WaitForSeconds(iFrameDuration - hitFlashTime);

        _invincible = false;
    }

    // ── Burn ────────────────────────────────────────────────────────────────────

    private float burnDps   = 0f;
    private float burnTimer = 0f;

    public void ApplyBurn(float dps, float duration)
    {
        burnDps   = dps;
        burnTimer = Mathf.Max(burnTimer, duration);
    }

    // ── Life steal ───────────────────────────────────────────────────────────────

    public void ApplyLifeSteal(float damageDealt)
    {
        if (isDead || PlayerStats.Instance == null) return;
        float steal = damageDealt * PlayerStats.Instance.LifeSteal;
        if (steal > 0f) Heal(steal);
    }

    public void Heal(float amount)
    {
        if (isDead || PlayerStats.Instance == null) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, PlayerStats.Instance.MaxHealth);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth()     => PlayerStats.Instance != null ? PlayerStats.Instance.MaxHealth : 1f;
    public bool  IsDead             => isDead;

    // ── Web slow ────────────────────────────────────────────────────────────────

    private Coroutine _slowCoroutine;

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.SetSlowMultiplier(1f);
    }

    public void ApplyWebSlow(float slowPercent, float duration)
    {
        if (_slowCoroutine != null) StopCoroutine(_slowCoroutine);
        _slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration));
    }

    private IEnumerator SlowRoutine(float slowPercent, float duration)
    {
        PlayerStats.Instance?.SetSlowMultiplier(1f - slowPercent);
        yield return new WaitForSecondsRealtime(duration);
        _slowCoroutine = null;
        PlayerStats.Instance?.SetSlowMultiplier(1f);
    }

    private void Die()
    {
        AudioManager.Instance?.PlaySFX(deathSound, SoundCategory.Critical);
        GameManager.Instance?.GameOver();
    }
}
