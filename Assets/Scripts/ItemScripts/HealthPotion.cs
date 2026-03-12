using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [SerializeField] private float healAmount = 20f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().Heal(healAmount);
            Destroy(gameObject);
        }
    }
}