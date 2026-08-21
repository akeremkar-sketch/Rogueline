using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image      iconImage;
    [SerializeField] private GameObject emptyOverlay;

    private ItemData currentItem;

    public void SetItem(ItemData item)
    {
        currentItem = item;
        bool hasItem = item != null;

        if (iconImage != null)
        {
            iconImage.sprite  = hasItem ? item.icon : null;
            iconImage.enabled = hasItem && item.icon != null;
        }

        if (emptyOverlay != null)
            emptyOverlay.SetActive(!hasItem);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem == null) return;
        ItemTooltip.Instance?.SetMouseOverSlot(true);
        ItemTooltip.Instance?.Show(currentItem, this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Instance?.SetMouseOverSlot(false);
        ItemTooltip.Instance?.Hide();
    }
}
