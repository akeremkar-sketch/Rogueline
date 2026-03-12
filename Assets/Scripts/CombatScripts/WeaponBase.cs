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
        EnemyBase closest = EnemyManager.Instance.GetClosestEnemy(transform.position, GetRange());

        if (closest == null)
        {
            currentTarget = null;
            return;
        }

        // Açı kontrolü
        Vector2 dirToEnemy = (closest.transform.position - transform.position).normalized;
        Vector2 facingDir = GetFacingDirection();
        float angle = Vector2.Angle(facingDir, dirToEnemy);

        if (angle <= attackAngle / 2f)
            currentTarget = closest.transform;
        else
            currentTarget = null;
    }

    private Vector2 GetFacingDirection()
    {
        // PlayerController.Instance üzerinden al — Animator parametresine bağımlılık YOK
        if (PlayerController.Instance != null)
            return PlayerController.Instance.FacingDirection;

        return Vector2.down;
    }

    protected float GetDamage() => baseDamage * PlayerStats.Instance.Damage;
    protected float GetAttackSpeed() => baseAttackSpeed * PlayerStats.Instance.AttackSpeed;
    protected float GetRange() => baseRange;

    protected abstract void Attack();
}