// Scripts/Inventory/InventoryUI.cs
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject panel;
    public Transform itemListParent;
    public GameObject itemTextPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            panel.SetActive(!panel.activeSelf);
            Refresh();
        }
    }

    public void Refresh()
    {
        foreach (Transform child in itemListParent)
            Destroy(child.gameObject);

        foreach (string item in InventorySystem.Instance.items)
        {
            GameObject itemText = Instantiate(itemTextPrefab, itemListParent);
            itemText.GetComponent<Text>().text = "- " + item;
        }
    }
}
