using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float startingHealthPercent = 1f; // 1 = %100, 0.3 = %30

    private float currentHealth;

    private void Start()
    {
        currentHealth = PlayerStats.Instance.MaxHealth * startingHealthPercent;
    }

    public void TakeDamage(float damage)
    {
        float reducedDamage = damage - PlayerStats.Instance.Defense;
        reducedDamage = Mathf.Max(reducedDamage, 0f);
        currentHealth -= reducedDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, PlayerStats.Instance.MaxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, PlayerStats.Instance.MaxHealth);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => PlayerStats.Instance.MaxHealth;

    private void Die()
    {
        Debug.Log("Player öldü!");
    }
}