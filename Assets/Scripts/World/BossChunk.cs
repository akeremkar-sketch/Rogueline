using UnityEngine;

/// <summary>
/// Boss chunk component.
/// • Notifies CameraController of boss room mode when player enters.
/// • Returns to normal corridor when player exits (from the top).
/// • ChunkManager reads ChunkHeight from this component.
/// </summary>
public class BossChunk : MonoBehaviour
{
    [Header("Chunk Size")]
    [SerializeField] private float chunkHeight = 40f;
    [SerializeField] private float chunkWidth  = 40f;

    [Header("Boss Spawn")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform  bossSpawnPoint;

    public float ChunkHeight => chunkHeight;
    public float ChunkWidth  => chunkWidth;

    // True while player is inside any boss room — suppresses camera-based cleanup in managers
    public static bool IsActive { get; private set; } = false;

    private bool _playerInside = false;
    private bool _spawned      = false;
    private float _difficulty;

    public void Initialize(float difficulty)
    {
        _difficulty = difficulty;
    }

    private void Update()
    {
        if (PlayerController.Instance == null) return;

        float playerY  = PlayerController.Instance.transform.position.y;
        float bottomY  = transform.position.y;
        float topY     = bottomY + chunkHeight;

        // Camera lock / unlock
        bool inside = playerY >= bottomY && playerY < topY;

        if (inside && !_playerInside)
        {
            _playerInside = true;
            IsActive      = true;

            // Clear all enemies from previous chunks, then spawn boss
            EnemyManager.Instance?.ClearAllEnemies();
            if (!_spawned) { _spawned = true; SpawnBossEnemies(); }

            Vector2 center = new Vector2(transform.position.x, bottomY + chunkHeight * 0.5f);
            Vector2 size   = new Vector2(chunkWidth, chunkHeight);
            CameraController.Instance?.EnterBossRoom(center, size);
        }
        else if (!inside && _playerInside && playerY >= topY)
        {
            _playerInside = false;
            IsActive      = false;
            CameraController.Instance?.ExitBossRoom();
        }
    }

    protected virtual void SpawnBossEnemies()
    {
        if (bossPrefab == null) return;
        Vector3 pos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position + Vector3.up * chunkHeight * 0.5f;
        Instantiate(bossPrefab, pos, Quaternion.identity);
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position + new Vector3(0f, chunkHeight * 0.5f, 0f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.15f);
        Gizmos.DrawCube(center, new Vector3(chunkWidth, chunkHeight, 0.1f));
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(center, new Vector3(chunkWidth, chunkHeight, 0.1f));
    }
}
