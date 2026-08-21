using System.Collections;
using UnityEngine;

/// <summary>
/// A chest that looks harmless but activates when the player gets close.
/// While dormant: not registered with EnemyManager, kinematic, chest sprite shown.
/// On activation: pauses briefly ("!"), then becomes a full EnemyBase-based enemy.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class MimicEnemy : EnemyBase
{
    [Header("Mimic")]
    [SerializeField] private Sprite chestSprite;
    [SerializeField] private float  activationRange   = 3f;
    [SerializeField] private float  surpriseDuration  = 0.6f; // freeze before charging

    private bool           _isDormant = true;
    private SpriteRenderer _sr;
    private float          _activationRangeSq;

    protected override void Start()
    {
        base.Start(); // scales health, caches references, registers with EnemyManager
        _activationRangeSq = activationRange * activationRange;

        // Undo the enemy setup — mimic starts dormant
        EnemyManager.Instance?.Unregister(this);
        gameObject.layer = LayerMask.NameToLayer("Default");
        rb.isKinematic   = true;
        rb.velocity      = Vector2.zero;

        _sr = GetComponent<SpriteRenderer>();
        if (chestSprite != null) _sr.sprite = chestSprite;
        if (animator    != null) animator.enabled = false;
    }

    protected override void Update()
    {
        if (_isDormant) { CheckActivation(); return; }
        base.Update();
    }

    protected override void FixedUpdate()
    {
        if (_isDormant) return;
        base.FixedUpdate();
        UpdateSpriteFlip();
    }

    private void CheckActivation()
    {
        if (player == null) return;
        float sqrDist = ((Vector2)(player.position - transform.position)).sqrMagnitude;
        if (sqrDist <= _activationRangeSq)
            StartCoroutine(Activate());
    }

    private IEnumerator Activate()
    {
        _isDormant = false; // prevent re-entry

        // Surprise indicator
        FloatingTextManager.Instance?.Show("!", transform.position + Vector3.up * 0.5f, Color.red);

        yield return new WaitForSeconds(surpriseDuration);

        // Become a real enemy
        EnemyManager.Instance?.Register(this);
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        rb.isKinematic   = false;
        if (animator != null) animator.enabled = true;
    }

    protected override void UpdateMoveAnimation(Vector2 direction, bool inAttackRange)
    {
        SetAnimatorFloat("MoveX", inAttackRange ? 0f : (direction.x > 0f ? 1f : -1f));
    }

    protected override void Die()
    {
        _isDormant = false;
        StopAllCoroutines();
        base.Die();
    }
}
