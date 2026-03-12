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
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        currentTarget = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance <= GetRange() && distance < closestDistance)
            {
                // Açı kontrolü
                Vector2 dirToEnemy = (enemy.transform.position - transform.position).normalized;
                Vector2 facingDir = GetFacingDirection();
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
        // PlayerController'dan son hareket yönünü al
        Animator animator = GetComponentInParent<Animator>();
        if (animator == null) return Vector2.down;

        float moveX = animator.GetFloat("MoveX");
        float moveY = animator.GetFloat("MoveY");

        if (moveX != 0 || moveY != 0)
            return new Vector2(moveX, moveY).normalized;

        return Vector2.down; // varsayılan aşağı
    }

    protected float GetDamage() => baseDamage * PlayerStats.Instance.Damage;
    protected float GetAttackSpeed() => baseAttackSpeed * PlayerStats.Instance.AttackSpeed;
    protected float GetRange() => baseRange;

    protected abstract void Attack();
}