using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    [Header("Melee Settings")]
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private float effectDuration = 0.2f;

    private readonly Queue<GameObject> _effectPool = new();

    protected override void Attack(Transform target)
    {
        if (target == null) return;

        var (dmg, crit) = GetDamageWithCrit();
        Vector2 knockbackDir = (target.position - transform.position).normalized;

        EnemyBase enemy = target.GetComponent<EnemyBase>();
        if (enemy == null) return;
        enemy.TakeDamage(dmg, knockbackDir, crit: crit);
        PlayerHealth.Instance?.ApplyLifeSteal(dmg);

        if (slashEffectPrefab != null)
        {
            float      angle  = Mathf.Atan2(knockbackDir.y, knockbackDir.x) * Mathf.Rad2Deg;
            GameObject effect = RentEffect(target.position, Quaternion.Euler(0f, 0f, angle));
            StartCoroutine(ReturnEffect(effect, effectDuration));
        }
    }

    private GameObject RentEffect(Vector3 pos, Quaternion rot)
    {
        while (_effectPool.Count > 0)
        {
            GameObject recycled = _effectPool.Dequeue();
            if (recycled == null) continue;
            recycled.transform.SetPositionAndRotation(pos, rot);
            recycled.SetActive(true);
            return recycled;
        }
        return Instantiate(slashEffectPrefab, pos, rot);
    }

    private IEnumerator ReturnEffect(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effect == null) yield break;
        effect.SetActive(false);
        _effectPool.Enqueue(effect);
    }
}
