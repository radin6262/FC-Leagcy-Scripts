// Scripts/Inventory/PickupItem.cs
using UnityEngine;

public class PickupItem : Interactable
{
    public string itemName;

    public override void Interact()
    {
        InventorySystem.Instance.AddItem(itemName);
        Destroy(gameObject);
    }
}
