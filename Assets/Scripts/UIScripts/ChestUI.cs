using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestUI : MonoBehaviour
{
    public static ChestUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Item Bilgileri")]
    [SerializeField] private Image           itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescText;
    [SerializeField] private TextMeshProUGUI sellPriceText;

    [Header("Butonlar")]
    [SerializeField] private Button          takeButton;
    [SerializeField] private TextMeshProUGUI takeButtonLabel;
    [SerializeField] private Button          sellButton;

    public bool IsOpen { get; private set; }

    private ItemData _item;
    private Action   _onTransactionComplete; // item taken or sold

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        panel?.SetActive(false);
        takeButton?.onClick.AddListener(OnTake);
        sellButton?.onClick.AddListener(OnSell);
    }

    private void OnDestroy()
    {
        if (takeButton != null) takeButton.onClick.RemoveListener(OnTake);
        if (sellButton != null) sellButton.onClick.RemoveListener(OnSell);
    }

    private void Update()
    {
        // ESC: close UI only, transaction not complete — chest can be reopened
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseOnly();
    }

    public void Open(ItemData item, Action onTransactionComplete = null)
    {
        _item                  = item;
        _onTransactionComplete = onTransactionComplete;
        IsOpen                 = true;
        Time.timeScale         = 0f;
        panel?.SetActive(true);
        Refresh();
    }

    private void Refresh()
    {
        if (_item == null) return;

        if (itemIcon      != null) itemIcon.sprite      = _item.icon;
        if (itemNameText  != null) itemNameText.text    = _item.itemName;
        if (itemDescText  != null) itemDescText.text    = _item.description;
        if (sellPriceText != null) sellPriceText.text   = string.Format(Loc.Get("shop_sell"), _item.price);

        bool isFull = Inventory.Instance != null && Inventory.Instance.IsFull;
        if (takeButton      != null) takeButton.interactable = !isFull;
        if (takeButtonLabel != null) takeButtonLabel.text    = isFull ? Loc.Get("no_space") : Loc.Get("take");
    }

    private void OnTake()
    {
        if (_item == null || Inventory.Instance == null) return;
        if (Inventory.Instance.IsFull) return;
        Inventory.Instance.AddItem(_item);
        CompleteTransaction();
    }

    private void OnSell()
    {
        if (_item == null || CoinManager.Instance == null) return;
        CoinManager.Instance.AddCoins(_item.price);
        CompleteTransaction();
    }

    // Transaction complete (Take or Sell) — fire callback, chest cannot be reopened
    private void CompleteTransaction()
    {
        _onTransactionComplete?.Invoke();
        CloseOnly();
    }

    // Close panel only — transaction not complete (ESC)
    private void CloseOnly()
    {
        IsOpen         = false;
        Time.timeScale = 1f;
        panel?.SetActive(false);
        _item                  = null;
        _onTransactionComplete = null;
    }
}
