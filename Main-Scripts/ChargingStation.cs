using UnityEngine;

public class ChargingStation : Interactable
{
    public BatteryFlashlight playerFlashlight;
    public float chargeAmount = 100f;

    public override void Interact()
    {
        base.Interact();

        if (playerFlashlight != null)
        {
            playerFlashlight.batteryLife = chargeAmount;
            Debug.Log("Flashlight charged to " + chargeAmount + "%");
        }
        else
        {
            Debug.LogWarning("No flashlight found to charge.");
        }
    }
}
