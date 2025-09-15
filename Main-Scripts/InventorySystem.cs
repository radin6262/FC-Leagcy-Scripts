// Scripts/Inventory/InventorySystem.cs
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;
    public List<string> items = new List<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(string itemName)
    {
        items.Add(itemName);
        Debug.Log("Picked up: " + itemName);
    }

    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }
}
