// Scripts/Interactables/LockedDoor.cs
using UnityEngine;

public class LockedDoor : Interactable
{
    public string requiredItem = "Key";
    public Transform doorPivot;
    public float openAngle = 90f;
    public float openSpeed = 0.03f;
    private bool isOpen = false;

    private Quaternion closedRot, openRot;

    void Start()
    {
        closedRot = doorPivot.localRotation;
        openRot = Quaternion.Euler(0f, openAngle, 0f) * closedRot;
    }

    public override void Interact()
    {
        if (!InventorySystem.Instance.HasItem(requiredItem))
        {
            Debug.Log("You need: " + requiredItem);
            return;
        }

        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(OpenDoor(isOpen));
    }

    System.Collections.IEnumerator OpenDoor(bool open)
    {
        Quaternion targetRot = open ? openRot : closedRot;
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;
            doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, targetRot, t);
            yield return null;
        }
    }
}
