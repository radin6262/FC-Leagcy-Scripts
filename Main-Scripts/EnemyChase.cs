using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChaseOptionalAnimation : MonoBehaviour
{
    [Header("References")]
    public Transform player;             // Assign your player
    public Animation anim;               // Optional Legacy Animation component
    public AudioSource attackSound;      // Optional attack sound

    [Header("Animation Clips (Optional)")]
    public AnimationClip walkAnimation;  // Optional walking clip
    public AnimationClip attackAnimation; // Optional attack clip

    [Header("Movement Settings")]
    public float speed = 5f;
    public float detectionRange = 10f;
    public float fieldOfView = 60f;
    public float attackRange = 1.5f;

    [Header("Idle / Random Movement")]
    public float idleRotateSpeed = 30f;   // degrees/sec
    public float idleRotateInterval = 3f; // seconds
    public float forwardRayDistance = 1f; // detects walls in front
    public LayerMask obstacleLayers = 1;  // Layers to consider as obstacles

    [Header("Wall Avoidance")]
    public float[] rayAngles = { 0f, 45f, -45f, 90f, -90f }; // Angles to check for obstacles
    public float avoidanceForce = 5f;    // How strongly to avoid obstacles

    private Rigidbody rb;
    public bool isAttacking = false;
    private float idleTimer = 0f;
    private float idleAngle = 0f;
    private bool hasPlayerReference = false;
    private Vector3 avoidanceDirection = Vector3.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Try to find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        hasPlayerReference = player != null;

        // Add animation clips if assigned and Animation component exists
        if (anim != null)
        {
            if (walkAnimation != null && anim.GetClip(walkAnimation.name) == null) 
                anim.AddClip(walkAnimation, walkAnimation.name);
            if (attackAnimation != null && anim.GetClip(attackAnimation.name) == null) 
                anim.AddClip(attackAnimation, attackAnimation.name);
        }
    }

    private void FixedUpdate()
    {
        if (isAttacking) return;

        // Check if we need to find the player again
        if (player == null && !hasPlayerReference)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
            {
                player = playerObj.transform;
                hasPlayerReference = true;
            }
        }

        // Calculate obstacle avoidance
        avoidanceDirection = CalculateAvoidanceDirection();

        if (player == null)
        {
            IdleBehavior();
            return;
        }

        Vector3 toPlayer = player.position - rb.position;
        float distance = toPlayer.magnitude;
        Vector3 lookDirection = new Vector3(toPlayer.x, 0, toPlayer.z);

        bool seesPlayer = distance < detectionRange && 
                         Vector3.Angle(transform.forward, lookDirection) < fieldOfView / 2f &&
                         !IsPathBlocked(toPlayer); // Check if path is clear

        if (seesPlayer)
        {
            idleTimer = 0f;

            if (distance > attackRange)
            {
                MoveForward(speed, lookDirection);
                PlayWalking(true);
            }
            else
            {
                PlayWalking(false);
                StartCoroutine(Attack());
            }
        }
        else
        {
            IdleBehavior();
        }
    }

    private Vector3 CalculateAvoidanceDirection()
    {
        Vector3 direction = Vector3.zero;
        int hitCount = 0;
        
        // Check multiple directions for obstacles
        foreach (float angle in rayAngles)
        {
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 rayDirection = rotation * transform.forward;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, rayDirection, 
                               out hit, forwardRayDistance, obstacleLayers))
            {
                // Add force away from the obstacle
                direction += hit.normal * avoidanceForce;
                hitCount++;
            }
        }
        
        // Average the avoidance direction if we hit multiple obstacles
        if (hitCount > 0)
        {
            direction /= hitCount;
            return direction.normalized;
        }
        
        return Vector3.zero;
    }

    private bool IsPathBlocked(Vector3 direction)
    {
        RaycastHit hit;
        return Physics.Raycast(transform.position + Vector3.up * 0.5f, direction.normalized, 
                              out hit, direction.magnitude, obstacleLayers);
    }

    private void MoveForward(float moveSpeed, Vector3 direction)
    {
        // Apply obstacle avoidance
        if (avoidanceDirection != Vector3.zero)
        {
            // Blend between desired direction and avoidance direction
            direction = Vector3.Lerp(direction, avoidanceDirection, 0.7f).normalized;
        }

        // Rotate toward direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, 360f * Time.fixedDeltaTime);
        }

        // Check for obstacles in front before moving
        if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, 
                            forwardRayDistance, obstacleLayers))
        {
            // Move in the forward direction (after rotation)
            Vector3 move = transform.forward * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
        }
        else
        {
            // If still blocked after avoidance, rotate away more aggressively
            float angle = Random.Range(90f, 270f);
            rb.rotation *= Quaternion.Euler(0f, angle, 0f);
        }
    }

    private void IdleBehavior()
    {
        idleTimer += Time.fixedDeltaTime;
        if (idleTimer >= idleRotateInterval)
        {
            idleAngle = Random.Range(-180f, 180f);
            idleTimer = 0f;
        }

        // Apply obstacle avoidance to idle movement too
        Vector3 desiredDirection = Quaternion.Euler(0f, idleAngle, 0f) * Vector3.forward;
        if (avoidanceDirection != Vector3.zero)
        {
            desiredDirection = Vector3.Lerp(desiredDirection, avoidanceDirection, 0.7f).normalized;
            idleAngle = Mathf.Atan2(desiredDirection.x, desiredDirection.z) * Mathf.Rad2Deg;
        }

        Quaternion targetRotation = Quaternion.Euler(0f, idleAngle, 0f);
        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, idleRotateSpeed * Time.fixedDeltaTime);

        // Check for obstacles before moving
        if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, 
                            forwardRayDistance, obstacleLayers))
        {
            // Move forward slowly
            Vector3 move = transform.forward * speed * 0.5f * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
            PlayWalking(true);
        }
        else
        {
            PlayWalking(false);
            // Change direction if we hit an obstacle
            idleAngle = Random.Range(-180f, 180f);
            idleTimer = idleRotateInterval; // Force direction change next frame
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        PlayWalking(false);

        Debug.Log("Attack beginning!");

        // Play attack animation if assigned
        if (anim != null && attackAnimation != null)
        {
            anim.Play(attackAnimation.name);
            // Wait for animation to finish
            yield return new WaitForSeconds(attackAnimation.length);
            
            // Ensure animation transitions back properly
            if (walkAnimation != null)
                anim.Play(walkAnimation.name);
        }
        else
        {
            // Default wait time if no animation
            yield return new WaitForSeconds(1f);
        }

        // Play attack sound if assigned
        if (attackSound != null)
            attackSound.Play();

        Debug.Log("Attack finished! Place your code here.");

        isAttacking = false;
    }

    private void PlayWalking(bool walking)
    {
        if (anim == null || walkAnimation == null) return;
        
        if (walking)
        {
            if (!anim.IsPlaying(walkAnimation.name))
                anim.Play(walkAnimation.name);
        }
        else
        {
            if (anim.IsPlaying(walkAnimation.name))
                anim.Stop(walkAnimation.name);
        }
    }

    // Visual debugging aids
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw field of view
        Gizmos.color = Color.cyan;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView/2, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView/2, 0) * transform.forward * detectionRange;
        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);
        
        // Draw obstacle detection rays
        Gizmos.color = Color.magenta;
        foreach (float angle in rayAngles)
        {
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 rayDirection = rotation * transform.forward;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, rayDirection * forwardRayDistance);
        }
        
        // Draw current avoidance direction
        if (avoidanceDirection != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, avoidanceDirection * 2f);
        }
    }
}