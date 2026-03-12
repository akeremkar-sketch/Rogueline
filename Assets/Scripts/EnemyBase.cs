using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] protected float maxHealth = 50f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float enemyDamage = 10f;
    [SerializeField] protected float attackRange = 0.5f;
    [SerializeField] protected float attackCooldown = 1f;

    [Header("Drop Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int coinDropAmount = 3;
    [SerializeField] private int xpDropValue = 30;

    [Header("UI")]
    [SerializeField] private EnemyHealthBar healthBar;

    protected float currentHealth;
    protected float attackTimer = 0f;
    protected Transform player;
    protected Animator animator;
    private bool isDead = false;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.Register(this);

        if (healthBar != null)
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    protected virtual void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
            MoveTowardsPlayer();
        else
        {
            if (animator != null)
                animator.SetFloat("MoveX", 0f);

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                AttackPlayer();
            }
        }
    }

    protected virtual void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        if (animator != null)
            animator.SetFloat("MoveX", direction.x > 0 ? 1f : -1f);
    }

    protected virtual void AttackPlayer()
    {
        if (animator != null)
            animator.SetBool("IsAttacking", true);

        player.GetComponent<PlayerHealth>().TakeDamage(enemyDamage);
    }

    public virtual void TakeDamage(float incomingDamage)
    {
        if (isDead) return;

        currentHealth -= incomingDamage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (animator != null)
            animator.SetBool("IsHit", true);

        if (healthBar != null)
            healthBar.UpdateHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.Unregister(this);

        if (animator != null)
            animator.SetBool("IsDead", true);

        if (coinPrefab != null)
        {
            for (int i = 0; i < coinDropAmount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
                Instantiate(coinPrefab, (Vector2)transform.position + randomOffset, Quaternion.identity);
            }
        }

        if (xpOrbPrefab != null)
        {
            GameObject orb = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
            XPOrb xpOrb = orb.GetComponent<XPOrb>();
            if (xpOrb != null)
                xpOrb.SetXPValue(xpDropValue);
        }

        Destroy(gameObject, 1f);
    }
}