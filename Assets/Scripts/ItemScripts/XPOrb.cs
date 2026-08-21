using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [SerializeField] private int         xpValue       = 10;
    [SerializeField] private AudioClip[] collectSounds;

    private CameraController _cam;
    private GameObject       _sourcePrefab;
    private int              _frameOffset;

    public void SetXPValue(int value) => xpValue = value;

    public void Init(GameObject sourcePrefab, int xp)
    {
        _sourcePrefab = sourcePrefab;
        xpValue       = xp;
    }

    private void Start()
    {
        _cam         = CameraController.Instance;
        _frameOffset = GetInstanceID() & 0x7; // 0-7 spread
    }

    private void Update()
    {
        if ((Time.frameCount + _frameOffset) % 8 != 0) return;
        if (_cam != null && transform.position.y < _cam.CameraBottomY - 2f)
            ReturnOrDestroy();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectSounds != null && collectSounds.Length > 0)
                AudioManager.Instance?.PlaySFX(collectSounds[Random.Range(0, collectSounds.Length)], SoundCategory.Pickup);
            LevelSystem.Instance?.AddXP(xpValue);
            ReturnOrDestroy();
        }
    }

    private void ReturnOrDestroy()
    {
        if (_sourcePrefab != null && DropSystem.Instance != null)
            DropSystem.Instance.ReturnCollectible(_sourcePrefab, gameObject);
        else
            Destroy(gameObject);
    }
}
