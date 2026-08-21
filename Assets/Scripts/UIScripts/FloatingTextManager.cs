using System.Collections.Generic;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private int          initialPoolSize = 20;

    private readonly Queue<FloatingText> _pool = new Queue<FloatingText>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Pre-warm pool
        for (int i = 0; i < initialPoolSize; i++)
            _pool.Enqueue(CreateInstance());
    }

    public void Show(string text, Vector3 worldPos, Color color, float sizeMultiplier = 1f)
    {
        if (floatingTextPrefab == null) return;

        FloatingText ft = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();
        Vector3 offset  = new Vector3(Random.Range(-0.3f, 0.3f), 0.3f, -1f);
        ft.transform.position = worldPos + offset;
        ft.Spawn(text, color, sizeMultiplier);
    }

    public void ReturnToPool(FloatingText ft)
    {
        ft.gameObject.SetActive(false);
        _pool.Enqueue(ft);
    }

    private FloatingText CreateInstance()
    {
        FloatingText ft = Instantiate(floatingTextPrefab, transform);
        ft.gameObject.SetActive(false);
        return ft;
    }
}
