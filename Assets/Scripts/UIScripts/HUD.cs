using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
    [Header("Health Bar")]
    public Image           hpBarFill;
    public TextMeshProUGUI healthText;

    [Header("XP / Level Bar")]
    public Image           xpBarFill;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;

    [Header("Kill Count")]
    public TextMeshProUGUI killText;

    [Header("Other")]
    public TextMeshProUGUI coinText;

    [Header("Distance")]
    public TextMeshProUGUI distanceText;

    [Header("Character Avatar")]
    public Image           characterAvatarImage;

    [Header("Weapon UI")]
    public Image           weaponIconImage;
    public TextMeshProUGUI weaponNameText;

    [Header("Item Slots")]
    public ItemSlotUI[] itemSlots;

    // Cached previous values — update UI only when something actually changed
    private float      _lastHp, _lastMaxHp;
    private float      _lastXp, _lastXpRequired;
    private int        _lastLevel;
    private bool       _lastIsMaxLevel;
    private int        _lastCoins;
    private int        _lastKills;
    private int        _lastDistance;
    private WeaponBase _lastWeapon;

    private void Start()
    {
        if (xpBarFill != null)
            xpBarFill.fillAmount = 0f;

        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged += RefreshItemSlots;

        LocalizationManager.OnLanguageChanged += ForceRefreshAll;

        SetCharacterAvatar();
        RefreshItemSlots();
        ForceRefreshAll();
    }

    private void SetCharacterAvatar()
    {
        if (characterAvatarImage == null) return;
        CharacterData data = CharacterManager.Instance?.ActiveCharacter;
        if (data == null || data.avatar == null) return;
        characterAvatarImage.sprite = data.avatar;
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged -= RefreshItemSlots;
        LocalizationManager.OnLanguageChanged -= ForceRefreshAll;
    }

    private void RefreshItemSlots()
    {
        if (itemSlots == null) return;
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null) continue;
            ItemData item = (Inventory.Instance != null && i < Inventory.Instance.Items.Count)
                ? Inventory.Instance.Items[i]
                : null;
            itemSlots[i].SetItem(item);
        }
    }

    private void Update()
    {
        UpdateHealth();
        UpdateXP();
        UpdateCoins();
        UpdateKills();
        UpdateDistance();
        UpdateWeapon();
    }

    private void UpdateHealth()
    {
        if (PlayerHealth.Instance == null) return;
        float hp    = PlayerHealth.Instance.GetCurrentHealth();
        float maxHp = PlayerHealth.Instance.GetMaxHealth();
        if (hp == _lastHp && maxHp == _lastMaxHp) return;

        _lastHp    = hp;
        _lastMaxHp = maxHp;

        if (hpBarFill != null)
            hpBarFill.fillAmount = maxHp > 0f ? Mathf.Clamp01(hp / maxHp) : 0f;
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(hp)} / {Mathf.RoundToInt(maxHp)}";
    }

    private void UpdateXP()
    {
        if (LevelSystem.Instance == null) return;
        float xp        = LevelSystem.Instance.CurrentXP;
        float xpNeeded  = LevelSystem.Instance.XPRequired;
        int   level     = LevelSystem.Instance.CurrentLevel;
        bool  isMax     = LevelSystem.Instance.IsMaxLevel;

        if (xp == _lastXp && xpNeeded == _lastXpRequired && level == _lastLevel && isMax == _lastIsMaxLevel)
            return;

        _lastXp          = xp;
        _lastXpRequired  = xpNeeded;
        _lastLevel       = level;
        _lastIsMaxLevel  = isMax;

        if (xpBarFill != null)
            xpBarFill.fillAmount = isMax ? 1f : (xpNeeded > 0f ? Mathf.Clamp01(xp / xpNeeded) : 0f);
        if (levelText != null)
            levelText.text = string.Format(Loc.Get("hud_level"), level);
        if (xpText != null)
            xpText.text = isMax ? Loc.Get("hud_xp_max") : $"{xp} / {xpNeeded}";
    }

    private void UpdateCoins()
    {
        if (coinText == null || CoinManager.Instance == null) return;
        int coins = CoinManager.Instance.GetCoins();
        if (coins == _lastCoins) return;
        _lastCoins      = coins;
        coinText.text   = coins.ToString();
    }

    private void UpdateKills()
    {
        if (killText == null || GameManager.Instance == null) return;
        int kills = GameManager.Instance.KillCount;
        if (kills == _lastKills) return;
        _lastKills     = kills;
        killText.text  = kills.ToString();
    }

    private void UpdateDistance()
    {
        if (distanceText == null || GameManager.Instance == null) return;
        int dist = Mathf.FloorToInt(GameManager.Instance.Distance);
        if (dist == _lastDistance) return;
        _lastDistance       = dist;
        distanceText.text   = $"{dist}m";
    }

    private void UpdateWeapon()
    {
        if (WeaponManager.Instance == null) return;
        WeaponBase w = WeaponManager.Instance.ActiveWeapon;
        if (w == _lastWeapon) return;
        _lastWeapon = w;

        if (w == null) return;
        if (weaponIconImage != null) weaponIconImage.sprite = w.weaponIcon;
        if (weaponNameText  != null) weaponNameText.text    = w.weaponDisplayName;
    }

    // Called once at Start to populate all fields immediately
    private void ForceRefreshAll()
    {
        _lastHp = _lastMaxHp = -1f;
        _lastXp = _lastXpRequired = -1f;
        _lastLevel = -1;
        _lastCoins = _lastKills = _lastDistance = -1;
        _lastWeapon = null;
        Update();
    }
}
