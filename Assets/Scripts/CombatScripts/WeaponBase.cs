using System;
using System.Collections.Generic;
using UnityEngine;

public enum CritType { None, Normal, Super, Mega }

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Info")]
    [SerializeField] public string weaponDisplayName = "Weapon";
    [SerializeField] public Sprite weaponIcon;

    [Header("Weapon Settings")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float baseAttackSpeed = 1f;
    [SerializeField] protected float baseRange = 2f;

    [Header("Sound")]
    [SerializeField] protected AudioClip attackSound;

    protected float          attackTimer    = 0f;
    protected List<Transform> currentTargets = new List<Transform>();

    private List<(Transform t, float sqrDist)> _inRange = new List<(Transform, float)>();

    protected virtual void Update()
    {
        if (PlayerStats.Instance == null || EnemyManager.Instance == null) return;
        attackTimer += Time.deltaTime;

        if (attackTimer >= 1f / GetAttackSpeed())
        {
            attackTimer = 0f;
            FindTargets();

            if (currentTargets.Count > 0)
                OnAttackTriggered();

            foreach (Transform target in currentTargets)
            {
                if (target != null)
                    Attack(target);
            }
        }
    }

    protected void FindTargets()
    {
        currentTargets.Clear();
        _inRange.Clear();

        float sqrRange   = GetRange() * GetRange();
        int   attackCount = PlayerStats.Instance.AttackCount;

        // Enemies
        foreach (EnemyBase enemy in EnemyManager.Instance.GetActiveEnemies())
        {
            if (enemy == null) continue;
            float sqrDist = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDist <= sqrRange)
                _inRange.Add((enemy.transform, sqrDist));
        }

        // Partial selection — find K nearest without sorting all N.
        // For typical attackCount 1-4 this is O(N*K) vs O(N log N) sort.
        int take = Mathf.Min(attackCount, _inRange.Count);
        for (int i = 0; i < take; i++)
        {
            int minIdx = i;
            for (int j = i + 1; j < _inRange.Count; j++)
                if (_inRange[j].sqrDist < _inRange[minIdx].sqrDist)
                    minIdx = j;
            if (minIdx != i)
            {
                var tmp = _inRange[i];
                _inRange[i] = _inRange[minIdx];
                _inRange[minIdx] = tmp;
            }
            currentTargets.Add(_inRange[i].t);
        }
    }

    protected float GetDamage()
    {
        float dmg = PlayerStats.Instance != null ? baseDamage * PlayerStats.Instance.Damage : baseDamage;
        if (PlayerController.Instance != null)
            dmg *= PlayerController.Instance.AbilityDamageMultiplier;
        return dmg;
    }

    // Single roll — higher tier claims its slot first; remaining range goes to lower tiers.
    // Total crit chance never exceeds 100%.
    // Example (300 pts): mega=50%, super=50% → 50% mega, 50% super, 0% normal
    // Example (200 pts): super=50%, norm=50% → 50% super, 50% normal, no misses
    protected (float damage, CritType crit) GetDamageWithCrit()
    {
        float dmg = GetDamage();
        if (PlayerStats.Instance == null) return (dmg, CritType.None);

        float roll  = UnityEngine.Random.value;
        float mega  = PlayerStats.Instance.MegaCritChance;
        float super = PlayerStats.Instance.SuperCritChance;
        float norm  = PlayerStats.Instance.NormalCritChance;

        if (roll < mega)  return (dmg * 4f, CritType.Mega);
        roll -= mega;
        if (roll < super) return (dmg * 3f, CritType.Super);
        roll -= super;
        if (roll < norm)  return (dmg * 2f, CritType.Normal);
        return (dmg, CritType.None);
    }

    protected float GetAttackSpeed() => PlayerStats.Instance != null ? baseAttackSpeed * PlayerStats.Instance.AttackSpeed : baseAttackSpeed;
    protected float GetRange()       => baseRange;

    protected virtual void OnAttackTriggered()
    {
        AudioManager.Instance?.PlaySFX(attackSound);
    }

    protected abstract void Attack(Transform target);
}
