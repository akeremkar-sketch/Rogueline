using System.Collections;
using UnityEngine;

/// <summary>
/// Archer: Press Space to dash (2 charges). Cooldown starts when all charges are spent;
/// both charges refill when cooldown expires.
/// </summary>
public class ArcherAbility : MovementAbilityBase
{
    [SerializeField] private float     dashDistance = 5f;
    [SerializeField] private float     dashDuration = 0.15f;
    [SerializeField] private int       maxCharges   = 2;
    [SerializeField] private AudioClip dashSound;

    private Rigidbody2D _rb;
    private bool        isDashing      = false;
    private int         currentCharges = 0;
    private bool        _wasOnCooldown = false;

    public override string AbilityName   => Loc.Get("ability_dash");
    public override float  UIFillPercent => currentCharges > 0
        ? (float)currentCharges / maxCharges          // show charge fill while charges remain
        : 1f - CooldownPercent;                       // show cooldown progress when empty
    public override string StatusText    => currentCharges > 0
        ? $"{currentCharges} / {maxCharges}"
        : (IsReady ? Loc.Get("status_ready") : $"{CooldownRemaining:F1}s");

    private void OnEnable()
    {
        _rb            = GetComponent<Rigidbody2D>();
        currentCharges = maxCharges;
    }

    private void OnDisable() => CancelDash();

    protected override void Update()
    {
        base.Update();

        // Refill only when transitioning FROM cooldown TO ready (not at game start or mid-charge)
        if (_wasOnCooldown && IsReady)
        {
            currentCharges = maxCharges;
            _wasOnCooldown = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && currentCharges > 0 && !isDashing && !IsAnyUIOpen)
            StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        currentCharges--;
        AudioManager.Instance?.PlaySFX(dashSound);

        // Start cooldown only when all charges are spent
        if (currentCharges <= 0)
        {
            StartCooldown();
            _wasOnCooldown = true;
        }

        Vector2 dir      = GetAbilityDirection();
        Vector2 startPos = _rb.position;
        float   safeDist = CheckWallDistance(startPos, dir, dashDistance);
        Vector2 endPos   = startPos + dir * safeDist;
        float   elapsed  = 0f;

        if (PlayerController.Instance != null)
            PlayerController.Instance.IsAbilityControllingMovement = true;

        while (elapsed < dashDuration)
        {
            _rb.MovePosition(Vector2.Lerp(startPos, endPos, elapsed / dashDuration));
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _rb.MovePosition(endPos);
        _rb.velocity = Vector2.zero;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsAbilityControllingMovement = false;
        isDashing = false;
    }

    private void CancelDash()
    {
        if (!isDashing) return;
        StopAllCoroutines();
        isDashing = false;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsAbilityControllingMovement = false;
    }
}
