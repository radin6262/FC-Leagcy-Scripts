using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SecurityCamSystem : MonoBehaviour
{
    public GameObject monitorUI;                // UI Canvas for camera feed
    public RawImage camDisplay;                 // RawImage showing the RenderTexture
    public RenderTexture[] camFeeds;            // List of camera feeds
    public Text camLabel;                       // UI label for camera number
    public float interactDistance = 2.5f;       // Interaction distance
    public LayerMask monitorLayer;              // Layer for monitors
    public Text promptText;                     // UI prompt text
    public bool enablePanning = true;           // Toggle camera panning
    public float panSpeed = 20f;                // Speed of panning

    public Material nightVisionGlitchMaterial; // Combined night vision + glitch material
    public Material defaultMaterial;            // Default UI material

    private float interactCooldown = 0f;
    private int currentCam = 0;
    private bool isViewing = false;
    private bool nightVisionOn = false;

    private SimpleFPSPlayer playerController;
    private Camera mainCam;
    private Camera[] cameraObjects;

    void Start()
    {
        playerController = Object.FindFirstObjectByType<SimpleFPSPlayer>();
        mainCam = Camera.main;

        cameraObjects = new Camera[camFeeds.Length];
        for (int i = 0; i < camFeeds.Length; i++)
        {
            // IMPORTANT: This assumes the RenderTexture has a camera component on the same GameObject
            // Usually, RenderTexture is assigned to a camera's targetTexture, so this step may need adjustment
            // Instead, find cameras tagged or referenced properly if needed
            cameraObjects[i] = null; // Assign your cameras here properly if you have references
        }

        if (monitorUI != null)
            monitorUI.SetActive(false);

        // Ensure RawImage starts with default material
        if(camDisplay != null)
            camDisplay.material = defaultMaterial;
    }

    void Update()
    {
        interactCooldown -= Time.deltaTime;

        if (!isViewing && interactCooldown <= 0f)
        {
            CheckForMonitorInteraction();
        }

        if (isViewing && interactCooldown <= 0f)
        {
            HandleCamControls();
        }

        if (isViewing && enablePanning)
        {
            PanCurrentCamera();
        }
    }

    void CheckForMonitorInteraction()
    {
        if (mainCam == null || promptText == null)
            return;

        promptText.text = ""; // Clear prompt by default

        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, monitorLayer))
        {
            promptText.text = "Press [E] to view cameras\n[←] / [→] to switch | N for night vision/glitch";

            if (Input.GetKeyDown(KeyCode.E))
            {
                EnterCamView();
            }
        }
    }

    void HandleCamControls()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ExitCamView();
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchCam(1);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchCam(-1);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleNightVisionGlitch();
        }
    }

    void EnterCamView()
    {
        isViewing = true;
        ShowCam(currentCam);

        if (monitorUI != null)
            monitorUI.SetActive(true);

        if (playerController != null)
            playerController.enabled = false;

        StartCoroutine(UnlockCursorNextFrame());

        interactCooldown = 0.5f;

        Canvas.ForceUpdateCanvases();
    }

    void ExitCamView()
    {
        isViewing = false;

        if (monitorUI != null)
            monitorUI.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (promptText != null)
            promptText.text = "";

        camDisplay.material = defaultMaterial;
        nightVisionOn = false;

        interactCooldown = 0.5f;
    }

    void SwitchCam(int dir)
    {
        if (camFeeds.Length == 0) return;

        currentCam = (currentCam + dir + camFeeds.Length) % camFeeds.Length;
        ShowCam(currentCam);
    }

    void ShowCam(int index)
    {
        if (index >= 0 && index < camFeeds.Length && camFeeds[index] != null)
        {
            camDisplay.texture = camFeeds[index];
            camLabel.text = "CAM " + (index + 1);

            // Enable only current camera if you have references, else ignore
            for (int i = 0; i < cameraObjects.Length; i++)
            {
                if (cameraObjects[i] != null)
                    cameraObjects[i].enabled = (i == index);
            }
        }
        else
        {
            Debug.LogWarning("Invalid camera index or texture is null.");
        }
    }

    void ToggleNightVisionGlitch()
    {
        nightVisionOn = !nightVisionOn;

        if (nightVisionOn)
        {
            camDisplay.material = nightVisionGlitchMaterial;
        }
        else
        {
            camDisplay.material = defaultMaterial;
        }
    }

    void PanCurrentCamera()
    {
        // If you have actual camera objects controlling the feeds, pan them.
        if (cameraObjects.Length == 0) return;

        Camera cam = cameraObjects[currentCam];
        if (cam == null) return;

        float angle = Mathf.Sin(Time.time * 0.5f) * panSpeed;
        cam.transform.localRotation = Quaternion.Euler(0, angle, 0);
    }

    IEnumerator UnlockCursorNextFrame()
    {
        yield return null;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
