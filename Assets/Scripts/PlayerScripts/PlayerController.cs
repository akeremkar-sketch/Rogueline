using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Animation Settings")]
    [SerializeField] private float baseAnimSpeed = 1f;

    // Silahların yön bilgisini alması için
    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;

        bool isMoving = moveInput != Vector2.zero;
        animator.SetBool("IsMoving", isMoving);

        float speedRatio = PlayerStats.Instance.MoveSpeed / 5f;
        animator.speed = isMoving ? baseAnimSpeed * speedRatio : baseAnimSpeed;

        if (isMoving)
        {
            if (Mathf.Abs(y) >= Mathf.Abs(x))
            {
                animator.SetFloat("MoveX", 0f);
                animator.SetFloat("MoveY", y > 0 ? 1f : -1f);
                FacingDirection = new Vector2(0f, y > 0 ? 1f : -1f);
            }
            else
            {
                animator.SetFloat("MoveX", x > 0 ? 1f : -1f);
                animator.SetFloat("MoveY", 0f);
                FacingDirection = new Vector2(x > 0 ? 1f : -1f, 0f);
            }
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = moveInput * PlayerStats.Instance.MoveSpeed;
    }
}