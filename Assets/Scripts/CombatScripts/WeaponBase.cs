using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float baseAttackSpeed = 1f;
    [SerializeField] protected float baseRange = 2f;

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
        // Vampire Survivors tarzı: en yakın düşmana her zaman saldır, yön fark etmez
        EnemyBase closest = EnemyManager.Instance.GetClosestEnemy(transform.position, GetRange());
        currentTarget = closest != null ? closest.transform : null;
    }

    protected float GetDamage() => baseDamage * PlayerStats.Instance.Damage;
    protected float GetAttackSpeed() => baseAttackSpeed * PlayerStats.Instance.AttackSpeed;
    protected float GetRange() => baseRange;

    protected abstract void Attack();
}
