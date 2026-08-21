using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnZone
{
    public Transform center;
    public Vector2   size     = new Vector2(4f, 4f);
    public int       maxCount = 3;
}

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    [Range(0f, 1f)]
    public float      spawnChance = 0.5f;
    public float      minDistance = 0f;
    public float      maxDistance = 999999f;
}

[System.Serializable]
public class DestructibleZone
{
    public Transform   center;
    public Vector2     size        = new Vector2(6f, 4f);
    public int         maxCount    = 4;
    [Range(0f, 1f)]
    public float       spawnChance = 0.7f;   // separate chance per slot
    public GameObject[] prefabs;             // randomly picked from this
    [Tooltip("Minimum spacing between objects (prevents overlapping)")]
    public float       minSpacing  = 1.2f;
}

public class Chunk : MonoBehaviour
{
    [Header("Enemy Spawn")]
    [SerializeField] private SpawnZone[]       spawnZones;
    [SerializeField] private EnemySpawnEntry[] enemyEntries;

    [Header("Destructible Spawn")]
    [SerializeField] private DestructibleZone[] destructibleZones;

    [Header("Item Spawn")]
    [SerializeField] private Transform[]  itemSpawnPoints;
    [SerializeField] private GameObject[] itemPrefabs;
    [Range(0f, 1f)]
    [SerializeField] private float        itemSpawnChance = 0.15f;

    [Header("Difficulty Scaling")]
    [Tooltip("Spawn chance increase per 1 difficulty unit")]
    [SerializeField] private float spawnChancePerDifficulty = 0.05f;
    [Tooltip("Max count multiplier increase per 1 difficulty unit")]
    [SerializeField] private float maxCountPerDifficulty    = 0.1f;

    [Header("Chunk Size")]
    [SerializeField] private float chunkHeight       = 20f;
    [Tooltip("Spawn triggers when player is this far from chunk base (1 chunk ahead = chunkHeight)")]
    [SerializeField] private float spawnLookAhead    = 16f;

    public float             ChunkHeight   => chunkHeight;
    public SpawnZone[]       SpawnZones    => spawnZones;
    public EnemySpawnEntry[] EnemyEntries  => enemyEntries;

    private float _difficulty;
    private bool  _spawned = false;

    // Reused per-spawn to avoid allocation
    private readonly List<Vector2> _takenPositions = new List<Vector2>();

    public void Initialize(float difficulty)
    {
        _difficulty = difficulty;
    }

    private void Update()
    {
        if (_spawned) return;
        if (PlayerController.Instance == null) return;

        float playerY      = PlayerController.Instance.transform.position.y;
        float chunkBottomY = transform.position.y;

        if (playerY >= chunkBottomY - spawnLookAhead)
        {
            _spawned = true;
            SpawnEnemies(_difficulty);
            SpawnDestructibles();
            SpawnItems();
            enabled = false;
        }
    }

    // ── Enemy Spawn ────────────────────────────────────────────────────────────

    private void SpawnEnemies(float difficulty)
    {
        if (enemyEntries == null || enemyEntries.Length == 0) return;
        if (spawnZones   == null || spawnZones.Length   == 0) return;

        float currentDistance  = GameManager.Instance  != null ? GameManager.Instance.Distance  : 0f;
        float playerDifficulty = PlayerStats.Instance  != null ? PlayerStats.Instance.Difficulty : 0f;
        float totalDifficulty  = difficulty + playerDifficulty;
        float chanceBonus      = totalDifficulty * spawnChancePerDifficulty;
        float countMultiplier  = 1f + totalDifficulty * maxCountPerDifficulty;

        var available = new List<EnemySpawnEntry>();
        foreach (var entry in enemyEntries)
        {
            if (entry.prefab != null
                && currentDistance >= entry.minDistance
                && currentDistance <= entry.maxDistance)
                available.Add(entry);
        }

        if (available.Count == 0) return;

        foreach (SpawnZone zone in spawnZones)
        {
            if (zone.center == null) continue;

            int effectiveCount = Mathf.Max(1, Mathf.RoundToInt(zone.maxCount * countMultiplier));
            for (int i = 0; i < effectiveCount; i++)
            {
                // Stop spawning if max enemy limit reached
                if (EnemyManager.Instance != null && !EnemyManager.Instance.CanSpawnMore) return;

                EnemySpawnEntry entry = PickEnemyEntry(available, chanceBonus);
                if (entry == null) continue;

                Vector2 pos = RandomPosInZone(zone.center.position, zone.size);
                Instantiate(entry.prefab, pos, Quaternion.identity);
            }
        }
    }

    private EnemySpawnEntry PickEnemyEntry(List<EnemySpawnEntry> available, float bonus)
    {
        // Fisher-Yates shuffle
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        foreach (var entry in available)
        {
            if (Random.value < Mathf.Clamp01(entry.spawnChance + bonus))
                return entry;
        }

        return null;
    }

    // ── Destructible Spawn ─────────────────────────────────────────────────────

    private void SpawnDestructibles()
    {
        if (destructibleZones == null || destructibleZones.Length == 0) return;

        foreach (DestructibleZone zone in destructibleZones)
        {
            if (zone.center == null) continue;
            if (zone.prefabs == null || zone.prefabs.Length == 0) continue;

            _takenPositions.Clear();

            for (int i = 0; i < zone.maxCount; i++)
            {
                if (Random.value > zone.spawnChance) continue;

                Vector2? pos = TryFindFreePos(zone);
                if (pos == null) continue;

                GameObject prefab = zone.prefabs[Random.Range(0, zone.prefabs.Length)];
                if (prefab == null) continue;

                // Parent = chunk → auto-destroyed when chunk is destroyed
                Instantiate(prefab, pos.Value, Quaternion.identity, transform);
                _takenPositions.Add(pos.Value);
            }
        }
    }

    /// <summary>
    /// Returns a random position in the zone that doesn't overlap with already-spawned objects.
    /// Returns null if no free position found within maxAttempts.
    /// </summary>
    private Vector2? TryFindFreePos(DestructibleZone zone, int maxAttempts = 10)
    {
        float minSqr = zone.minSpacing * zone.minSpacing;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 candidate = RandomPosInZone(zone.center.position, zone.size);

            bool clear = true;
            foreach (Vector2 taken in _takenPositions)
            {
                if ((candidate - taken).sqrMagnitude < minSqr)
                {
                    clear = false;
                    break;
                }
            }

            if (clear) return candidate;
        }

        return null;
    }

    // ── Item Spawn ─────────────────────────────────────────────────────────────

    private void SpawnItems()
    {
        if (itemPrefabs     == null || itemPrefabs.Length     == 0) return;
        if (itemSpawnPoints == null || itemSpawnPoints.Length == 0) return;

        foreach (Transform point in itemSpawnPoints)
        {
            if (Random.value < itemSpawnChance)
            {
                GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
                Instantiate(prefab, point.position, Quaternion.identity);
            }
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Vector2 RandomPosInZone(Vector2 center, Vector2 size)
    {
        return new Vector2(
            center.x + Random.Range(-size.x * 0.5f, size.x * 0.5f),
            center.y + Random.Range(-size.y * 0.5f, size.y * 0.5f)
        );
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Chunk boundary — white
        Vector3 chunkCenter = transform.position + new Vector3(0f, chunkHeight * 0.5f, 0f);
        Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
        Gizmos.DrawCube(chunkCenter, new Vector3(20f, chunkHeight, 0.1f));
        Gizmos.color = new Color(1f, 1f, 1f, 0.6f);
        Gizmos.DrawWireCube(chunkCenter, new Vector3(20f, chunkHeight, 0.1f));

        // Enemy zones — red
        if (spawnZones != null)
        {
            foreach (SpawnZone zone in spawnZones)
            {
                if (zone.center == null) continue;
                Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.35f);
                Gizmos.DrawCube(zone.center.position, new Vector3(zone.size.x, zone.size.y, 0.1f));
                Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.9f);
                Gizmos.DrawWireCube(zone.center.position, new Vector3(zone.size.x, zone.size.y, 0.1f));
            }
        }

        // Destructible zones — green
        if (destructibleZones != null)
        {
            foreach (DestructibleZone zone in destructibleZones)
            {
                if (zone.center == null) continue;
                Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.25f);
                Gizmos.DrawCube(zone.center.position, new Vector3(zone.size.x, zone.size.y, 0.1f));
                Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.9f);
                Gizmos.DrawWireCube(zone.center.position, new Vector3(zone.size.x, zone.size.y, 0.1f));
            }
        }
    }
}
