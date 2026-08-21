using UnityEngine;

public class FlameWeapon : WeaponBase
{
    [Header("Flame Settings")]
    [SerializeField] private FlameProjectile flamePrefab;
    [SerializeField] private float flameSpeed     = 7f;
    [SerializeField] private float burnDps        = 5f;
    [SerializeField] private float burnDuration   = 3f;
    [SerializeField] private float homingStrength = 180f;

    protected override void Attack(Transform target)
    {
        if (flamePrefab == null || target == null) return;

        var (dmg, crit) = GetDamageWithCrit();
        Vector2 dir = (target.position - transform.position).normalized;
        FlameProjectile fp = FlameProjectile.Rent(flamePrefab, transform.position);
        fp.Init(dir, dmg, burnDps, burnDuration, flameSpeed, GetRange(), target, homingStrength, crit);
    }
}
