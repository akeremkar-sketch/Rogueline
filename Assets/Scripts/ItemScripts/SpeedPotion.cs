using UnityEngine;

public class SpeedPotion : MonoBehaviour
{
    [SerializeField] private float speedBonus = 0.05f; // %5 hız bonusu

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats.Instance.AddMoveSpeedBonus(speedBonus);
            Destroy(gameObject);
        }
    }
}