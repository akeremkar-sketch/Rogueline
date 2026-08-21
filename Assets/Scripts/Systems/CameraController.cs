using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private Transform target;
    [SerializeField] private float verticalOffset = 3f;
    [SerializeField] private float smoothSpeed    = 3f;

    [Header("Corridor Settings")]
    [Tooltip("Center X position of the corridor")]
    [SerializeField] private float corridorCenterX = 0f;
    [Tooltip("Player is considered in corridor mode when within this half-width")]
    [SerializeField] private float corridorHalfWidth = 9f;

    [Header("Side Room Settings")]
    [Tooltip("X follow speed in side rooms")]
    [SerializeField] private float sideRoomPanSpeed = 5f;

    private Camera cam;
    private bool inCorridor = true;

    // Boss room state
    private bool   _inBossRoom      = false;
    private Vector2 _bossRoomCenter = Vector2.zero;
    private Vector2 _bossRoomSize   = Vector2.zero;

    // Cached every LateUpdate — all systems read this, avoids accessing native cam property directly
    public float CameraBottomY { get; private set; }
    public float CameraTopY    { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (target == null && PlayerController.Instance != null)
            target = PlayerController.Instance.transform;

        transform.position = new Vector3(corridorCenterX, transform.position.y, transform.position.z);
    }

    /// <summary>
    /// Called when entering a boss room.
    /// Camera follows the player while staying within the room bounds.
    /// </summary>
    public void EnterBossRoom(Vector2 center, Vector2 size)
    {
        _inBossRoom     = true;
        _bossRoomCenter = center;
        _bossRoomSize   = size;
    }

    /// <summary>
    /// Called when leaving the boss room. Returns to normal corridor mode.
    /// </summary>
    public void ExitBossRoom()
    {
        _inBossRoom = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float halfH = cam != null ? cam.orthographicSize : 5f;
        CameraBottomY = transform.position.y - halfH;
        CameraTopY    = transform.position.y + halfH;

        float newX, newY;

        if (_inBossRoom)
        {
            // Boss room: follow player but clamp to room bounds
            float camHalfH = cam != null ? cam.orthographicSize : 5f;
            float camHalfW = cam != null ? cam.orthographicSize * cam.aspect : 8f;

            float minX = _bossRoomCenter.x - _bossRoomSize.x * 0.5f + camHalfW;
            float maxX = _bossRoomCenter.x + _bossRoomSize.x * 0.5f - camHalfW;
            float minY = _bossRoomCenter.y - _bossRoomSize.y * 0.5f + camHalfH;
            float maxY = _bossRoomCenter.y + _bossRoomSize.y * 0.5f - camHalfH;

            // If room is narrower than camera, lock to center
            float clampedX = minX <= maxX
                ? Mathf.Clamp(target.position.x, minX, maxX)
                : _bossRoomCenter.x;
            float clampedY = minY <= maxY
                ? Mathf.Clamp(target.position.y + verticalOffset, minY, maxY)
                : _bossRoomCenter.y;

            newX = Mathf.Lerp(transform.position.x, clampedX, sideRoomPanSpeed * Time.deltaTime);
            newY = Mathf.Lerp(transform.position.y, clampedY, smoothSpeed       * Time.deltaTime);
        }
        else
        {
            float playerX = target.position.x;
            inCorridor = playerX >= corridorCenterX - corridorHalfWidth &&
                         playerX <= corridorCenterX + corridorHalfWidth;

            if (inCorridor)
            {
                // Corridor: fixed X, Y only moves upward
                newX = Mathf.Lerp(transform.position.x, corridorCenterX, sideRoomPanSpeed * Time.deltaTime);

                float desiredY = target.position.y + verticalOffset;
                float targetY  = Mathf.Max(transform.position.y, desiredY);
                newY = Mathf.Lerp(transform.position.y, targetY, smoothSpeed * Time.deltaTime);
                newY = Mathf.Max(newY, transform.position.y);
            }
            else
            {
                // Side room (shop etc.): freely follow player
                newX = Mathf.Lerp(transform.position.x, playerX, sideRoomPanSpeed * Time.deltaTime);
                newY = Mathf.Lerp(transform.position.y, target.position.y + verticalOffset, smoothSpeed * Time.deltaTime);
            }
        }

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
