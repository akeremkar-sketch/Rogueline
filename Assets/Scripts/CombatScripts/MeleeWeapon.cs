using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    [Header("Melee Settings")]
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private float effectDuration = 0.2f;

    protected override void Attack()
    {
        if (currentTarget == null) return;

        EnemyBase enemy = currentTarget.GetComponent<EnemyBase>();
        if (enemy != null)
            enemy.TakeDamage(GetDamage());

        if (slashEffectPrefab != null)
        {
            // Düşmanın yönüne göre açıyı hesapla
            Vector2 dir = (currentTarget.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject effect = Instantiate(slashEffectPrefab, currentTarget.position, rotation);
            Destroy(effect, effectDuration);
        }
    }
}