using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class ChestObject : MonoBehaviour
{
    [Header("Loot")]
    [SerializeField] private ItemData[] possibleItems;

    [Header("Open Sprite")]
    [Tooltip("Sprite shown when the chest is opened — assign in Inspector")]
    [SerializeField] private Sprite openSprite;

    [Header("Interact Prompt")]
    [SerializeField] private float  interactRange        = 1.5f;
    [SerializeField] private float  promptVerticalOffset = 0.8f;
    [SerializeField] private float  promptFontSize       = 1.2f;
    [SerializeField] private Color  promptColor          = Color.yellow;
    [SerializeField] private TMP_FontAsset promptFont;
    [SerializeField] private float  pulseSpeed           = 3f;
    [SerializeField] private float  pulseMin             = 0.9f;
    [SerializeField] private float  pulseMax             = 1.35f;

    private bool           _opened;
    private bool           _transactionDone;
    private ItemData       _selectedItem;
    private SpriteRenderer _sr;
    private Animator       _anim;
    private Collider2D     _col;
    private bool           _playerInRange;
    private Transform      _player;
    private GameObject     _promptGO;

    private void Awake()
    {
        _sr  = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        _col  = GetComponent<Collider2D>();
        BuildPrompt();
    }

    private void BuildPrompt()
    {
        _promptGO = new GameObject("InteractPrompt");
        _promptGO.transform.SetParent(transform);
        _promptGO.transform.SetLocalPositionAndRotation(Vector3.up * promptVerticalOffset, Quaternion.identity);
        _promptGO.transform.localScale    = Vector3.one;

        var tmp              = _promptGO.AddComponent<TextMeshPro>();
        tmp.text             = "[E]";
        tmp.alignment        = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = false;
        tmp.fontSize         = promptFontSize;
        tmp.color            = promptColor;
        if (promptFont != null) tmp.font = promptFont;
        tmp.sortingLayerID   = SortingLayer.NameToID("Default");
        tmp.sortingOrder     = 10;

        _promptGO.SetActive(false);
    }

    private void ShowPrompt(bool show)
    {
        if (_promptGO != null) _promptGO.SetActive(show);
    }

    private void Update()
    {
        if (_player == null && PlayerController.Instance != null)
            _player = PlayerController.Instance.transform;

        if (_transactionDone || _player == null)
        {
            if (_playerInRange) { _playerInRange = false; ShowPrompt(false); }
            return;
        }

        bool inRange = Vector2.Distance(transform.position, _player.position) <= interactRange;

        if (inRange != _playerInRange)
        {
            _playerInRange = inRange;
            ShowPrompt(inRange);
        }

        if (_promptGO != null && _promptGO.activeSelf)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float s = Mathf.Lerp(pulseMin, pulseMax, t);
            _promptGO.transform.localScale = Vector3.one * s;
        }

        if (inRange && Input.GetKeyDown(KeyCode.E) && !AnyUIOpen())
        {
            if (!_opened) OpenChest();
            else          ShowUI();
        }
    }

    private bool AnyUIOpen() =>
        (ShopUI.Instance  != null && ShopUI.Instance.IsOpen) ||
        (ChestUI.Instance != null && ChestUI.Instance.IsOpen);

    private void OpenChest()
    {
        _opened      = true;
        _col.enabled = false;
        ShowPrompt(false);

        // Stop animator, set open sprite
        if (_anim != null) _anim.enabled = false;
        if (_sr   != null && openSprite != null) _sr.sprite = openSprite;

        // Select item
        if (possibleItems == null || possibleItems.Length == 0)
        {
            Debug.LogError("[Chest] possibleItems is empty — assign ItemData in Inspector.");
            return;
        }
        _selectedItem = possibleItems[Random.Range(0, possibleItems.Length)];

        ShowUI();
    }

    private void ShowUI()
    {
        if (_selectedItem == null) return;

        if (ChestUI.Instance == null)
        {
            Debug.LogError("[Chest] ChestUI not found in scene.");
            return;
        }

        ChestUI.Instance.Open(_selectedItem, OnTransactionComplete);
    }

    private void OnTransactionComplete()
    {
        _transactionDone = true;
        ShowPrompt(false);
    }
}
