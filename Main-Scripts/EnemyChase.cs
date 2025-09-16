using UnityEngine;
using System.Collections;

public class EnemyChaseThreeSideFOVNoRigidbody : MonoBehaviour
{
    [Header("References")]
    public Transform player;             
    public Animation anim;               
    public AudioSource attackSound;      

    [Header("Animation Clips (Optional)")]
    public AnimationClip walkAnimation;  
    public AnimationClip attackAnimation; 

    [Header("Movement Settings")]
    public float speed = 5f;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;

    [Header("Side FOV Settings")]
    public float fovAngle = 60f;      // Half-angle of each FOV
    public float fovDistance = 10f;   // How far each FOV can detect

    [Header("Idle / Random Movement")]
    public float idleRotateSpeed = 60f;   
    public float idleRotateInterval = 3f; 
    public float forwardRayDistance = 1f; 
    public LayerMask obstacleLayers = 1;  

    private bool isAttacking = false;
    private float idleTimer = 0f;
    private float idleAngle = 0f;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (anim != null)
        {
            if (walkAnimation != null && anim.GetClip(walkAnimation.name) == null) 
                anim.AddClip(walkAnimation, walkAnimation.name);
            if (attackAnimation != null && anim.GetClip(attackAnimation.name) == null) 
                anim.AddClip(attackAnimation, attackAnimation.name);
        }
    }

    private void Update()
    {
        if (isAttacking) return;
        if (player == null) { IdleBehavior(); return; }

        Vector3 toPlayer = player.position - transform.position;
        Vector3 flatToPlayer = new Vector3(toPlayer.x, 0, toPlayer.z);

        // Define three sides of the cube for FOV
        Vector3[] fovSides = new Vector3[3];
        fovSides[0] = transform.forward;                         // Front
        fovSides[1] = Quaternion.Euler(0, -90f, 0) * transform.forward; // Left
        fovSides[2] = Quaternion.Euler(0, 90f, 0) * transform.forward;  // Right

        bool seesPlayer = false;
        Vector3 chosenDirection = transform.forward;

        // Check each side FOV
        foreach (Vector3 side in fovSides)
        {
            if (Vector3.Angle(side, flatToPlayer) < fovAngle / 2f &&
                !Physics.Raycast(transform.position + Vector3.up * 0.5f, flatToPlayer.normalized, flatToPlayer.magnitude, obstacleLayers))
            {
                seesPlayer = true;
                chosenDirection = side;
                break; // Pick the first side that sees the player with clear path
            }
        }

        if (seesPlayer)
        {
            float distance = flatToPlayer.magnitude;

            if (distance > attackRange)
            {
                MoveForward(speed, chosenDirection);
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

    private void MoveForward(float moveSpeed, Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
        }

        // Move forward if path is clear
        if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, forwardRayDistance, obstacleLayers))
        {
            transform.Translate(transform.forward * moveSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            idleAngle = Random.Range(-180f, 180f); // Rotate if blocked
        }
    }

    private void IdleBehavior()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleRotateInterval)
        {
            idleAngle = Random.Range(-180f, 180f);
            idleTimer = 0f;
        }

        Quaternion targetRotation = Quaternion.Euler(0, idleAngle, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, idleRotateSpeed * Time.deltaTime);

        if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, forwardRayDistance, obstacleLayers))
        {
            transform.Translate(transform.forward * speed * 0.5f * Time.deltaTime, Space.World);
            PlayWalking(true);
        }
        else
        {
            PlayWalking(false);
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        PlayWalking(false);

        if (anim != null && attackAnimation != null)
        {
            anim.Play(attackAnimation.name);
            yield return new WaitForSeconds(attackAnimation.length);
            if (walkAnimation != null) anim.Play(walkAnimation.name);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw three side FOVs
        Gizmos.color = Color.cyan;
        Vector3[] fovSides = { transform.forward, 
                               Quaternion.Euler(0, -90f, 0) * transform.forward, 
                               Quaternion.Euler(0, 90f, 0) * transform.forward };

        foreach (var side in fovSides)
            Gizmos.DrawRay(transform.position, side * fovDistance);
    }
}
