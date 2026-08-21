using UnityEngine;
using System.Collections.Generic;

public class Arrow : HomingProjectileBase
{
    private float    damage;
    private CritType crit;
    private HashSet<EnemyBase> hitEnemies = new HashSet<EnemyBase>();

    protected override float RotationOffset => -90f;

    // ── Pool ────────────────────────────────────────────────────────────────────
    private static readonly Queue<Arrow> _pool = new Queue<Arrow>();

    public static Arrow Rent(Arrow prefab, Vector3 position)
    {
        while (_pool.Count > 0)
        {
            Arrow a = _pool.Dequeue();
            if (a == null) continue;
            a.transform.position = position;
            a.gameObject.SetActive(true);
            return a;
        }
        return Instantiate(prefab, position, Quaternion.identity);
    }

    // ── Init ────────────────────────────────────────────────────────────────────
    public void Init(Vector2 dir, float dmg, float spd, float range, Transform homingTarget = null, float turnSpeed = 90f, CritType critType = CritType.None)
    {
        damage = dmg;
        crit   = critType;
        hitEnemies.Clear();
        InitBase(dir, spd, range, homingTarget, turnSpeed);
    }

    // ── Hit handling ────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>()
                       ?? other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            if (hitEnemies.Contains(enemy)) return;
            hitEnemies.Add(enemy);
            Vector2 knockbackDir = rb != null ? rb.velocity.normalized : Vector2.zero;
            enemy.TakeDamage(damage, knockbackDir, crit: crit);
            PlayerHealth.Instance?.ApplyLifeSteal(damage);
            return;
        }

        if (other.GetComponent<DestructibleObject>() != null
         || other.GetComponentInParent<DestructibleObject>() != null)
        {
            Die();
            return;
        }

        if (IsWall(other)) Die();
    }

    // ── Pool return ─────────────────────────────────────────────────────────────
    protected override void Die()
    {
        if (!gameObject.activeSelf) return; // already returned to pool this frame
        hitEnemies.Clear();
        if (rb != null) rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
        _pool.Enqueue(this);
    }
}
