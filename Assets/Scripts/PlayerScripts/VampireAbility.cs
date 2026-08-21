using UnityEngine;

/// <summary>
/// Vampire: Press Space to toggle bat form.
/// Blood meter fills on kills. Can activate as long as blood > 0.
/// In bat form: faster, cannot attack, uses bat sprite. Drains blood over time.
/// </summary>
public class VampireAbility : MovementAbilityBase
{
    [Header("Blood Meter")]
    [SerializeField] private float maxBlood          = 100f;
    [SerializeField] private float bloodPerKill      = 25f;  // blood gained per enemy kill
    [SerializeField] private float bloodDrainPerSec  = 20f;  // blood drained per second in bat form

    [Header("Bat Form")]
    [SerializeField] private float                     batSpeedMultiplier    = 2f;
    [SerializeField] private Sprite                    batSprite;
    [SerializeField] private RuntimeAnimatorController batAnimatorController;
    [SerializeField] private AudioClip                 enterBatSound;
    [SerializeField] private AudioClip                 exitBatSound;

    // Shared static so ScytheWeapon and BatEnemy can read it without a direct reference
    public static bool IsInBatForm { get; private set; }

    [SerializeField] [Range(0f, 1f)] private float startingBloodPercent = 0.2f;

    // Blood meter — exposed for UI
    public float BloodCurrent { get; private set; }
    public float BloodMax     => maxBlood;

    public override string AbilityName   => Loc.Get("ability_bat_form");
    public override float  UIFillPercent => maxBlood > 0f ? BloodCurrent / maxBlood : 0f;
    public override string StatusText    => BloodCurrent > 0f ? $"{BloodCurrent:F1}" : Loc.Get("status_no_blood");
    public override Color  BarColor      => new Color(0.75f, 0.05f, 0.05f); // blood red

    private SpriteRenderer            _sr;
    private Animator                  _animator;
    private Rigidbody2D               _rb;
    // Captured at EnterBatForm time (not OnEnable) so CharacterManager color is already applied
    private Sprite                    _savedSprite;
    private Color                     _savedColor;
    private RuntimeAnimatorController _savedController;

    private void OnEnable()
    {
        _sr           = GetComponent<SpriteRenderer>();
        _animator     = GetComponent<Animator>();
        _rb           = GetComponent<Rigidbody2D>();
        BloodCurrent  = maxBlood * startingBloodPercent;

        if (GameManager.Instance != null)
            GameManager.Instance.OnEnemyKilled += OnKill;
    }

    private void OnDisable()
    {
        ExitBatForm();

        if (GameManager.Instance != null)
            GameManager.Instance.OnEnemyKilled -= OnKill;
    }

    protected override void Update()
    {
        // No base.Update() — VampireAbility uses blood meter, not a time-based cooldown

        if (IsInBatForm)
        {
            BloodCurrent -= bloodDrainPerSec * Time.deltaTime;

            if (BloodCurrent <= 0f || IsAnyUIOpen)
            {
                BloodCurrent = Mathf.Max(0f, BloodCurrent);
                ExitBatForm();
                return;
            }

            // Press Space again to exit bat form
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ExitBatForm();
                return;
            }

            // Flip sprite based on horizontal movement direction
            if (_sr != null && PlayerController.Instance != null)
            {
                float moveX = PlayerController.Instance.MoveDirection.x;
                if (moveX != 0f)
                    _sr.flipX = moveX < 0f;
            }

            // Scale animation speed with movement speed
            if (_animator != null && _rb != null)
            {
                float maxSpeed    = PlayerStats.Instance != null
                    ? PlayerStats.Instance.MoveSpeed * batSpeedMultiplier
                    : 5f * batSpeedMultiplier;
                float velocityRatio  = Mathf.Clamp01(_rb.velocity.magnitude / maxSpeed);
                _animator.speed      = Mathf.Lerp(0.8f, batSpeedMultiplier, Mathf.Sqrt(velocityRatio));
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) && BloodCurrent > 0f && !IsAnyUIOpen)
                EnterBatForm();
        }
    }

    private void OnKill()
    {
        BloodCurrent = Mathf.Min(BloodCurrent + bloodPerKill, maxBlood);
    }

    private void EnterBatForm()
    {
        IsInBatForm = true;
        AudioManager.Instance?.PlaySFX(enterBatSound);

        // Capture current visuals NOW (CharacterManager has already set placeholder color)
        if (_sr != null)
        {
            _savedSprite = _sr.sprite;
            _savedColor  = _sr.color;
            if (batSprite != null)
            {
                _sr.sprite = batSprite;
                _sr.color  = Color.white;
            }
        }

        if (_animator != null && batAnimatorController != null)
        {
            _savedController = _animator.runtimeAnimatorController;
            _animator.enabled = true;
            _animator.runtimeAnimatorController = batAnimatorController;
            _animator.SetBool("IsMoving", true);
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.AbilitySpeedMultiplier      = batSpeedMultiplier;
            PlayerController.Instance.IsAbilityControllingAnimation = true;
        }

        BatEnemy.AlliesActive = true;
    }

    private void ExitBatForm()
    {
        if (!IsInBatForm) return;
        IsInBatForm = false;
        AudioManager.Instance?.PlaySFX(exitBatSound);

        // Restore visuals before re-enabling animator so it renders the correct sprite
        if (_sr != null)
        {
            _sr.sprite = _savedSprite;
            _sr.color  = _savedColor;
            _sr.flipX  = false;
        }

        // Restore the vampire animator controller
        if (_animator != null)
        {
            _animator.runtimeAnimatorController = _savedController;
            _animator.enabled = _savedController != null;
            if (_animator.enabled)
            {
                _animator.SetBool("IsMoving", false);
                _animator.SetFloat("MoveX", 0f);
                _animator.SetFloat("MoveY", -1f);
            }
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.AbilitySpeedMultiplier        = 1f;
            PlayerController.Instance.IsAbilityControllingAnimation = false;
        }

        BatEnemy.AlliesActive = false;
    }
}
