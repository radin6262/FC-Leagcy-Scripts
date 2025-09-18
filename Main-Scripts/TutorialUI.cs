using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel;  // Your UI Panel prefab
    public Text tutorialText;         // For displaying text
    public Image tutorialImage;       // Optional image slot

    public void ShowTutorial(string message, Sprite image = null)
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        if (tutorialText != null)
            tutorialText.text = message;

        if (tutorialImage != null)
        {
            if (image != null)
            {
                tutorialImage.sprite = image;
                tutorialImage.enabled = true;
            }
            else
            {
                tutorialImage.enabled = false;
            }
        }
    }

    public void HideTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }
}
