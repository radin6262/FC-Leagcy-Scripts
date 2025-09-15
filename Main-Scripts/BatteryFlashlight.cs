// Scripts/Systems/BatteryFlashlight.cs
using UnityEngine;
using UnityEngine.UI;

public class BatteryFlashlight : MonoBehaviour
{
    public Light flashlight;
    public float batteryLife = 100f;
    public float drainRate = 0.20f;
    public Text batteryText;

    public bool isOn = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && batteryLife > 0)
        {
            isOn = !isOn;
            flashlight.enabled = isOn;
        }

        if (isOn)
        {
            batteryLife -= drainRate * Time.deltaTime;
            if (batteryLife <= 0)
            {
                batteryLife = 0;
                flashlight.enabled = false;
                isOn = false;
            }
        }

        batteryText.text = Mathf.RoundToInt(batteryLife) + "%";
    }

    public void Recharge(float amount)
    {
        batteryLife = Mathf.Clamp(batteryLife + amount, 0, 100);
    }
}
