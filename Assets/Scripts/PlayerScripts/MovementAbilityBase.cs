using UnityEngine;

/// <summary>
/// Abstract base for all character movement abilities.
/// Handles cooldown logic and exposes shared helpers for direction and wall detection.
/// </summary>
public abstract class MovementAbilityBase : MonoBehaviour
{
    [SerializeField] protected float cooldown = 6f;

    public float CooldownRemaining { get; protected set; } = 0f;
    public float CooldownPercent   => cooldown > 0f ? Mathf.Clamp01(CooldownRemaining / cooldown) : 0f;
    public bool  IsReady           => CooldownRemaining <= 0f;

    public abstract string AbilityName { get; }

    // UI fill: 1 = ready/full, 0 = on cooldown
    public virtual float  UIFillPercent => 1f - CooldownPercent;
    public virtual string StatusText    => IsReady ? Loc.Get("status_ready") : $"{CooldownRemaining:F1}s";
    public virtual Color  BarColor      => new Color(0.631f, 0.349f, 0.082f); // #A15915

    // Cached wall mask shared across all ability instances
    private static LayerMask _wallMask;
    private static bool      _wallMaskCached;

    private Collider2D _col;
    private bool       _colCached;

    // Returns true when any menu is open — abilities should not activate.
    // Time.timeScale == 0 is the authoritative check (all pause-inducing UIs set it).
    // IsAnyUIOpen is a secondary check for cases where UI opens mid-frame before timeScale is set.
    protected static bool IsAnyUIOpen =>
        Time.timeScale == 0f                                            ||
        (UpgradeUI.Instance  != null && UpgradeUI.Instance.IsShowing)  ||
        (StatsPanel.Instance != null && StatsPanel.Instance.IsOpen)     ||
        (ShopUI.Instance     != null && ShopUI.Instance.IsOpen);

    protected virtual void Update()
    {
        if (CooldownRemaining > 0f)
            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - Time.deltaTime);
    }

    protected void StartCooldown() => CooldownRemaining = cooldown;

    /// <summary>
    /// Returns the current movement direction including diagonals.
    /// Falls back to last facing direction if the player is standing still.
    /// </summary>
    protected static Vector2 GetAbilityDirection()
    {
        var pc = PlayerController.Instance;
        if (pc == null) return Vector2.up;
        Vector2 dir = pc.MoveDirection.sqrMagnitude > 0.01f ? pc.MoveDirection : pc.FacingDirection;
        return dir == Vector2.zero ? Vector2.up : dir;
    }

    /// <summary>
    /// Sweeps ahead for walls and returns the farthest safe travel distance.
    /// Uses the ability owner's collider size for an accurate sweep.
    /// </summary>
    protected float CheckWallDistance(Vector2 origin, Vector2 dir, float maxDist)
    {
        if (!_wallMaskCached)
        {
            _wallMask       = ~LayerMask.GetMask("Player", "Enemy");
            _wallMaskCached = true;
        }

        if (!_colCached) { _col = GetComponent<Collider2D>(); _colCached = true; }

        float radius = 0.25f;
        if      (_col is CircleCollider2D  circle)  radius = circle.radius  * 0.9f;
        else if (_col is CapsuleCollider2D capsule) radius = capsule.size.x * 0.45f;

        RaycastHit2D hit = Physics2D.CircleCast(origin, radius, dir, maxDist, _wallMask);
        if (hit.collider != null && !hit.collider.isTrigger)
            return Mathf.Max(0f, hit.distance - 0.05f);

        return maxDist;
    }
}
