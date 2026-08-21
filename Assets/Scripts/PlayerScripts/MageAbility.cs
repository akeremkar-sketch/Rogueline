using UnityEngine;

/// <summary>
/// Mage: Press Space to blink in the current movement direction (supports diagonals).
/// Blocked by walls via base.CheckWallDistance.
/// </summary>
public class MageAbility : MovementAbilityBase
{
    [SerializeField] private float     teleportDistance = 5f;
    [SerializeField] private AudioClip blinkSound;

    private Rigidbody2D _rb;

    public override string AbilityName => Loc.Get("ability_blink");

    private void OnEnable() => _rb = GetComponent<Rigidbody2D>();

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space) && IsReady && !IsAnyUIOpen)
            Teleport();
    }

    private void Teleport()
    {
        if (_rb == null) return;

        AudioManager.Instance?.PlaySFX(blinkSound);
        Vector2 dir      = GetAbilityDirection();
        float   safeDist = CheckWallDistance(_rb.position, dir, teleportDistance);

        _rb.position += dir * safeDist;
        _rb.velocity  = Vector2.zero;
        StartCooldown();
    }
}
