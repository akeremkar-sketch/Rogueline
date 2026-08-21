using System.Collections;
using UnityEngine;

public class SpeedPotion : MonoBehaviour
{
    [SerializeField] private float     speedBoost = 50f;
    [SerializeField] private float     duration   = 5f;
    [SerializeField] private AudioClip useSound;

    private CameraController _cam;

    private void Start()
    {
        _cam = CameraController.Instance;
    }

    private void Update()
    {
        if (_cam != null && transform.position.y < _cam.CameraBottomY - 2f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance?.PlaySFX(useSound, SoundCategory.Pickup);
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.StartCoroutine(TemporaryBoost());
            Destroy(gameObject);
        }
    }

    private IEnumerator TemporaryBoost()
    {
        if (PlayerStats.Instance == null) yield break;
        float before = PlayerStats.Instance.MoveSpeedPoints;
        PlayerStats.Instance.ModifyStat(StatType.MoveSpeed, speedBoost);
        float actualAdded = PlayerStats.Instance.MoveSpeedPoints - before;
        yield return new WaitForSeconds(duration);
        if (PlayerStats.Instance != null && actualAdded != 0f)
            PlayerStats.Instance.ModifyStat(StatType.MoveSpeed, -actualAdded);
    }
}
