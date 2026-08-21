using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossChunkEntry
{
    [Tooltip("Boss chunk spawns at this distance (meters)")]
    public float      distanceThreshold;
    [Tooltip("Boss chunk prefab to spawn")]
    public GameObject prefab;
}

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    [Header("Chunk Prefabs")]
    [SerializeField] private GameObject[] chunkPrefabs;

    [Header("Shop Chunk")]
    [SerializeField] private GameObject shopChunkPrefab;
    [SerializeField] private int        shopChunkInterval = 5; // every N chunks

    [Header("Boss Chunks")]
    [SerializeField] private BossChunkEntry[] bossChunks =
    {
        new BossChunkEntry { distanceThreshold = 1000f },
        new BossChunkEntry { distanceThreshold = 2500f },
        new BossChunkEntry { distanceThreshold = 5000f },
    };

    [Header("Settings")]
    [SerializeField] private float chunkHeight        = 20f;
    [SerializeField] private float spawnAheadDistance = 24f;
    [SerializeField] private float despawnDistance    = 10f;

    public int                        ChunksSpawned    { get; private set; } = 0;
    public IReadOnlyList<GameObject>  GetActiveChunks() => activeChunks;

    private readonly List<GameObject> activeChunks       = new List<GameObject>();
    private readonly List<float>      activeChunkHeights  = new List<float>();
    private float            nextSpawnY;
    private float            _startY;
    private int              _nextBossIndex = 0;
    private Transform        player;
    private CameraController _cam;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (PlayerController.Instance == null) return;

        player     = PlayerController.Instance.transform;
        _cam       = CameraController.Instance;
        _startY    = player.position.y;
        nextSpawnY = player.position.y - chunkHeight;

        while (nextSpawnY < player.position.y + spawnAheadDistance)
            SpawnChunk();
    }

    private void Update()
    {
        if (player == null) return;

        while (nextSpawnY < player.position.y + spawnAheadDistance)
            SpawnChunk();

        if (_cam == null) _cam = CameraController.Instance;
        if (_cam == null) return;
        float killLine = _cam.CameraBottomY - despawnDistance;

        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            if (activeChunks[i] == null)
            {
                activeChunks.RemoveAt(i);
                activeChunkHeights.RemoveAt(i);
                continue;
            }

            float chunkTopY = activeChunks[i].transform.position.y + activeChunkHeights[i];
            if (chunkTopY < killLine)
            {
                // Never destroy the active boss chunk while player is still inside it
                if (BossChunk.IsActive && activeChunks[i].GetComponent<BossChunk>() != null)
                    continue;

                Destroy(activeChunks[i]);
                activeChunks.RemoveAt(i);
                activeChunkHeights.RemoveAt(i);
            }
        }
    }

    private void SpawnChunk()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            nextSpawnY += chunkHeight;
            return;
        }

        float difficulty      = ChunksSpawned / 10f;
        float distanceFromStart = nextSpawnY - _startY;

        // Boss chunk priority: did we pass the next boss distance threshold?
        bool isBossChunk = bossChunks != null
                           && _nextBossIndex < bossChunks.Length
                           && bossChunks[_nextBossIndex].prefab != null
                           && distanceFromStart >= bossChunks[_nextBossIndex].distanceThreshold;

        GameObject prefab;
        if (isBossChunk)
        {
            prefab = bossChunks[_nextBossIndex].prefab;
            _nextBossIndex++;
        }
        else
        {
            // Every shopChunkInterval chunks (skip chunk 0), spawn a shop chunk
            bool isShopChunk = shopChunkPrefab != null
                               && ChunksSpawned > 0
                               && ChunksSpawned % shopChunkInterval == 0;

            prefab = isShopChunk
                ? shopChunkPrefab
                : chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
        }

        GameObject go = Instantiate(prefab, new Vector3(0f, Mathf.Round(nextSpawnY), 0f), Quaternion.identity);

        float spawnedHeight;
        if (isBossChunk)
        {
            BossChunk bossComp = go.GetComponent<BossChunk>();
            bossComp?.Initialize(difficulty);
            spawnedHeight = bossComp != null ? bossComp.ChunkHeight : chunkHeight;
        }
        else
        {
            Chunk chunk = go.GetComponent<Chunk>();
            chunk?.Initialize(difficulty);
            // Each chunk reports its own height; falls back to ChunkManager value if component missing
            spawnedHeight = chunk != null ? chunk.ChunkHeight : chunkHeight;
        }

        activeChunks.Add(go);
        activeChunkHeights.Add(spawnedHeight);
        nextSpawnY += spawnedHeight;
        ChunksSpawned++;
    }
}
