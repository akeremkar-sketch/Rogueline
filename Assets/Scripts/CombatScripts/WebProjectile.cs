using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed    = 4f;
    [SerializeField] private LayerMask wallLayers;
    [SerializeField] private float lifetime = 3f;

    [Header("Slow")]
    [SerializeField] [Range(0f, 1f)] private float slowAmount   = 0.8f;
    [SerializeField] private float slowDuration = 3f;

    [Header("Open Animation")]
    [SerializeField] private float openDuration = 0.4f;

    private Rigidbody2D rb;
    private bool  hasHit = false;
    private float timer  = 0f;

    // ── Pool ────────────────────────────────────────────────────────────────────
    private static readonly Queue<WebProjectile> _pool = new Queue<WebProjectile>();

    public static WebProjectile Rent(GameObject prefab, Vector3 position)
    {
        while (_pool.Count > 0)
        {
            WebProjectile w = _pool.Dequeue();
            if (w == null) continue;
            w.transform.position = position;
            w.gameObject.SetActive(true);
            return w;
        }
        return Instantiate(prefab, position, Quaternion.identity).GetComponent<WebProjectile>();
    }

    private void ReturnToPool()
    {
        StopAllCoroutines();
        rb.velocity = Vector2.zero;
        hasHit = false;
        timer  = 0f;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
        _pool.Enqueue(this);
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.localScale = Vector3.zero;
    }

    public void Launch(Vector2 direction)
    {
        rb.velocity = direction * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        StartCoroutine(LifetimeRoutine());
    }

    private void Update()
    {
        if (hasHit) return;
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / lifetime);
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        if (!hasHit)
            StartCoroutine(OpenAndDestroy());
    }

    private IEnumerator OpenAndDestroy()
    {
        rb.velocity = Vector2.zero;
        transform.localScale = Vector3.one;
        yield return new WaitForSeconds(openDuration);
        ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (!other.CompareTag("Player"))
        {
            if ((wallLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                hasHit = true;
                ReturnToPool();
            }
            return;
        }

        hasHit = true;
        rb.velocity = Vector2.zero;
        transform.localScale = Vector3.one;

        PlayerHealth.Instance?.ApplyWebSlow(slowAmount, slowDuration);

        if (FloatingTextManager.Instance != null && PlayerController.Instance != null)
            FloatingTextManager.Instance.Show(Loc.Get("status_slowed"), PlayerController.Instance.transform.position, new Color(0.5f, 0.2f, 1f));

        StartCoroutine(OpenAndDestroy());
    }
}
