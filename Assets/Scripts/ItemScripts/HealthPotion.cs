using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [SerializeField] private float     healAmount = 20f;
    [SerializeField] private AudioClip useSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance?.PlaySFX(useSound, SoundCategory.Pickup);
            PlayerHealth.Instance?.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}