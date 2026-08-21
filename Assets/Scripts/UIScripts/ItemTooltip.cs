using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI sellPriceText;

    [Header("Sell")]
    [SerializeField] private Button sellButton;
    [SerializeField] [Range(0f, 1f)] private float sellRatio = 0.5f;

    private ItemData      currentItem;
    private RectTransform panelRect;
    private Canvas        rootCanvas;
    private RectTransform rootCanvasRect;
    private bool isHidePending  = false;
    private bool mouseOverSlot  = false;
    private WaitForSecondsRealtime hideWait;
    private float showTime = -1f;

    public void SetMouseOverSlot(bool value) => mouseOverSlot = value;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null)
        {
            panel.SetActive(false);
            panelRect = panel.GetComponent<RectTransform>();
        }

        rootCanvas     = GetComponentInParent<Canvas>();
        rootCanvasRect = rootCanvas != null ? rootCanvas.GetComponent<RectTransform>() : null;
        hideWait       = new WaitForSecondsRealtime(0.15f);

        if (sellButton != null) sellButton.onClick.AddListener(SellItem);
    }

    private void OnDestroy()
    {
        if (sellButton != null) sellButton.onClick.RemoveListener(SellItem);
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf || panelRect == null) return;

        bool overPanel = RectTransformUtility.RectangleContainsScreenPoint(
            panelRect, Input.mousePosition, rootCanvas.worldCamera);

        // Grace period after Show — don't hide immediately
        if (Time.unscaledTime - showTime < 0.3f) return;

        if (overPanel)
        {
            if (isHidePending)
            {
                isHidePending = false;
                StopAllCoroutines();
            }
        }
        else if (!isHidePending && !mouseOverSlot)
        {
            isHidePending = true;
            StopAllCoroutines();
            StartCoroutine(HideDelayed());
        }
    }

    public void Show(ItemData item, ItemSlotUI slot)
    {
        currentItem   = item;
        isHidePending = false;
        showTime      = Time.unscaledTime;
        StopAllCoroutines();

        int sellPrice = Mathf.FloorToInt(item.price * sellRatio);

        if (nameText      != null) nameText.text      = item.itemName;
        if (descText      != null) descText.text      = item.description;
        if (sellPriceText != null) sellPriceText.text = string.Format(Loc.Get("tooltip_sell"), sellPrice);

        if (panel != null) panel.SetActive(true);

        if (panelRect != null && rootCanvas != null && rootCanvasRect != null)
        {
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvasRect,
                RectTransformUtility.WorldToScreenPoint(rootCanvas.worldCamera, slotRect.position),
                rootCanvas.worldCamera,
                out Vector2 slotPos
            );
            panelRect.anchoredPosition = slotPos + new Vector2(0f, 120f);
        }
    }

    public void Hide()
    {
        if (panel == null || !panel.activeSelf) return;
        if (isHidePending) return;
        isHidePending = true;
        StopAllCoroutines();
        StartCoroutine(HideDelayed());
    }

    private IEnumerator HideDelayed()
    {
        yield return hideWait;
        currentItem   = null;
        isHidePending = false;
        if (panel != null) panel.SetActive(false);
    }

    private void SellItem()
    {
        if (currentItem == null || Inventory.Instance == null || CoinManager.Instance == null) return;

        int sellPrice = Mathf.FloorToInt(currentItem.price * sellRatio);
        Inventory.Instance.RemoveItem(currentItem);
        CoinManager.Instance.AddCoins(sellPrice);
        currentItem   = null;
        isHidePending = false;
        StopAllCoroutines();
        if (panel != null) panel.SetActive(false);
    }
}
