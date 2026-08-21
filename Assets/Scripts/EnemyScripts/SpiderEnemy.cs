using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpiderEnemy : EnemyBase
{
    [Header("Spider Settings")]
    [SerializeField] private GameObject webPrefab;
    [SerializeField] private float webMinRange = 2f;
    [SerializeField] private float webMaxRange = 5f;

    private float webMinSqr;
    private float webMaxSqr;

    protected override void Start()
    {
        base.Start();
        attackRange   = 0f;
        attackRangeSq = 0f;
        webMinSqr = webMinRange * webMinRange;
        webMaxSqr = webMaxRange * webMaxRange;
    }

    protected override void Update()
    {
        base.Update();

        if (player == null || isDead) return;

        float sqrDist = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;

        if (sqrDist >= webMinSqr && sqrDist <= webMaxSqr)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown && attackAnimTimer <= 0f)
            {
                attackTimer = 0f;
                AttackPlayer();
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (player == null || isDead) return;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        float sqrDist    = toPlayer.sqrMagnitude;

        if (sqrDist < webMinSqr)
        {
            rb.velocity = -toPlayer.normalized * moveSpeed;
            SetAnimatorBool("IsMoving",     true);
            SetAnimatorBool("IsRetreating", true);
        }
        else if (sqrDist > webMaxSqr)
        {
            rb.velocity = toPlayer.normalized * moveSpeed;
            SetAnimatorBool("IsMoving",     true);
            SetAnimatorBool("IsRetreating", false);
        }
        else
        {
            rb.velocity = Vector2.zero;
            SetAnimatorBool("IsMoving",     false);
            SetAnimatorBool("IsRetreating", false);
        }

        UpdateSpriteFlip();
    }

    protected override void AttackPlayer()
    {
        PlayAttackAnimation();

        if (webPrefab == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        WebProjectile web = WebProjectile.Rent(webPrefab, transform.position);
        if (web != null) web.Launch(dir);
    }
}
