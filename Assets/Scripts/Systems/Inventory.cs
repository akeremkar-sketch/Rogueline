using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [SerializeField] private int maxSlots = 6;

    public List<ItemData> Items    { get; private set; } = new List<ItemData>();
    public int            MaxSlots => maxSlots;
    public bool           IsFull   => Items.Count >= maxSlots;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Returns true if added successfully
    public bool AddItem(ItemData item)
    {
        if (IsFull) return false;

        Items.Add(item);
        item.Apply();
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemData item)
    {
        if (!Items.Contains(item)) return false;
        item.Remove();
        Items.Remove(item);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        foreach (var item in Items)
            item.Remove();
        Items.Clear();
        OnInventoryChanged?.Invoke();
    }
}
