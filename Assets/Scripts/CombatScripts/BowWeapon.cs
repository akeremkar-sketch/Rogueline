using UnityEngine;

public class BowWeapon : WeaponBase
{
    [Header("Bow Settings")]
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private float arrowSpeed    = 12f;
    [SerializeField] private float homingStrength = 90f;

    protected override void Attack(Transform target)
    {
        if (arrowPrefab == null || target == null) return;

        var (dmg, crit) = GetDamageWithCrit();
        Vector2 dir = (target.position - transform.position).normalized;
        Arrow arrow = Arrow.Rent(arrowPrefab, transform.position);
        arrow.Init(dir, dmg, arrowSpeed, GetRange(), target, homingStrength, crit);
    }
}
