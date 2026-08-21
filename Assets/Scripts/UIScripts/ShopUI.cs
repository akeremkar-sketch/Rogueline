using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject   shopPanel;

    [Header("Item Card Template")]
    [SerializeField] private GameObject   itemCardPrefab;
    [SerializeField] private Transform    itemCardContainer;

    [Header("Close Button")]
    [SerializeField] private Button       closeButton;

    [Header("Coin Display")]
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Messages")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Interact Prompt")]
    [SerializeField] private GameObject interactPrompt;

    public bool IsOpen { get; private set; } = false;
    private List<ItemData> currentStock = new List<ItemData>();

    // ShopUI is not destroyed, so sound coroutines complete safely
    private AudioSource _uiAudio;
    private Coroutine   _soundCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        _uiAudio              = gameObject.AddComponent<AudioSource>();
        _uiAudio.playOnAwake  = false;
        _uiAudio.spatialBlend = 0f;
        _uiAudio.volume       = 1f;
    }

    private void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
    }

    // ── Ses ─────────────────────────────────────────────────────────────────────

    public void PlayUISound(AudioClip clip)
    {
        if (clip == null) return;
        if (_soundCoroutine != null) StopCoroutine(_soundCoroutine);
        _soundCoroutine = StartCoroutine(PlayClippedRoutine(clip));
    }

    private IEnumerator PlayClippedRoutine(AudioClip clip)
    {
        const float duration = 0.25f;
        const float fadeOut  = 0.08f;
        float startVol = AudioManager.Instance != null ? AudioManager.Instance.EffectiveUI : 1f;

        _uiAudio.volume = startVol;
        _uiAudio.clip   = clip;
        _uiAudio.Play();

        float playTime = Mathf.Max(0f, duration - fadeOut);
        if (playTime > 0f)
            yield return new WaitForSecondsRealtime(playTime);

        float elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed         += Time.unscaledDeltaTime;
            _uiAudio.volume  = Mathf.Lerp(startVol, 0f, elapsed / fadeOut);
            yield return null;
        }

        _uiAudio.Stop();
        _uiAudio.volume = 1f;
        _soundCoroutine = null;
    }

    // ── Panel ────────────────────────────────────────────────────────────────────

    public void ShowPrompt(bool show)
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(show);
    }

    public void Open(List<ItemData> stock)
    {
        currentStock = stock;
        IsOpen = true;
        Time.timeScale = 0f;
        shopPanel?.SetActive(true);
        BuildCards();
        if (messageText != null) messageText.text = "";
        UpdateCoinText();
    }

    public void Close()
    {
        IsOpen = false;
        Time.timeScale = 1f;
        shopPanel?.SetActive(false);
    }

    private void BuildCards()
    {
        foreach (Transform child in itemCardContainer)
        {
            foreach (var tmp in child.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                tmp.raycastTarget = false;
            Destroy(child.gameObject);
        }

        foreach (var item in currentStock)
        {
            if (itemCardPrefab == null) continue;
            GameObject card = Instantiate(itemCardPrefab, itemCardContainer);
            card.GetComponent<ShopItemCard>()?.Setup(item, this);
        }
    }

    // ── Purchase ─────────────────────────────────────────────────────────────────

    public bool TryBuy(ItemData item)
    {
        if (CoinManager.Instance == null || Inventory.Instance == null) return false;

        if (Inventory.Instance.IsFull)
        {
            ShowMessage(Loc.Get("shop_inventory_full"));
            return false;
        }

        if (!CoinManager.Instance.SpendCoins(item.price))
        {
            ShowMessage(Loc.Get("shop_not_enough_coins"));
            return false;
        }

        Inventory.Instance.AddItem(item);
        currentStock.Remove(item);
        BuildCards();
        UpdateCoinText();
        ShowMessage(string.Format(Loc.Get("shop_bought"), item.itemName));
        return true;
    }

    private void UpdateCoinText()
    {
        if (coinText != null && CoinManager.Instance != null)
            coinText.text = CoinManager.Instance.GetCoins().ToString();
    }

    private void ShowMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
    }
}
