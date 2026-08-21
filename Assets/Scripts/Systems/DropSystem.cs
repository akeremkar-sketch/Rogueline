using System.Collections.Generic;
using UnityEngine;

public class DropSystem : MonoBehaviour
{
    public static DropSystem Instance { get; private set; }

    private readonly Dictionary<int, Queue<GameObject>> _pools    = new();
    private readonly Dictionary<int, GameObject>        _prefabMap = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ── Pool helpers ────────────────────────────────────────────────────────────

    private GameObject Rent(GameObject prefab, Vector2 position)
    {
        int id = prefab.GetInstanceID();
        _prefabMap.TryAdd(id, prefab);

        if (_pools.TryGetValue(id, out var pool))
        {
            while (pool.Count > 0)
            {
                GameObject recycled = pool.Dequeue();
                if (recycled == null) continue; // destroyed externally — skip
                recycled.transform.position = position;
                recycled.SetActive(true);
                return recycled;
            }
        }

        return Instantiate(prefab, position, Quaternion.identity);
    }

    public void ReturnCollectible(GameObject prefab, GameObject obj)
    {
        int id = prefab.GetInstanceID();
        obj.SetActive(false);

        if (!_pools.TryGetValue(id, out var pool))
        {
            pool = new Queue<GameObject>();
            _pools[id] = pool;
        }
        pool.Enqueue(obj);
    }

    // ── Drop rolls ──────────────────────────────────────────────────────────────

    public int RollDropAmount()
    {
        if (PlayerStats.Instance == null) return 1;
        float luck = Mathf.Clamp(PlayerStats.Instance.Luck, 0f, 3f);

        float chance0 = Mathf.Max(0f, 50f - luck * 15f);
        float chance1 = 25f;
        float chance2 = 15f + luck * 8f;
        float chance3 = 10f + luck * 7f;

        float total = chance0 + chance1 + chance2 + chance3;
        float roll  = Random.Range(0f, total);

        if (roll < chance0)                         return 0;
        if (roll < chance0 + chance1)               return 1;
        if (roll < chance0 + chance1 + chance2)     return 2;
        return 3;
    }

    // ── Spawn ───────────────────────────────────────────────────────────────────

    public void SpawnCoins(GameObject coinPrefab, Vector2 position, int baseAmount = 1)
    {
        int amount = Mathf.RoundToInt(RollDropAmount() * baseAmount);
        for (int i = 0; i < amount; i++)
        {
            Vector2    offset = Random.insideUnitCircle * 0.5f;
            GameObject obj    = Rent(coinPrefab, position + offset);
            obj.GetComponent<Collectible>()?.Init(coinPrefab);
            obj.GetComponent<Coin>()?.Init(coinPrefab);
        }
    }

    public void SpawnXP(GameObject xpOrbPrefab, Vector2 position, int xpDropValue)
    {
        int amount = RollDropAmount();
        if (amount <= 0) return;

        float luckMult  = PlayerStats.Instance != null ? 1f + PlayerStats.Instance.Luck : 1f;
        int   totalXP   = Mathf.RoundToInt(xpDropValue * luckMult);
        int   xpPerOrb  = totalXP / amount;
        int   remainder = totalXP % amount;

        for (int i = 0; i < amount; i++)
        {
            Vector2    offset = Random.insideUnitCircle * 0.5f;
            GameObject obj    = Rent(xpOrbPrefab, position + offset);
            obj.GetComponent<XPOrb>()?.Init(xpOrbPrefab, xpPerOrb + (i == 0 ? remainder : 0));
        }
    }
}
