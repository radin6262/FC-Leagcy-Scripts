using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider), typeof(MeshRenderer))]
public class raycast : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Settings")]
    public Material normalMaterial;
    public Material hoverMaterial;
    public Material clickMaterial;
    public float hoverScaleFactor = 1.1f;
    
    [Header("Text Settings")]
    public TMP_Text buttonText;
    public Color normalTextColor = Color.white;
    public Color hoverTextColor = Color.yellow;
    
    [Header("Animation Settings")]
    public float animationSpeed = 5f;
    public float clickScaleFactor = 0.9f;
    
    [Header("Audio Settings")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    [Range(0,1)] public float volume = 0.8f;
    
    [Header("Events")]
    public UnityEvent onClick;
    
    private Vector3 originalScale;
    private MeshRenderer meshRenderer;
    private AudioSource audioSource;
    private bool isAnimating = false;
    
    void Start()
    {
        originalScale = transform.localScale;
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = normalMaterial;
        
        // Set up audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        
        if (buttonText != null)
        {
            buttonText.color = normalTextColor;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isAnimating) return;
        
        meshRenderer.material = hoverMaterial;
        transform.localScale = originalScale * hoverScaleFactor;
        
        if (buttonText != null)
        {
            buttonText.color = hoverTextColor;
        }
        
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isAnimating) return;
        
        meshRenderer.material = normalMaterial;
        transform.localScale = originalScale;
        
        if (buttonText != null)
        {
            buttonText.color = normalTextColor;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateButton());
            ButtonClicked();
        }
    }
    
    IEnumerator AnimateButton()
    {
        isAnimating = true;
        
        // Play click sound
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        
        // Click animation (scale down)
        meshRenderer.material = clickMaterial;
        Vector3 clickScale = originalScale * clickScaleFactor;
        
        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * animationSpeed;
            transform.localScale = Vector3.Lerp(originalScale * hoverScaleFactor, clickScale, progress);
            yield return null;
        }
        
        // Return to hover state
        progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * animationSpeed;
            transform.localScale = Vector3.Lerp(clickScale, originalScale * hoverScaleFactor, progress);
            yield return null;
        }
        
        meshRenderer.material = hoverMaterial;
        isAnimating = false;
    }
    
    private void ButtonClicked()
    {
        Debug.Log("Button clicked: " + gameObject.name);
        onClick.Invoke(); // Trigger UnityEvent
        
        // Add your custom button functionality here
        SceneManager.LoadScene("night1");
        // Example: SceneManager.LoadScene("GameScene");
        
    }
    
    // For traditional mouse input (works alongside EventSystem)
    void OnMouseEnter() => OnPointerEnter(null);
    void OnMouseExit() => OnPointerExit(null);
    void OnMouseUpAsButton() => OnPointerClick(null);

    
}