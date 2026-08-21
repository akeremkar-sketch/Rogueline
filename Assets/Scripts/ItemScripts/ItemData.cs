using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemStatModifier
{
    public StatType stat;
    public float    amount;
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Rogueline/Item")]
public class ItemData : ScriptableObject
{
    [Header("Display")]
    public string itemName        = "Item";
    [TextArea] public string description = "";
    public Sprite icon;

    [Header("Shop")]
    public int price = 10;

    [Header("Stat Effects")]
    public List<ItemStatModifier> modifiers = new List<ItemStatModifier>();

    public void Apply()
    {
        foreach (var m in modifiers)
            PlayerStats.Instance.ModifyStat(m.stat, m.amount);
    }

    public void Remove()
    {
        foreach (var m in modifiers)
            PlayerStats.Instance.ModifyStat(m.stat, -m.amount);
    }
}
