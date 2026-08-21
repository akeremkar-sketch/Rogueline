using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class DestructibleObject : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 1f;

    [Header("Drops")]
    [SerializeField] private GameObject   coinPrefab;
    [SerializeField] private GameObject   xpOrbPrefab;
    [SerializeField] private int          xpDropValue    = 10;
    [SerializeField] private int          coinDropAmount = 1;
    [SerializeField] private GameObject[] itemDropPrefabs;
    [SerializeField] [Range(0f, 1f)] private float itemDropChance = 0.10f;

    [Header("Break")]
    [SerializeField] private string breakTrigger    = "Break";
    [SerializeField] private bool   destroyOnBreak  = true;
    [SerializeField] private float  destroyDelay    = 0.6f;

    [Header("Animation")]
    [SerializeField] private float animationSpeed = 3f;

    [Header("Interact Prompt")]
    [SerializeField] private float          promptVerticalOffset = 0.8f;
    [SerializeField] private float          promptFontSize       = 0.1f;
    [SerializeField] private TMP_FontAsset  promptFont;
    [SerializeField] private Color          promptColor          = new Color(0.9f, 0.75f, 0.1f);
    [SerializeField] private float          pulseSpeed           = 3f;
    [SerializeField] private float          pulseMin             = 0.9f;
    [SerializeField] private float          pulseMax             = 1.35f;

    private float      _health;
    private bool       _broken;
    private Collider2D _col;
    private Animator   _animator;
    private GameObject _promptGO;

    private void Awake()
    {
        _health   = maxHealth;
        _col      = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();
        BuildPrompt();
    }

    private void Start()
    {
        DestructibleManager.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        DestructibleManager.Instance?.Unregister(this);
    }

    private void Update()
    {
        if (_promptGO != null && _promptGO.activeSelf)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float s = Mathf.Lerp(pulseMin, pulseMax, t);
            _promptGO.transform.localScale = Vector3.one * s;
        }
    }

    // ── Interact Prompt ────────────────────────────────────────────────────────

    private void BuildPrompt()
    {
        _promptGO = new GameObject("InteractPrompt");
        _promptGO.transform.SetParent(transform);
        _promptGO.transform.localPosition = Vector3.up * promptVerticalOffset;
        _promptGO.transform.localRotation = Quaternion.identity;
        _promptGO.transform.localScale    = Vector3.one;

        var tmp                = _promptGO.AddComponent<TextMeshPro>();
        tmp.text               = "[E]";
        tmp.alignment          = TextAlignmentOptions.Center;
        tmp.enableAutoSizing   = false;
        tmp.fontSize           = promptFontSize;
        tmp.color              = promptColor;
        if (promptFont != null) tmp.font = promptFont;
        tmp.sortingLayerID     = SortingLayer.NameToID("Default");
        tmp.sortingOrder       = 10;

        _promptGO.SetActive(false);
    }

    public void ShowPrompt(bool show)
    {
        if (_promptGO != null && !_broken)
        {
            _promptGO.SetActive(show);
            if (show) _promptGO.transform.localScale = Vector3.one;
        }
    }

    // ── Damage / Break ─────────────────────────────────────────────────────────

    public void TakeDamage(float dmg)
    {
        if (_broken) return;
        _health -= dmg;
        if (_health <= 0f) Break();
    }

    public void Interact() => Break();

    private void Break()
    {
        _broken      = true;
        _col.enabled = false;

        if (_promptGO != null) _promptGO.SetActive(false);

        DestructibleManager.Instance?.Unregister(this);

        if (_animator != null)
        {
            _animator.speed = animationSpeed;
            _animator.SetTrigger(breakTrigger);
        }

        SpawnDrops();
        if (destroyOnBreak) StartCoroutine(BreakSequence());
    }

    private IEnumerator BreakSequence()
    {
        yield return new WaitForSeconds(destroyDelay);

        // Freeze on last frame — stays until chunk is destroyed
        if (_animator != null) _animator.speed = 0f;
    }

    private void SpawnDrops()
    {
        Vector2 pos = transform.position;

        if (coinPrefab  != null) DropSystem.Instance?.SpawnCoins(coinPrefab,  pos, coinDropAmount);
        if (xpOrbPrefab != null) DropSystem.Instance?.SpawnXP(xpOrbPrefab,    pos, xpDropValue);

        if (itemDropPrefabs != null && itemDropPrefabs.Length > 0 && Random.value < itemDropChance)
        {
            GameObject prefab = itemDropPrefabs[Random.Range(0, itemDropPrefabs.Length)];
            Instantiate(prefab, pos + Random.insideUnitCircle * 0.3f, Quaternion.identity);
        }
    }
}
