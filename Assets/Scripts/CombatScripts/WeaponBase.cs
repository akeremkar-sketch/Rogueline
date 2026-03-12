using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float baseAttackSpeed = 1f;
    [SerializeField] protected float baseRange = 2f;
    [SerializeField] protected float attackAngle = 120f;

    protected float attackTimer = 0f;
    protected Transform currentTarget;

    protected virtual void Update()
    {
        attackTimer += Time.deltaTime;
        FindTarget();

        if (currentTarget != null && attackTimer >= 1f / GetAttackSpeed())
        {
            attackTimer = 0f;
            Attack();
        }
    }

    protected void FindTarget()
    {
        // EnemyManager üzerinden cachelenen listeyi kullan — her frame FindGameObjectsWithTag YOK
        if (EnemyManager.Instance == null) return;

        List<EnemyBase> enemies = EnemyManager.Instance.GetActiveEnemies();
        float closestDistance = Mathf.Infinity;
        currentTarget = null;

        Vector2 facingDir = GetFacingDirection();
        float range = GetRange();

        foreach (EnemyBase enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance <= range && distance < closestDistance)
            {
                // Açı kontrolü
                Vector2 dirToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(facingDir, dirToEnemy);

                if (angle <= attackAngle / 2f)
                {
                    closestDistance = distance;
                    currentTarget = enemy.transform;
                }
            }
        }
    }

    private Vector2 GetFacingDirection()
    {
        // PlayerController.Instance üzerinden doğrudan al — Animator parametresine bağımlılık YOK
        if (PlayerController.Instance != null)
            return PlayerController.Instance.FacingDirection;

        return Vector2.down;
    }

    protected float GetDamage() => baseDamage * PlayerStats.Instance.Damage;
    protected float GetAttackSpeed() => baseAttackSpeed * PlayerStats.Instance.AttackSpeed;
    protected float GetRange() => baseRange;

    protected abstract void Attack();
}
