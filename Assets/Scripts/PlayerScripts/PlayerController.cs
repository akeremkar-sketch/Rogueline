using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Animation Settings")]
    [SerializeField] private float baseAnimSpeed = 1f;

    public Vector2 FacingDirection              { get; private set; } = Vector2.down;
    public Vector2 MoveDirection               { get; private set; } = Vector2.down;
    public bool    IsAbilityControllingMovement    { get; set; }    = false;
    public bool    IsAbilityControllingAnimation   { get; set; }    = false;
    public float   AbilitySpeedMultiplier      { get; set; }        = 1f;
    public float   AbilityDamageMultiplier     { get; set; }        = 1f;

    private Rigidbody2D    rb;
    private Animator       animator;
    private SpriteRenderer sr;
    private bool hasAnimator => animator != null && animator.enabled && animator.runtimeAnimatorController != null;
    private Vector2        moveInput;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float       footstepInterval = 0.35f;
    private float _footstepTimer = 0f;

    [Header("Interaction")]
    [SerializeField] private float interactRange = 1.5f;

    private DestructibleObject _lastNearest;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        rb       = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr       = GetComponent<SpriteRenderer>();
        gameObject.layer = LayerMask.NameToLayer("Player");
        // Default facing: down (IdleDown state)
        if (hasAnimator) { animator.SetFloat("MoveX", 0f); animator.SetFloat("MoveY", -1f); }
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;

        bool isMoving = moveInput != Vector2.zero;
        if (isMoving) MoveDirection = moveInput;

        if (isMoving && footstepClips != null && footstepClips.Length > 0)
        {
            _footstepTimer += Time.deltaTime;
            if (_footstepTimer >= footstepInterval)
            {
                _footstepTimer -= footstepInterval;
                AudioManager.Instance?.PlaySFX(footstepClips[Random.Range(0, footstepClips.Length)], SoundCategory.Ambient);
            }
        }

        if (hasAnimator && !IsAbilityControllingAnimation)
        {
            animator.SetBool("IsMoving", isMoving);
            float speedRatio = PlayerStats.Instance.MoveSpeed / 5f;
            animator.speed = isMoving ? baseAnimSpeed * speedRatio : baseAnimSpeed;
        }

        if (isMoving)
        {
            if (Mathf.Abs(y) >= Mathf.Abs(x))
            {
                if (hasAnimator && !IsAbilityControllingAnimation) { animator.SetFloat("MoveX", 0f); animator.SetFloat("MoveY", y > 0 ? 1f : -1f); }
                FacingDirection = new Vector2(0f, y > 0 ? 1f : -1f);
            }
            else
            {
                if (hasAnimator && !IsAbilityControllingAnimation) { animator.SetFloat("MoveX", x > 0 ? 1f : -1f); animator.SetFloat("MoveY", 0f); }
                FacingDirection = new Vector2(x > 0 ? 1f : -1f, 0f);
                // Sprites face left — flip horizontally when moving right
                if (sr != null && !IsAbilityControllingAnimation) sr.flipX = x > 0;
            }
        }

        DestructibleObject nearest = GetNearestDestructible();

        if (nearest != _lastNearest)
        {
            if (_lastNearest != null) _lastNearest.ShowPrompt(false);
            if (nearest      != null) nearest.ShowPrompt(true);
            _lastNearest = nearest;
        }

        if (Input.GetKeyDown(KeyCode.E) && nearest != null)
            nearest.Interact();
    }

    private DestructibleObject GetNearestDestructible()
    {
        var destructibles = DestructibleManager.Instance?.GetActive();
        if (destructibles == null || destructibles.Count == 0) return null;

        Vector2 playerPos  = transform.position;
        float   sqrRange   = interactRange * interactRange;
        DestructibleObject nearest    = null;
        float              nearestSqr = float.MaxValue;

        foreach (var obj in destructibles)
        {
            if (obj == null) continue;
            float sqrDist = ((Vector2)obj.transform.position - playerPos).sqrMagnitude;
            if (sqrDist <= sqrRange && sqrDist < nearestSqr)
            {
                nearestSqr = sqrDist;
                nearest    = obj;
            }
        }

        return nearest;
    }

    private void FixedUpdate()
    {
        if (PlayerStats.Instance == null) return;
        if (IsAbilityControllingMovement) return;

        rb.velocity = moveInput * PlayerStats.Instance.MoveSpeed * AbilitySpeedMultiplier;

        if (CameraController.Instance != null)
        {
            float minY = CameraController.Instance.CameraBottomY + 0.5f;
            if (rb.position.y < minY)
            {
                rb.position = new Vector2(rb.position.x, minY);
                if (rb.velocity.y < 0f)
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
            }
        }
    }
}
