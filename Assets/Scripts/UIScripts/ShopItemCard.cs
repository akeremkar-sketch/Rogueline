using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemCard : MonoBehaviour
{
    [SerializeField] private Image           iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button          buyButton;

    private ItemData   item;
    private ShopUI     shop;
    private AudioClip  _clickClip;
    private AudioClip  _errorClip;

    public void Setup(ItemData data, ShopUI shopUI)
    {
        item = data;
        shop = shopUI;

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        if (nameText  != null) nameText.text  = data.itemName;
        if (descText  != null) descText.text  = data.description;
        if (priceText != null) priceText.text = string.Format(Loc.Get("shop_price"), data.price);

        if (buyButton != null)
        {
            // Get clips from button, but play sound via ShopUI (continues even if card is destroyed)
            var anim = buyButton.GetComponent<ButtonAnimation>();
            if (anim != null)
            {
                _clickClip = anim.ClickSoundClip;
                _errorClip = anim.ErrorSoundClip;
                anim.SetAutoPlayClick(false); // OnPointerDown won't play sound — we handle it
            }

            buyButton.onClick.AddListener(OnBuyClicked);
        }
    }

    private void OnBuyClicked()
    {
        bool success = shop.TryBuy(item);
        // Sound plays from ShopUI — won't be interrupted if card is destroyed
        shop.PlayUISound(success ? _clickClip : _errorClip);
    }

    private void OnDestroy()
    {
        if (buyButton != null)
            buyButton.onClick.RemoveAllListeners();
    }
}
