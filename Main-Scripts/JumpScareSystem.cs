using UnityEngine;
using UnityEngine.SceneManagement; // required for scene loading

public class JumpScareSystem : MonoBehaviour
{
    [Header("Jumpscare Settings")]
    public GameObject scareObject;
    public AudioClip scareClip;
    public AnimationClip scareAnimation;
    public float scareDuration = 2f;

    [Header("Player Settings")]
    public MonoBehaviour movementScript; // movement script to disable
    public Transform playerCamera;       // player camera transform

    [Header("Scene Transfer")]
    public string nextSceneName; // name of the scene to load after jumpscare

    private AudioSource scareSound;
    private Animation scareAnim;
    private bool isScaring = false;

    void Start()
    {
        if (scareObject != null)
            scareObject.SetActive(false);

        // setup audio
        scareSound = gameObject.AddComponent<AudioSource>();
        scareSound.clip = scareClip;
        scareSound.playOnAwake = false;

        // setup animation
        if (scareObject != null && scareAnimation != null)
        {
            scareAnim = scareObject.GetComponent<Animation>();
            if (scareAnim == null)
                scareAnim = scareObject.AddComponent<Animation>();
            scareAnim.clip = scareAnimation;
        }
    }

    public void TriggerScare()
    {
        if (isScaring) return;
        StartCoroutine(DoScare());
    }

    private System.Collections.IEnumerator DoScare()
    {
        isScaring = true;

        // freeze player
        if (movementScript != null)
            movementScript.enabled = false;

        // rotate camera to face scare object
        if (playerCamera != null && scareObject != null)
        {
            Vector3 direction = scareObject.transform.position - playerCamera.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            playerCamera.rotation = targetRotation;
        }

        // show scare object
        if (scareObject != null)
            scareObject.SetActive(true);

        // play sound
        if (scareSound != null)
            scareSound.Play();

        // play animation
        if (scareAnim != null && scareAnimation != null)
            scareAnim.Play();

        // wait for scare duration
        yield return new WaitForSeconds(scareDuration);

        // hide scare
        if (scareObject != null)
            scareObject.SetActive(false);

        // unfreeze player
        if (movementScript != null)
            movementScript.enabled = true;

        isScaring = false;

        // transfer to next scene if assigned
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}

