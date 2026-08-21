using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [Header("Stock")]
    [SerializeField] private List<ItemData> possibleItems = new List<ItemData>();
    [SerializeField] private int            stockCount    = 3;

    private bool playerInZone  = false;
    private bool _promptShown = false;
    private List<ItemData> stock;

    private void Start()
    {
        stock = BuildStock();
    }

    private void Update()
    {
        if (!playerInZone) return;

        bool shopOpen   = ShopUI.Instance != null && ShopUI.Instance.IsOpen;
        bool showPrompt = !shopOpen;

        // Only update SetActive when state actually changes
        if (showPrompt != _promptShown)
        {
            _promptShown = showPrompt;
            ShopUI.Instance?.ShowPrompt(showPrompt);
        }

        // Only open when no other UI is active
        bool anyUIOpen = shopOpen
            || (UpgradeUI.Instance  != null && UpgradeUI.Instance.IsShowing)
            || (StatsPanel.Instance != null && StatsPanel.Instance.IsOpen);

        if (!anyUIOpen && Input.GetKeyDown(KeyCode.E))
            OpenShop();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone  = false;
        _promptShown  = false;
        ShopUI.Instance?.ShowPrompt(false);
    }

    private void OpenShop()
    {
        if (ShopUI.Instance == null) return;
        ShopUI.Instance.Open(stock);
    }

    private List<ItemData> BuildStock()
    {
        List<ItemData> pool   = new List<ItemData>(possibleItems);
        List<ItemData> result = new List<ItemData>();

        for (int i = 0; i < stockCount && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        return result;
    }
}
