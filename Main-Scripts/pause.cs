using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Canvas to enable")]
    public Canvas pauseCanvas;  // Assign your Canvas here

    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape; // Default is ESC, but you can change it in Inspector

    private bool isPaused = false;

    private void Awake()
    {
        if (pauseCanvas != null)
            pauseCanvas.enabled = false; // Ensure it's disabled at start

        // Hide and lock cursor at start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseCanvas != null)
            pauseCanvas.enabled = isPaused;

        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            EnableCursor();
        }
        else
        {
            DisableCursor();
        }
    }

    private void EnableCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}