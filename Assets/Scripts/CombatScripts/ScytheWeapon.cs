using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Vampire's weapon. Hits ALL enemies AND destructibles in range simultaneously.
/// Ignores AttackCount. Has bonus lifesteal on top of stat-based lifesteal.
/// Automatically disabled while the Vampire is in bat form.
/// </summary>
public class ScytheWeapon : WeaponBase
{
    [Header("Scythe Settings")]
    [SerializeField] private float bonusLifeSteal = 0.15f;

    private readonly List<EnemyBase>         _hitEnemies  = new List<EnemyBase>();
    private readonly List<DestructibleObject> _hitDestruct = new List<DestructibleObject>();

    protected override void Update()
    {
        if (VampireAbility.IsInBatForm) return;
        if (PlayerStats.Instance == null || EnemyManager.Instance == null) return;

        attackTimer += Time.deltaTime;
        if (attackTimer < 1f / GetAttackSpeed()) return;
        attackTimer = 0f;

        float sqrRange = GetRange() * GetRange();
        float totalDmg = 0f;

        // Hit enemies
        _hitEnemies.Clear();
        foreach (EnemyBase enemy in EnemyManager.Instance.GetActiveEnemies())
        {
            if (enemy == null) continue;
            float sqrDist = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDist <= sqrRange) _hitEnemies.Add(enemy);
        }

        if (_hitEnemies.Count > 0)
            AudioManager.Instance?.PlaySFX(attackSound);

        foreach (EnemyBase enemy in _hitEnemies)
        {
            var (dmg, crit) = GetDamageWithCrit();
            Vector2 knockDir = (enemy.transform.position - transform.position).normalized;
            enemy.TakeDamage(dmg, knockDir, crit: crit);
            totalDmg += dmg;
        }

        // Lifesteal from enemy damage only
        if (totalDmg > 0f)
        {
            float steal = totalDmg * (PlayerStats.Instance.LifeSteal + bonusLifeSteal);
            PlayerHealth.Instance?.Heal(steal);
        }
    }

    // Not used — ScytheWeapon manages its own attack loop in Update
    protected override void Attack(Transform target) { }
}
