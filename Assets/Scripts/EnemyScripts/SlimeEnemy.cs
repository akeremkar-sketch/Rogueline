using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SlimeEnemy : EnemyBase
{
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateSpriteFlip();
    }

    protected override void UpdateMoveAnimation(Vector2 direction, bool inAttackRange)
    {
        SetAnimatorFloat("MoveX", inAttackRange ? 0f : 1f);
    }
}
