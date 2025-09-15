using UnityEngine;
using System.Collections.Generic;

public class MenuManager3D : MonoBehaviour
{
    public List<GameObject> menus = new List<GameObject>();
    public int currentMenuIndex = 0;
    
    void Start()
    {
        ShowMenu(currentMenuIndex);
    }
    
    public void ShowMenu(int index)
    {
        if (index < 0 || index >= menus.Count) return;
        
        // Hide all menus
        foreach (var menu in menus)
        {
            menu.SetActive(false);
        }
        
        // Show selected menu
        menus[index].SetActive(true);
        currentMenuIndex = index;
    }
    
    public void NextMenu()
    {
        ShowMenu((currentMenuIndex + 1) % menus.Count);
    }
    
    public void PreviousMenu()
    {
        ShowMenu((currentMenuIndex - 1 + menus.Count) % menus.Count);
    }
}