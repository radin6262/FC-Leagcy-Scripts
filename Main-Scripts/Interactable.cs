// Scripts/Interactables/Interactable.cs
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public string promptMessage = "Press E to interact";
    public virtual void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }
}
