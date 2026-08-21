using System.Collections;
using UnityEngine;

/// Minotaur Boss — 2 attacks, 2 phases.
///
/// Animator parameters:
///   IsAttacking2 (bool) — axe swing (both phases)
///   IsAttacking1 (bool) — fast attack (Phase 2 only)
///   IsEnraged    (bool) — rage transition animation (once at Phase 2 start)
///   IsMoving     (bool) — walk / idle distinction (optional)
///   IsHit        (bool) — took damage
///   IsDead       (bool) — death
[RequireComponent(typeof(SpriteRenderer))]
public class MinotaurBoss : EnemyBase
{
    [Header("Axe Swing (Both Phases)")]
    [SerializeField] private float axeDamage   = 25f;
    [SerializeField] private float axeRange    = 2.8f;
    [SerializeField] private float axeCooldown = 2f;
    [SerializeField] private float axeWindup   = 0.4f;
    [SerializeField] private float axeRecovery = 0.4f;

    [Header("Fast Attack (Phase 2 Only)")]
    [SerializeField] private float fastDamage   = 20f;
    [SerializeField] private float fastRange    = 2.5f;
    [SerializeField] private float fastCooldown = 0.8f;
    [SerializeField] private float fastWindup   = 0.15f;
    [SerializeField] private float fastRecovery = 0.2f;

    [Header("Phase 2 — Rage")]
    [SerializeField] private float phase2Threshold    = 0.5f;
    [SerializeField] private float phase2SpeedMult    = 1.5f;
    [SerializeField] private float rageAnimDuration   = 1.5f;

    [Header("Hareket")]
    [SerializeField] private float sprintThreshold  = 4f;
    [SerializeField] private float sprintSpeedMult  = 2f;
    [SerializeField] private float approachStopDist = 1.5f;

    private float _axeTimer;
    private float _fastTimer;
    private bool  _inAttack;
    private bool  _phase2;
    private bool  _enraging;
    private float _baseSpeed;

    protected override float AttackAnimDuration => 0f;

    protected override void Start()
    {
        base.Start();
        _baseSpeed = moveSpeed;
    }

    protected override void Update()
    {
        if (isDead) return;
        base.Update();
        if (player == null || _inAttack || _enraging) return;

        // Phase 2 transition — play rage animation once
        if (!_phase2 && currentHealth <= maxHealth * phase2Threshold)
        {
            _phase2   = true;
            _enraging = true;
            StartCoroutine(EnrageSequence());
            return;
        }

        if (_axeTimer  > 0f) _axeTimer  -= Time.deltaTime;
        if (_fastTimer > 0f) _fastTimer -= Time.deltaTime;

        float sqrDist = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;

        // Phase 2: fast attack takes priority
        if (_phase2 && sqrDist <= fastRange * fastRange && _fastTimer <= 0f)
        {
            _fastTimer = fastCooldown;
            StartCoroutine(PerformFastAttack());
            return;
        }

        if (sqrDist <= axeRange * axeRange && _axeTimer <= 0f)
        {
            _axeTimer = axeCooldown;
            StartCoroutine(PerformAxeSwing());
        }
    }

    protected override void FixedUpdate()
    {
        if (_inAttack || _enraging)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (player == null || isDead) return;

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        float sqrDist    = toPlayer.sqrMagnitude;

        bool withinRange  = sqrDist <= approachStopDist * approachStopDist;
        bool shouldSprint = !withinRange && sqrDist > sprintThreshold * sprintThreshold;
        float speedMult   = _phase2 ? phase2SpeedMult : 1f;

        moveSpeed = shouldSprint
            ? _baseSpeed * speedMult * sprintSpeedMult
            : _baseSpeed * speedMult;

        if (withinRange)
        {
            rb.velocity = Vector2.zero;
            SetAnimatorBool("IsMoving", false);
        }
        else
        {
            rb.velocity = toPlayer.normalized * moveSpeed;
            SetAnimatorBool("IsMoving", true);
        }

        UpdateSpriteFlip();
        if (animator != null)
            animator.speed = withinRange ? 1f : (shouldSprint ? 2.5f : 1f);
    }

    protected override void UpdateMoveAnimation(Vector2 direction, bool inAttackRange) { }

    private IEnumerator PerformAxeSwing()
    {
        _inAttack = true;
        rb.velocity = Vector2.zero;
        SetAnimatorBool("IsAttacking2", true);

        yield return new WaitForSeconds(axeWindup);

        if (player != null && Vector2.Distance(transform.position, player.position) <= axeRange * 1.2f
            && PlayerHealth.Instance != null)
            PlayerHealth.Instance.TakeDamage(axeDamage);

        yield return new WaitForSeconds(axeRecovery);
        SetAnimatorBool("IsAttacking2", false);
        _inAttack = false;
    }

    private IEnumerator PerformFastAttack()
    {
        _inAttack = true;
        rb.velocity = Vector2.zero;
        SetAnimatorBool("IsAttacking1", true);

        yield return new WaitForSeconds(fastWindup);

        if (player != null && Vector2.Distance(transform.position, player.position) <= fastRange * 1.2f
            && PlayerHealth.Instance != null)
            PlayerHealth.Instance.TakeDamage(fastDamage);

        yield return new WaitForSeconds(fastRecovery);
        SetAnimatorBool("IsAttacking1", false);
        _inAttack = false;
    }

    private IEnumerator EnrageSequence()
    {
        rb.velocity = Vector2.zero;
        SetAnimatorBool("IsEnraged", true);
        yield return new WaitForSeconds(rageAnimDuration);
        SetAnimatorBool("IsEnraged", false);
        _enraging = false;
    }

    protected override void Die()
    {
        StopAllCoroutines();
        _inAttack = false;
        _enraging = false;
        if (animator != null) animator.speed = 1f;
        base.Die();
    }
}
