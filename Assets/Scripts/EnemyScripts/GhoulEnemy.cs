using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GhoulEnemy : EnemyBase
{
    [Header("Charge Attack")]
    [SerializeField] private float chargeRange    = 4f;
    [SerializeField] private float chargeSpeed    = 10f;
    [SerializeField] private float chargeDuration = 0.35f;
    [SerializeField] private float chargeCooldown = 3f;
    [SerializeField] private float telegraphTime  = 0.4f;

    [Header("Enrage (below % of max HP)")]
    [SerializeField] private float enrageThreshold  = 0.5f;
    [SerializeField] private float enrageSpeedBonus = 1.5f;
    [SerializeField] private float enrageDmgBonus   = 1.4f;

    private bool  isCharging  = false;
    private bool  isEnraged   = false;
    private float chargeTimer = 0f;
    private float chargeRangeSq;
    private float baseSpeed;
    private float baseDamage;

    protected override void Start()
    {
        base.Start();
        baseSpeed     = moveSpeed;
        baseDamage    = enemyDamage;
        chargeRangeSq = chargeRange * chargeRange;
        chargeTimer   = chargeCooldown * 0.5f;

        attackRange   = 1.2f;
        attackRangeSq = attackRange * attackRange;
    }

    protected override void Update()
    {
        base.Update();
        if (player == null || isDead) return;

        CheckEnrage();

        if (!isCharging)
        {
            chargeTimer += Time.deltaTime;
            float sqrDist = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;

            if (sqrDist <= chargeRangeSq && chargeTimer >= chargeCooldown)
            {
                chargeTimer = 0f;
                StartCoroutine(ChargeSlam());
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (isCharging) return;
        base.FixedUpdate();
        UpdateSpriteFlip();
    }

    protected override void UpdateMoveAnimation(Vector2 direction, bool inAttackRange)
    {
        SetAnimatorFloat("MoveX", inAttackRange ? 0f : (direction.x > 0 ? 1f : -1f));
    }

    private void CheckEnrage()
    {
        if (isEnraged) return;
        if (currentHealth <= maxHealth * enrageThreshold)
        {
            isEnraged   = true;
            moveSpeed   = baseSpeed  * enrageSpeedBonus;
            enemyDamage = baseDamage * enrageDmgBonus;
            SetAnimatorBool("IsEnraged", true);
        }
    }

    private IEnumerator ChargeSlam()
    {
        if (isDead) yield break;

        isCharging = true;

        rb.velocity = Vector2.zero;
        SetAnimatorBool("IsTelegraph", true);
        yield return new WaitForSeconds(telegraphTime);

        if (isDead) { isCharging = false; yield break; }

        SetAnimatorBool("IsTelegraph", false);
        SetAnimatorBool("IsAttacking", true);

        Vector2 dir     = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float   elapsed = 0f;

        while (elapsed < chargeDuration && !isDead)
        {
            rb.velocity = dir * chargeSpeed;
            elapsed    += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = Vector2.zero;

        if (!isDead && player != null)
        {
            float sqrDist  = ((Vector2)transform.position - (Vector2)player.position).sqrMagnitude;
            float sqrThreshold = (attackRange + 1f) * (attackRange + 1f);
            if (sqrDist <= sqrThreshold)
            {
                AttackPlayer();
                attackTimer = 0f;
            }
        }

        attackAnimTimer = 0.3f;
        isCharging      = false;
    }

    protected override void Die()
    {
        StopAllCoroutines();
        isCharging = false;
        base.Die();
    }
}
