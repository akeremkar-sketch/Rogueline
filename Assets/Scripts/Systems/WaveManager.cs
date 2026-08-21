using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Timing")]
    [SerializeField] private float firstWaveDelay   = 20f;
    [SerializeField] private float baseWaveDuration = 20f;
    [SerializeField] private float baseCooldown     = 20f;
    [Tooltip("+5s added to both wave duration and cooldown per completed wave")]
    [SerializeField] private float durationStep     = 5f;

    [Header("Spawn Settings")]
    [Tooltip("Seconds between each spawn burst during a wave")]
    [SerializeField] private float spawnInterval      = 3f;
    [Tooltip("Enemies spawned per burst in wave 1")]
    [SerializeField] private int   baseEnemiesPerBurst = 2;
    [Tooltip("Additional enemies per burst added each wave")]
    [SerializeField] private int   burstStep           = 1;

    [Header("Warning Indicator")]
    [Tooltip("Total duration of the flashing X warning before an enemy spawns")]
    [SerializeField] private float warningDuration = 1.5f;
    [Tooltip("Number of times the X flashes during the warning")]
    [SerializeField] private int   warningFlashes  = 3;
    [SerializeField] private Sprite warningIcon    = null;  // assign later to replace the default ✕
    [SerializeField] private float  warningFontSize = 6f;
    [SerializeField] private float  warningIconScale = 1.5f;
    [SerializeField] private Color  warningColor    = new Color(1f, 0.15f, 0.15f, 1f);

    // ── Public state (read by WaveHUD) ───────────────────────────────────────────

    public int   CurrentWave   { get; private set; }
    public int   NextWave      => CurrentWave + 1;
    public float TimeRemaining { get; private set; }
    public bool  IsWaveActive  { get; private set; }

    // ── Internal state ───────────────────────────────────────────────────────────

    private enum State { WaitingForFirst, Cooldown, Active }

    private State _state      = State.WaitingForFirst;
    private float _timer;
    private float _spawnTimer;

    // Cached lists — reused every burst to avoid per-burst GC allocations
    private readonly List<(SpawnZone zone, EnemySpawnEntry[] entries)> _spawnSlots =
        new List<(SpawnZone, EnemySpawnEntry[])>();
    private readonly List<(float bottomY, Chunk chunk)> _sorted =
        new List<(float, Chunk)>();
    private readonly List<EnemySpawnEntry> _available = new List<EnemySpawnEntry>();

    // ── Lifecycle ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        _timer = firstWaveDelay;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        TimeRemaining = Mathf.Max(0f, _timer);

        switch (_state)
        {
            case State.WaitingForFirst:
            case State.Cooldown:
                if (_timer <= 0f)
                    BeginWave();
                break;

            case State.Active:
                _spawnTimer -= Time.deltaTime;
                if (_spawnTimer <= 0f)
                {
                    _spawnTimer = spawnInterval;
                    TriggerSpawnBurst();
                }
                if (_timer <= 0f)
                    EndWave();
                break;
        }
    }

    // ── Wave state transitions ───────────────────────────────────────────────────

    private void BeginWave()
    {
        CurrentWave++;
        IsWaveActive = true;
        _state       = State.Active;
        _timer       = baseWaveDuration + (CurrentWave - 1) * durationStep;
        _spawnTimer  = 0f; // immediate first burst
    }

    private void EndWave()
    {
        IsWaveActive = false;
        _state       = State.Cooldown;
        _timer       = baseCooldown + (CurrentWave - 1) * durationStep;
    }

    // ── Spawning ─────────────────────────────────────────────────────────────────

    private void TriggerSpawnBurst()
    {
        if (BossChunk.IsActive) return;

        BuildSpawnSlots();
        if (_spawnSlots.Count == 0) return;

        int   burst    = baseEnemiesPerBurst + (CurrentWave - 1) * burstStep;
        float bonus    = (CurrentWave - 1) * 0.05f;
        float gameDist = GameManager.Instance != null ? GameManager.Instance.Distance : 0f;

        for (int i = 0; i < burst; i++)
        {
            if (EnemyManager.Instance != null && !EnemyManager.Instance.CanSpawnMore) break;

            var (zone, entries) = _spawnSlots[Random.Range(0, _spawnSlots.Count)];
            if (zone.center == null || entries == null || entries.Length == 0) continue;

            _available.Clear();
            foreach (var e in entries)
            {
                if (e.prefab != null
                    && gameDist >= e.minDistance
                    && gameDist <= e.maxDistance)
                    _available.Add(e);
            }
            if (_available.Count == 0) continue;

            EnemySpawnEntry picked = PickEntry(_available, bonus);
            if (picked == null) continue;

            Vector2 pos = RandomPosInZone(zone.center.position, zone.size);
            StartCoroutine(SpawnWithWarning(pos, picked.prefab));
        }
    }

    // Finds SpawnZones in the chunk the player is currently in + the one directly above.
    // Writes into cached _spawnSlots list — no allocations.
    private void BuildSpawnSlots()
    {
        _spawnSlots.Clear();

        if (ChunkManager.Instance == null || PlayerController.Instance == null) return;

        IReadOnlyList<GameObject> active = ChunkManager.Instance.GetActiveChunks();
        float playerY = PlayerController.Instance.transform.position.y;

        _sorted.Clear();
        foreach (var go in active)
        {
            if (go == null) continue;
            Chunk c = go.GetComponent<Chunk>();
            if (c == null || c.SpawnZones == null || c.SpawnZones.Length == 0) continue;
            _sorted.Add((go.transform.position.y, c));
        }
        if (_sorted.Count == 0) return;
        _sorted.Sort((a, b) => a.bottomY.CompareTo(b.bottomY));

        int currentIdx = -1;
        for (int i = 0; i < _sorted.Count; i++)
        {
            float top = _sorted[i].bottomY + _sorted[i].chunk.ChunkHeight;
            if (playerY >= _sorted[i].bottomY && playerY < top)
            {
                currentIdx = i;
                break;
            }
        }
        if (currentIdx == -1)
        {
            for (int i = _sorted.Count - 1; i >= 0; i--)
            {
                if (_sorted[i].bottomY <= playerY) { currentIdx = i; break; }
            }
        }

        if (currentIdx < 0) return;

        AppendZones(_sorted[currentIdx].chunk, _spawnSlots);
        if (currentIdx + 1 < _sorted.Count)
            AppendZones(_sorted[currentIdx + 1].chunk, _spawnSlots);
    }

    private static void AppendZones(Chunk chunk, List<(SpawnZone, EnemySpawnEntry[])> result)
    {
        if (chunk == null || chunk.SpawnZones == null || chunk.EnemyEntries == null) return;
        foreach (SpawnZone zone in chunk.SpawnZones)
            if (zone.center != null)
                result.Add((zone, chunk.EnemyEntries));
    }

    // ── Warning coroutine ────────────────────────────────────────────────────────

    // Visual and spawn are in separate coroutines: if TMP throws, the visual coroutine
    // stops but SpawnWithWarning continues and still instantiates the enemy.
    private IEnumerator SpawnWithWarning(Vector2 pos, GameObject prefab)
    {
        StartCoroutine(ShowWarningVisual(pos));
        yield return new WaitForSeconds(warningDuration);

        if (EnemyManager.Instance != null && EnemyManager.Instance.CanSpawnMore)
            Instantiate(prefab, pos, Quaternion.identity);
    }

    private IEnumerator ShowWarningVisual(Vector2 pos)
    {
        var go = new GameObject("WaveWarning");
        go.transform.position = (Vector3)pos + new Vector3(0f, 0f, -0.5f);

        if (warningIcon != null)
        {
            // Custom icon assigned in inspector
            var sr       = go.AddComponent<SpriteRenderer>();
            sr.sprite    = warningIcon;
            sr.color     = warningColor;
            sr.sortingOrder = 10;
            go.transform.localScale = Vector3.one * warningIconScale;
        }
        else
        {
            // Default: red ✕ text
            var tmp         = go.AddComponent<TMPro.TextMeshPro>();
            tmp.text        = "✕";
            tmp.fontSize    = warningFontSize;
            tmp.color       = warningColor;
            tmp.alignment   = TMPro.TextAlignmentOptions.Center;
            tmp.sortingOrder = 10;
        }

        float half = warningDuration / (warningFlashes * 2f);
        for (int i = 0; i < warningFlashes; i++)
        {
            go.SetActive(true);
            yield return new WaitForSeconds(half);
            go.SetActive(false);
            yield return new WaitForSeconds(half);
        }
        if (go != null) Destroy(go);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static EnemySpawnEntry PickEntry(List<EnemySpawnEntry> list, float bonus)
    {
        // Fisher-Yates shuffle then find first entry that passes its chance roll
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);

        }
        foreach (var e in list)
            if (Random.value < Mathf.Clamp01(e.spawnChance + bonus))
                return e;
        return list[Random.Range(0, list.Count)]; // guaranteed fallback
    }

    private static Vector2 RandomPosInZone(Vector2 center, Vector2 size) =>
        new Vector2(
            center.x + Random.Range(-size.x * 0.5f, size.x * 0.5f),
            center.y + Random.Range(-size.y * 0.5f, size.y * 0.5f)
        );
}
