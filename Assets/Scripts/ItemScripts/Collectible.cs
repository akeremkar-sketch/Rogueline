using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Transform        player;
    private CameraController _cam;
    private bool             isAttracting   = false;
    private GameObject       _sourcePrefab;
    private float            _sqrPickupRadius;
    private int              _frameOffset;

    public void Init(GameObject sourcePrefab)
    {
        _sourcePrefab  = sourcePrefab;
        isAttracting   = false;
        _frameOffset   = GetInstanceID() & 0xF; // 0-15 spread
        CachePickupRadius();
    }

    private void Start()
    {
        if (PlayerController.Instance != null)
            player = PlayerController.Instance.transform;
        _cam = CameraController.Instance;
        CachePickupRadius();
    }

    private void CachePickupRadius()
    {
        float r = PlayerStats.Instance != null ? PlayerStats.Instance.PickupRadius : 1f;
        _sqrPickupRadius = r * r;
    }

    private void Update()
    {
        if (player == null) return;

        // Move every frame — required for smooth appearance
        if (isAttracting)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            return;
        }

        // Camera and distance check: every 5 frames (approx. 80ms)
        if ((Time.frameCount + _frameOffset) % 5 != 0) return;

        if (_cam != null && transform.position.y < _cam.CameraBottomY - 2f)
        {
            ReturnOrDestroy();
            return;
        }

        float sqrDist = ((Vector2)transform.position - (Vector2)player.position).sqrMagnitude;
        if (sqrDist <= _sqrPickupRadius) isAttracting = true;
    }

    public void ReturnOrDestroy()
    {
        if (_sourcePrefab != null && DropSystem.Instance != null)
            DropSystem.Instance.ReturnCollectible(_sourcePrefab, gameObject);
        else
            Destroy(gameObject);
    }
}
