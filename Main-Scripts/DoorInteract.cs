// Scripts/Interactables/DoorInteract.cs
using UnityEngine;

public class DoorInteract : Interactable
{
    private bool isOpen = false;
    public Transform doorPivot;
    public float openAngle = 90f;
    public float openSpeed = 0.03f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = doorPivot.localRotation;
        openRotation = Quaternion.Euler(0f, openAngle, 0f) * closedRotation;
    }

    public override void Interact()
    {
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(OpenDoor(isOpen));
    }

    System.Collections.IEnumerator OpenDoor(bool open)
    {
        Quaternion targetRotation = open ? openRotation : closedRotation;
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;
            doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, targetRotation, t);
            yield return null;
        }
    }
}
