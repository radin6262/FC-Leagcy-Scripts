using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public float[] playerPosition;   // [x, y, z]
    public List<string> inventoryItems;
}

public class AutoSaveSystem : MonoBehaviour
{
    [Header("References")]
    public Transform player;                
    public InventorySystem inventorySystem; 
    public Text autosaveText;               // Assign a UI Text element in Canvas
    public CharacterController controller; // assign your player’s controller

    [Header("Settings")]
    public float autoSaveInterval = 30f;    // seconds between autosaves
    public float autosaveMessageTime = 2f;  // how long message stays visible

    private string savePath;
    private float messageTimer = 0f;

    private void Start()
    {
        savePath = Application.persistentDataPath + "/save.json";
        InvokeRepeating(nameof(SaveGame), autoSaveInterval, autoSaveInterval);

        if (autosaveText != null)
            autosaveText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (autosaveText != null && autosaveText.gameObject.activeSelf)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
                autosaveText.gameObject.SetActive(false);
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // Save player position
        data.playerPosition = new float[3];
        data.playerPosition[0] = player.position.x;
        data.playerPosition[1] = player.position.y;
        data.playerPosition[2] = player.position.z;

        // Save inventory
        data.inventoryItems = new List<string>(inventorySystem.items);

        // Convert to JSON
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("Game autosaved! Path: " + savePath);

        ShowAutosaveMessage("Autosaving...");
    }
    

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No save file found!");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Vector3 targetPos = new Vector3(
            data.playerPosition[0],
            data.playerPosition[1],
            data.playerPosition[2]
        );

        // Move safely using Move() which respects collisions
        if (controller != null)
        {
            Vector3 offset = targetPos - controller.transform.position;
            controller.enabled = true; // keep enabled
            controller.Move(offset);
        }
        else
        {
            player.position = targetPos; // fallback
        }

        // Load inventory
        inventorySystem.items.Clear();
        foreach (string item in data.inventoryItems)
            inventorySystem.AddItem(item);

        Debug.Log("Game loaded safely!");
    }


    private void ShowAutosaveMessage(string message)
    {
        if (autosaveText != null)
        {
            autosaveText.text = message;
            autosaveText.gameObject.SetActive(true);
            messageTimer = autosaveMessageTime;
        }
    }
}
