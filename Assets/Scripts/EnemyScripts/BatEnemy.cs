using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BatEnemy : EnemyBase
{
    [Header("Lunge Settings")]
    [SerializeField] private float lungeSpeed    = 12f;
    [SerializeField] private float lungeDistance = 1.5f;
    [SerializeField] private float returnSpeed   = 6f;

    public static bool AlliesActive { get; set; } = false;

    private bool           isLunging;
    private EnemyBase      _allyTarget;
    private SpriteRenderer _sr;
    private Vector2        _approachOffset;
    private bool           _offsetInitialized;
    private bool           _wasAlly;
    private float          _targetSearchTimer;

    protected override void Start()
    {
        base.Start();
        _sr = GetComponent<SpriteRenderer>();
    }

    protected override void Update()
    {
        if (AlliesActive) { AllyUpdate(); return; }
        base.Update();
    }

    protected override void FixedUpdate()
    {
        if (AlliesActive != _wasAlly)
        {
            StopAllCoroutines();
            isLunging = false; _allyTarget = null;
            _offsetInitialized = false; rb.velocity = Vector2.zero;
            _wasAlly = AlliesActive;
        }

        if (AlliesActive)
        {
            AllyFixedUpdate();
            if (_allyTarget != null) _sr.flipX = _allyTarget.transform.position.x < transform.position.x;
            return;
        }

        if (!isLunging) base.FixedUpdate();
        UpdateSpriteFlip();
    }

    // --- Enemy behaviour ---

    protected override void AttackPlayer()
    {
        base.AttackPlayer();
        if (!isLunging) StartCoroutine(LungeCoroutine(player, returnToOrigin: true));
    }

    // --- Ally behaviour ---

    private void AllyUpdate()
    {
        if (isDead) return;

        // base.Update() is skipped in ally mode, so tick attack anim timer manually
        if (attackAnimTimer > 0f)
        {
            attackAnimTimer -= Time.deltaTime;
            if (attackAnimTimer <= 0f)
                ResetAttackAnimation();
        }

        _targetSearchTimer -= Time.deltaTime;
        if (_targetSearchTimer <= 0f)
        {
            _targetSearchTimer = 0.25f;
            EnemyBase newTarget = FindAllyTarget();
            if (newTarget != _allyTarget) { _allyTarget = newTarget; _offsetInitialized = false; }
        }

        if (_allyTarget == null || isLunging) return;

        float sqrDist = ((Vector2)(_allyTarget.transform.position - transform.position)).sqrMagnitude;
        if (sqrDist <= attackRange * attackRange)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown) { attackTimer = 0f; AllyAttack(); }
        }
        else
        {
            attackTimer = 0f;
        }
    }

    private void AllyFixedUpdate()
    {
        if (isDead || isLunging || _allyTarget == null) return;

        if (!_offsetInitialized)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _approachOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.6f;
            _offsetInitialized = true;
        }

        Vector2 toDestination = (Vector2)_allyTarget.transform.position + _approachOffset - (Vector2)transform.position;
        float sqrDist = toDestination.sqrMagnitude;

        if (sqrDist > 0.1f)
        {
            rb.velocity = toDestination.normalized * moveSpeed;
            UpdateMoveAnimation(toDestination.normalized, sqrDist <= attackRange * attackRange);
        }
        else
        {
            rb.velocity = Vector2.zero;
            UpdateMoveAnimation(Vector2.zero, true);
        }
    }

    private void AllyAttack()
    {
        if (_allyTarget == null) return;
        PlayAttackAnimation();
        _allyTarget.TakeDamage(enemyDamage, (_allyTarget.transform.position - transform.position).normalized);
        StartCoroutine(LungeCoroutine(_allyTarget.transform, returnToOrigin: false));
    }

    private EnemyBase FindAllyTarget()
    {
        if (EnemyManager.Instance == null) return null;
        float bestSqr = float.MaxValue;
        EnemyBase best = null;
        foreach (EnemyBase enemy in EnemyManager.Instance.GetActiveEnemies())
        {
            if (enemy == null || enemy is BatEnemy) continue;
            float sqr = ((Vector2)(enemy.transform.position - transform.position)).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = enemy; }
        }
        return best;
    }

    // --- Shared ---

    // returnToOrigin = true  → enemy lunge (goes forward then returns)
    // returnToOrigin = false → ally lunge  (goes forward only)
    private IEnumerator LungeCoroutine(Transform target, bool returnToOrigin)
    {
        if (target == null) yield break;
        isLunging = true;

        Vector2 origin    = transform.position;
        Vector2 direction = (target.position - transform.position).normalized;
        Vector2 lungeEnd  = origin + direction * lungeDistance;
        rb.velocity       = Vector2.zero;

        while ((rb.position - lungeEnd).sqrMagnitude > 0.0025f)
        {
            if (target == null) break;
            rb.MovePosition(Vector2.MoveTowards(rb.position, lungeEnd, lungeSpeed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }

        if (returnToOrigin)
        {
            while ((rb.position - origin).sqrMagnitude > 0.0025f)
            {
                rb.MovePosition(Vector2.MoveTowards(rb.position, origin, returnSpeed * Time.fixedDeltaTime));
                yield return new WaitForFixedUpdate();
            }
        }

        rb.velocity = Vector2.zero;
        isLunging   = false;
    }

    protected override void Die()
    {
        StopAllCoroutines();
        isLunging = false;
        base.Die();
    }

    protected override void UpdateMoveAnimation(Vector2 direction, bool inAttackRange)
    {
        SetAnimatorFloat("MoveX", inAttackRange ? 0f : (direction.x > 0 ? 1f : -1f));
    }
}
