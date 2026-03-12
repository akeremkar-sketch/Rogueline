using UnityEngine;

public class HealthTest : MonoBehaviour
{
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        // H tuşuna basınca 10 hasar al
        if (Input.GetKeyDown(KeyCode.H))
            playerHealth.TakeDamage(10f);

        // G tuşuna basınca 10 can kazan
        if (Input.GetKeyDown(KeyCode.G))
            playerHealth.Heal(10f);
    }
}