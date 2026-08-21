using UnityEngine;
using System.Collections.Generic;

public class FlameProjectile : HomingProjectileBase
{
    private float    damage;
    private float    burnDps;
    private float    burnDuration;
    private CritType crit;
    private bool     hasHit = false;

    protected override float RotationOffset => 0f;

    // ── Pool ────────────────────────────────────────────────────────────────────
    private static readonly Queue<FlameProjectile> _pool = new Queue<FlameProjectile>();

    public static FlameProjectile Rent(FlameProjectile prefab, Vector3 position)
    {
        while (_pool.Count > 0)
        {
            FlameProjectile fp = _pool.Dequeue();
            if (fp == null) continue;
            fp.transform.position = position;
            fp.gameObject.SetActive(true);
            return fp;
        }
        return Instantiate(prefab, position, Quaternion.identity);
    }

    // ── Init ────────────────────────────────────────────────────────────────────
    public void Init(Vector2 dir, float dmg, float bDps, float bDuration, float spd, float range, Transform homingTarget = null, float turnSpeed = 180f, CritType critType = CritType.None)
    {
        hasHit       = false;
        damage       = dmg;
        burnDps      = bDps;
        burnDuration = bDuration;
        crit         = critType;
        InitBase(dir, spd, range, homingTarget, turnSpeed);
    }

    // ── Hit handling ────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        EnemyBase enemy = other.GetComponent<EnemyBase>()
                       ?? other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            hasHit = true;
            Vector2 knockbackDir = rb != null ? rb.velocity.normalized : Vector2.zero;
            enemy.TakeDamage(damage, knockbackDir, crit: crit);
            enemy.ApplyBurn(burnDps, burnDuration);
            PlayerHealth.Instance?.ApplyLifeSteal(damage);
            Die();
            return;
        }

        if (other.GetComponent<DestructibleObject>() != null
         || other.GetComponentInParent<DestructibleObject>() != null)
        {
            hasHit = true;
            Die();
            return;
        }

        if (IsWall(other))
        {
            hasHit = true;
            Die();
        }
    }

    // ── Pool return ─────────────────────────────────────────────────────────────
    protected override void Die()
    {
        if (!gameObject.activeSelf) return; // already returned to pool this frame
        hasHit = true;
        if (rb != null) rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
        _pool.Enqueue(this);
    }
}
