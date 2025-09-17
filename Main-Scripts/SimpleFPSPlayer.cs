using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 3f;
    public float jumpHeight = 2f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.81f;

    [Header("Stamina Settings")]
    public float stamina = 0.5f;
    public float maxStamina = 0.5f;
    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 0.5f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float normalHeight = 2.5f;

    [Header("Camera & UI")]
    public Transform cameraTransform;
    public LayerMask interactLayer;
    public float interactDistance = 3f;
    public Text promptText;
    public Slider staminaBar;
    public CanvasGroup staminaBarCanvas;
    public Image staminaWarningImage;

    [Header("Audio")]
    public AudioSource footstepAudioSource;
    public AudioClip[] footstepClips;
    public AudioSource breathingAudioSource;
    public AudioClip heavyBreathClip;

    [Header("Headbob Settings")]
    public float walkBobSpeed = 10f;
    public float sprintBobSpeed = 14f;
    public float crouchBobSpeed = 6f;
    public float bobAmount = 0.05f;

    [Header("Player Animations (Clips Only)")]
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip sprintClip;

    private CharacterController controller;
    private float verticalVelocity;
    private float xRotation = 0f;
    private float footstepTimer = 0.25f;
    private float stepInterval = 0.5f;
    private Vector3 move;

    private bool isCrouching = false;
    private float cameraTargetY;
    private float bobTimer;
    private float defaultCamY;

    private float baseSprintSpeed;

    private Animation anim; // legacy animation component

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        defaultCamY = cameraTransform.localPosition.y;
        cameraTargetY = defaultCamY;

        baseSprintSpeed = sprintSpeed;

        // Setup Animation component
        anim = gameObject.AddComponent<Animation>();
        if (idleClip != null) anim.AddClip(idleClip, "Idle");
        if (walkClip != null) anim.AddClip(walkClip, "Walk");
        if (sprintClip != null) anim.AddClip(sprintClip, "Sprint");

        if (idleClip != null)
        {
            anim.clip = idleClip;
            anim.Play("Idle");
        }
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
        else
        {
            stamina += staminaRegenRate * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        // Crouch toggle
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? crouchHeight : normalHeight;
            cameraTargetY = isCrouching ? defaultCamY * 0.6f : defaultCamY;
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

        ApplyHeadbob(isMoving, sprinting);
        AnimateCameraCrouch();

        // Play animations
        HandleAnimations(isMoving, sprinting);
    }

    void HandleAnimations(bool isMoving, bool isSprinting)
    {
        if (anim == null) return;

        if (!isMoving)
        {
            if (idleClip != null && !anim.IsPlaying("Idle"))
                anim.CrossFade("Idle");
        }
        else if (isSprinting)
        {
            if (sprintClip != null && !anim.IsPlaying("Sprint"))
                anim.CrossFade("Sprint");
        }
        else
        {
            if (walkClip != null && !anim.IsPlaying("Walk"))
                anim.CrossFade("Walk");
        }
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

    void ApplyHeadbob(bool isMoving, bool isSprinting)
    {
        if (controller.isGrounded && isMoving)
        {
            float bobSpeed = isSprinting ? sprintBobSpeed : (isCrouching ? crouchBobSpeed : walkBobSpeed);
            bobTimer += Time.deltaTime * bobSpeed;

            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;
            Vector3 localPos = cameraTransform.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, cameraTargetY + bobOffset, Time.deltaTime * 8f);
            cameraTransform.localPosition = localPos;
        }
        else
        {
            Vector3 localPos = cameraTransform.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, cameraTargetY, Time.deltaTime * 8f);
            cameraTransform.localPosition = localPos;
            bobTimer = 0f;
        }
    }

    void AnimateCameraCrouch()
    {
        // Smooth crouch handled inside ApplyHeadbob by cameraTargetY
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
            sprintSpeed = baseSprintSpeed * 0.5f; // temporary slowdown
        }
        else if (stamina > 0.1f && breathingAudioSource.isPlaying)
        {
            breathingAudioSource.Stop();
            sprintSpeed = baseSprintSpeed;
        }
    }
}
