using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSPlayer : MonoBehaviour
{
    public float bobamount = 0.5f;
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 3f;
    public float jumpHeight = 2f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.81f;
    public float interactDistance = 3f;
    public float stamina = 0.5f;
    public float maxStamina = 0.5f;
    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 0.5f;
    public float crouchHeight = 1f;
    public float normalHeight = 2.5f;
    public Transform cameraTransform;
    public LayerMask interactLayer;
    public Text promptText;
    public Slider staminaBar;
    public CanvasGroup staminaBarCanvas;
    public Image staminaWarningImage;
    public AudioSource footstepAudioSource;
    public AudioClip[] footstepClips;
    public AudioSource breathingAudioSource;
    public AudioClip heavyBreathClip;

    private CharacterController controller;
    private float verticalVelocity;
    private float xRotation = 0f;
    private float footstepTimer = 0.25f;
    private float stepInterval = 0.5f;
    private Vector3 move;
    private bool isCrouching = false;
    private float cameraTargetY;
    private float cameraSmoothVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        cameraTargetY = cameraTransform.localPosition.y;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleInteract();
        HandleFootsteps();
        UpdateStaminaBar();
        HandleBreathingSounds();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 direction = transform.right * moveX + transform.forward * moveZ;
        float currentSpeed = walkSpeed;
        bool isMoving = direction.magnitude > 0.1f;

        // Sprint
        bool sprinting = Input.GetKey(KeyCode.LeftShift) && stamina > 0f && controller.isGrounded && !isCrouching && isMoving;

        if (sprinting)
        {
            currentSpeed = sprintSpeed;
            stamina -= staminaDrainRate * Time.deltaTime;
        }
        else if (!sprinting)
        {
            stamina += staminaRegenRate * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        // Crouch toggle
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? crouchHeight : normalHeight;
            cameraTargetY = isCrouching ? 0.5f : 0.9f;
        }

        if (isCrouching) currentSpeed = crouchSpeed;

        // Gravity & Jump
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (Input.GetButtonDown("Jump") && !isCrouching)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else verticalVelocity += gravity * Time.deltaTime;

        move = direction * currentSpeed;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        ApplyHeadbob(isMoving);
        AnimateCameraCrouch();
    }

    void HandleInteract()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactDistance, interactLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                promptText.text = interactable.promptMessage;
                if (Input.GetKeyDown(KeyCode.E))
                    interactable.Interact();
            }
        }
        else promptText.text = "";
    }

    void HandleFootsteps()
    {
        if (!controller.isGrounded || move.magnitude < 0.5f) return;

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            footstepTimer = stepInterval;
            if (footstepClips.Length > 0 && footstepAudioSource != null)
            {
                footstepAudioSource.clip = footstepClips[Random.Range(0, footstepClips.Length)];
                footstepAudioSource.Play();
            }
        }
    }

    void ApplyHeadbob(bool isMoving)
    {
        if (controller.isGrounded && isMoving)
        {
            float bobAmount = Mathf.Sin(Time.time * 10f) * bobamount;
            cameraTransform.localPosition = new Vector3(0f, cameraTransform.localPosition.y + bobAmount, 0f);
        }
    }

    void AnimateCameraCrouch()
    {
        float newY = Mathf.SmoothDamp(cameraTransform.localPosition.y, cameraTargetY, ref cameraSmoothVelocity, 0.1f);
        cameraTransform.localPosition = new Vector3(0f, newY, 0f);
    }

    void UpdateStaminaBar()
    {
        bool shouldShow = stamina < maxStamina || Input.GetKey(KeyCode.LeftShift);

        if (staminaBar != null)
        {
            staminaBar.value = stamina / maxStamina;
            staminaBarCanvas.alpha = shouldShow ? 1f : 0f;
        }

        if (staminaWarningImage != null)
        {
            if (stamina <= 0.5f)
            {
                staminaWarningImage.enabled = true;
                staminaWarningImage.color = new Color(1f, 0f, 0f, Mathf.PingPong(Time.time * 4, 1f));
            }
            else staminaWarningImage.enabled = false;
        }
    }

    void HandleBreathingSounds()
    {
        if (stamina <= 0.1f && !breathingAudioSource.isPlaying)
        {
            breathingAudioSource.clip = heavyBreathClip;
            breathingAudioSource.loop = true;
            breathingAudioSource.Play();
            if (breathingAudioSource.isPlaying)
            {
                sprintSpeed -= 8f;
            }
        }
        else if (stamina > 0.1f && breathingAudioSource.isPlaying)
        {
            breathingAudioSource.Stop();
            sprintSpeed = 19.5f;
        }
    }
}
