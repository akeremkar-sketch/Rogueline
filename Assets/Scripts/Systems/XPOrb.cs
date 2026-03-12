using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [SerializeField] private int xpValue = 10;

    // EnemyBase'in dinamik olarak toplam XP değerini set etmesi için
    public void SetXPValue(int value) => xpValue = value;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LevelSystem.Instance.AddXP(xpValue);
            Destroy(gameObject);
        }
    }
}
