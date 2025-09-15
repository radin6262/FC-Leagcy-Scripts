using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;
    public float maxLeftAngle = -90f;  // 90 degrees left
    public float maxRightAngle = 90f;   // 90 degrees right
    
    [Header("Idle Movement Settings")]
    public float idleRollSpeed = 15f;
    public float idleRollAmount = 5f;
    public float idleReturnSpeed = 5f;
    public float idleMovementDelay = 3f; // Seconds before idle movement starts
    
    [Header("VHS Effect Settings")]
    public Material vhsMaterial;
    public float effectIntensity = 1f;
    public float staticIntensity = 0.1f;
    
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float currentRotationY = 0f;
    private Vector3 lastPosition;
    private float motionAmount;
    private float idleTimer = 0f;
    private float currentIdleRoll = 0f;
    private float targetIdleRoll = 0f;
    
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        lastPosition = transform.position;
        
        if (vhsMaterial != null)
        {
            vhsMaterial.SetFloat("_MotionBlurIntensity", 0f);
        }
    }
    
    void Update()
    {
        // Calculate motion for motion blur effect
        motionAmount = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;
        
        // Update VHS effect based on motion
        if (vhsMaterial != null)
        {
            float motionBlur = Mathf.Clamp(motionAmount * 10f * effectIntensity, 0f, 0.8f);
            vhsMaterial.SetFloat("_MotionBlurIntensity", motionBlur);
            vhsMaterial.SetFloat("_NoiseIntensity", staticIntensity);
        }
        
        bool isRotating = Input.GetMouseButton(0);
        
        // Rotate menu with mouse drag (left click)
        if (isRotating)
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed;
            
            // Calculate new rotation with clamping
            currentRotationY += rotX;
            currentRotationY = Mathf.Clamp(currentRotationY, maxLeftAngle, maxRightAngle);
            
            // Reset idle timer when player is interacting
            idleTimer = 0f;
            targetIdleRoll = 0f; // Reset target roll
        }
        
        // Handle idle movement
        if (!isRotating)
        {
            idleTimer += Time.deltaTime;
            
            // Start idle movement after delay
            if (idleTimer > idleMovementDelay)
            {
                // Set new random target if we've reached the current one
                if (Mathf.Approximately(currentIdleRoll, targetIdleRoll))
                {
                    targetIdleRoll = Random.Range(-idleRollAmount, idleRollAmount);
                }
                
                // Smoothly move toward target roll
                currentIdleRoll = Mathf.Lerp(currentIdleRoll, targetIdleRoll, Time.deltaTime * idleRollSpeed);
            }
            else
            {
                // Smoothly return to zero if interrupted during delay
                currentIdleRoll = Mathf.Lerp(currentIdleRoll, 0f, Time.deltaTime * idleReturnSpeed);
            }
        }
        else
        {
            // Smoothly return to zero when interrupted
            currentIdleRoll = Mathf.Lerp(currentIdleRoll, 0f, Time.deltaTime * idleReturnSpeed);
        }
        
        // Apply combined rotation
        transform.localEulerAngles = new Vector3(
            initialRotation.eulerAngles.x + currentIdleRoll, // Add idle roll to X rotation
            currentRotationY,                                // Current Y rotation
            initialRotation.eulerAngles.z                    // Maintain original Z rotation
        );
        
        // Reset view
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            currentRotationY = 0f;
            currentIdleRoll = 0f;
            targetIdleRoll = 0f;
            idleTimer = 0f;
        }
    }
    
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (vhsMaterial != null)
        {
            Graphics.Blit(src, dest, vhsMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}