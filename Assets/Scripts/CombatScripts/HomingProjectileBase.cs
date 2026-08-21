using UnityEngine;

/// <summary>
/// Shared base for all homing projectiles (Arrow, FlameProjectile, etc.).
/// Handles: velocity-based homing, facing rotation, max-range self-destruct, wall detection.
/// Child classes call InitBase() in their own Init() and override OnTriggerEnter2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class HomingProjectileBase : MonoBehaviour
{
    [SerializeField] protected LayerMask wallLayers;

    protected Rigidbody2D rb;
    protected float  speed;
    protected float  maxRange;
    private   float  maxRangeSq;
    protected float  homingStrength;
    protected Transform target;
    protected Vector2 startPos;

    // Arrow needs -90 offset (sprite points up), Flame needs 0 (sprite points right)
    protected virtual float RotationOffset => 0f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected void InitBase(Vector2 dir, float spd, float range, Transform homingTarget, float turnSpeed)
    {
        speed          = spd;
        maxRange       = range;
        maxRangeSq     = range * range;
        homingStrength = turnSpeed;
        target         = homingTarget;
        startPos       = transform.position;

        if (rb != null)
            rb.velocity = dir.normalized * speed;
    }

    protected virtual void Update()
    {
        // Homing
        if (rb != null && target != null)
        {
            Vector2 currentDir  = rb.velocity.normalized;
            Vector2 targetDir   = ((Vector2)target.position - rb.position).normalized;
            float   signedAngle = Vector2.SignedAngle(currentDir, targetDir);
            float   maxTurn     = homingStrength * Time.deltaTime;
            float   turn        = Mathf.Clamp(signedAngle, -maxTurn, maxTurn);
            Vector2 newDir      = (Vector2)(Quaternion.Euler(0f, 0f, turn) * currentDir);
            rb.velocity         = newDir * speed;
        }

        // Rotate sprite to face velocity direction
        if (rb != null && rb.velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg + RotationOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // Self-destruct when max range is reached (sqrMagnitude avoids sqrt)
        if (((Vector2)transform.position - startPos).sqrMagnitude >= maxRangeSq)
            Die();
    }

    // Subclasses override to return to pool instead of destroying
    protected virtual void Die() => Destroy(gameObject);

    /// <summary>Returns true if the collider belongs to a wall layer.</summary>
    protected bool IsWall(Collider2D other) =>
        (wallLayers.value & (1 << other.gameObject.layer)) != 0;
}
