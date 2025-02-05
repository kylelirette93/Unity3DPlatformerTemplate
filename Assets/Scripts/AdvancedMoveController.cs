using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Extends basic MovementController with advanced features for character movement.
/// Handles ground detection, slope interactions, jumping mechanics, and platform movement.
/// This controller can be used by both player characters and AI-controlled entities.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class AdvancedMoveController : MovementController
{
    [Header("Ground Detection")]
    [Tooltip("Maximum stair step that the character can climb")]
    public float maxStepClimbable = 2.0f;
    [Tooltip("Maximum slope angle (in degrees) that the character can traverse")]
    public float maxTraversableSlope = 40f;
    [Tooltip("Speed at which the character slides down non-traversable slopes")]
    public float slideSpeed = 42f;
    [Tooltip("Force multiplier to maintain position on moving platforms")]
    public float platformGripForce = 7.7f;

    [Header("Combat")]
    [Tooltip("Damage applied when jumping on an enemy")]
    public int stompDamage = 1;
    [Header("Air Movement Settings")]
    [Tooltip("Air rotation speed")]
    public float airRotationSpeed = 0.38f;
    public float airAcceleration = 18f;
    public float airFriction = 1.1f;

    [Header("Jump Settings")]
    [Tooltip("Jump forces for consecutive jumps (base jump, double jump, etc)")]
    public Vector3[] consecutiveJumpForces = new Vector3[]{
        new Vector3(0, 13, 0),
        new Vector3(0, 16, 0),
        new Vector3(0, 17, 6)
    };
    [Tooltip("Time window for chaining jumps")]
    public float chainJumpWindow = 0.1f;
    [Tooltip("Time window to buffer jump input before landing")]
    public float jumpBufferTime = 0.17f;

    [Header("Jump Audio Feedback")]
    [Tooltip("Sound effect played when jumping")]
    public AudioClip jumpAudio;
    [Tooltip("Sound effect played when landing")]
    public AudioClip landAudio;

    [Header("Events")]
    public UnityEvent onJumpPerformed = new UnityEvent();
    public UnityEvent onLandingPerformed = new UnityEvent();

    // State properties
    public bool isGrounded { get; private set; }
    public float slopeAngle { get; private set; }
    public Vector3 platformVelocity { get; private set; }
    public float timeGrounded { get; private set; }
    public float lastJumpRequestTime { get; private set; } = -50f;
    public float lastJumpedTime { get; private set; } = -50f;
    public int jumpChainCount { get; private set; }
    public int bounceComboCount { get; set; } = 0;

    private float lastTimeTookStep;
    private Vector3 slideDirection = Vector3.zero;
    private float slideDuration = 0f;
    private RaycastHit[] groundHits = new RaycastHit[4];
    private bool wasGrounded;
    private Vector3 lastReceivedMovementDirection;

    private float currentFriction;

    /// <summary>
    /// Updates ground detection and movement parameters. Should be called in FixedUpdate.
    /// Handles ground detection, slope interactions, and jump leniency timing.
    /// </summary>
    public void UpdateMovement()
    {
        isGrounded = CheckGroundContact();
        timeGrounded = isGrounded ? timeGrounded + Time.deltaTime : 0f;

        // Update movement parameters based on ground state, lerping so landing isn't so jarring if input direction isn't zero.
        float desiredFriction = isGrounded ? Friction : airFriction;
        currentFriction = Mathf.Lerp(currentFriction, desiredFriction, Time.deltaTime * (desiredFriction < currentFriction || lastReceivedMovementDirection.magnitude < 0.05f ? 18f : 2.82f));

        // Apply movement controls
        ApplyVelocityControl(currentFriction, maxVelocity + platformVelocity.magnitude, true);

        // Handle landing and buffered jumps
        if (isGrounded && !wasGrounded && lastTimeTookStep.HasTimeElapsedSince(0.2f))
        {
            if (landAudio)
                landAudio.PlaySound(transform.position);

            ApplyLandingSquashEffect();
            onLandingPerformed.Invoke();
            if (lastJumpRequestTime + jumpBufferTime > Time.time) {
                PerformJump();
            }
        }
        
        wasGrounded = isGrounded;
    }

    /// <summary>
    /// Processes jump input request and initiates jump if conditions are met.
    /// Stores jump request time for jump leniency feature.
    /// </summary>
    public bool RequestJump()
    {
        lastJumpRequestTime = Time.time;
        if (((isGrounded && slopeAngle < maxTraversableSlope) || overrideCanJump) && lastJumpedTime + 0.15f < lastJumpRequestTime) {
            PerformJump();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Executes the jump with appropriate force based on consecutive jump count.
    /// </summary>
    private void PerformJump()
    {
        // Prevent jumping too close together from last jump.
        if (lastJumpedTime + 0.15f > Time.time)
            return;
        lastJumpedTime = Time.time;
        jumpChainCount = (timeGrounded < chainJumpWindow) ? 
            Mathf.Min(2, jumpChainCount + 1) : 0;
        Vector3 jumpForce = consecutiveJumpForces[
            Mathf.Min(jumpChainCount, consecutiveJumpForces.Length - 1)
        ];

        if (jumpAudio)
            jumpAudio.PlaySound(transform.position);
        ApplyJumpForce(jumpForce);
    }

    /// <summary>
    /// Applies the jump force.
    /// Resets vertical velocity before applying jump force to ensure consistent jump heights.
    /// </summary>
    private void ApplyJumpForce(Vector3 jumpForce)
    {
        // Reset vertical velocity for consistent jump heights
        onJumpPerformed.Invoke();
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddRelativeForce(jumpForce, ForceMode.Impulse);
        ApplyJumpSquashEffect();
    }

    /// <summary>
    /// Handles character movement towards a target direction.
    /// Manages slope interactions and applies appropriate movement parameters based on grounded state.
    /// </summary>
    /// <param name="direction">Desired movement direction</param>
    /// <param name="shouldRotate">Whether the character should rotate to face movement direction</param>
    public void ApplyMovement(Vector3 direction, bool shouldTurn = true)
    {
        if (!enabled)
            return;
        lastReceivedMovementDirection = direction;
        Vector3 targetPosition = transform.position + direction;
        
        // Blend with slide direction on steep slopes
        if (slideDirection.magnitude > 0.01f)
        {
            direction = Vector3.Lerp(direction, slideDirection, 
                Mathf.Clamp(slideDuration, 0f, 1f));
        }
        float currentAcceleration = isGrounded ? Acceleration : airAcceleration;
        float currentTurnSpeed = isGrounded ? rotationSpeed : airRotationSpeed;

        ApplyMovementForceTowards(targetPosition, currentAcceleration, 0.7f, true);
        
        if (shouldTurn && rotationSpeed != 0 && direction.magnitude != 0)
        {
            AlignWithDirection(targetPosition, currentTurnSpeed * 5, true);
        }
    }

    /// <summary>
    /// Performs ground detection using SphereCast and handles slope/platform interactions.
    /// Uses multiple raycasts to ensure accurate ground detection and smooth movement.
    /// </summary>
    /// <returns>True if character is considered grounded</returns>
    private bool CheckGroundContact()
    {
        if (gameObject == null || GameManager.IsQuittingGame) return false;

        float dist = mainCollider.bounds.extents.y;
        int groundHitsFound = Physics.SphereCastNonAlloc(
            transform.TransformPoint(((CapsuleCollider)mainCollider).center),
            ((CapsuleCollider)mainCollider).radius,
            Vector3.down,
            groundHits,
            dist * 0.33f,
            GameManager.Instance.groundMask,
            QueryTriggerInteraction.Ignore
        );

        platformVelocity = Vector3.zero;
        
        for (int i = 0; i < groundHitsFound; i++) {
            RaycastHit hit = groundHits[i];
            
            // Slope handling
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle > maxTraversableSlope && hit.transform.tag != "Pushable")
            {
                slideDuration += Time.deltaTime * 0.22f;
                Vector3 reflectedVector = Vector3.Reflect(Vector3.down, hit.normal);
                reflectedVector.y = 0f;
                slideDirection = Vector3.ProjectOnPlane(
                    Vector3.Reflect(reflectedVector.normalized, hit.normal),
                    hit.normal
                ) * slideSpeed;
                rb.AddForce(slideDirection, ForceMode.Force);
            }
            else
            {
                slideDirection = Vector3.zero;
                slideDuration = 0f;
            }

            if (hit.transform.CompareTag("Enemy") && rb.velocity.y < 0 && hit.transform.TryGetComponent(out EnemyController enemy))
            {
                enemy.HandlePlayerBounce();
                gameObject.ApplyDamageAndKnockback(enemy.gameObject, stompDamage, 0f, 0f);
                bounceComboCount += 1;
                ApplyJumpForce(enemy.playerBounceForce + (new Vector3(0f, 1.5f, 0f) * bounceComboCount));
            } else {
                bounceComboCount = 0;
            }
            // Moving platform handling
            if ((hit.transform.CompareTag("MovingPlatform") || hit.transform.CompareTag("Pushable")) 
                && hit.rigidbody != null)
            {
                platformVelocity = hit.rigidbody.velocity.SetY(0);
                rb.AddForce(
                    platformVelocity * platformGripForce * Time.fixedDeltaTime,
                    ForceMode.VelocityChange
                );
            }
            
            Vector3 foundGroundPos = hit.point;
            // Attempt find stairs'
            if (lastReceivedMovementDirection.magnitude > 0.15f && lastTimeTookStep.HasTimeElapsedSince(0.25f)) {
                groundHitsFound = Physics.SphereCastNonAlloc(
                    transform.TransformPoint(((CapsuleCollider)mainCollider).center + Vector3.up * 0.25f) + lastReceivedMovementDirection.SetY(0).normalized * ((CapsuleCollider)mainCollider).radius * 1.42f,
                    ((CapsuleCollider)mainCollider).radius * 0.4f,
                    Vector3.down,
                    groundHits,
                    dist * 0.33f,
                    GameManager.Instance.groundMask,
                    QueryTriggerInteraction.Ignore
                );
                for (int z = 0; z < groundHitsFound; z++)
                {
                    float stepDifference = groundHits[z].point.y - foundGroundPos.y;
                    
                    if (stepDifference > 0.04f && stepDifference < maxStepClimbable)
                    {
                        rb.AddForce(Vector3.up *stepDifference * 14.5f, ForceMode.Impulse);
                        lastTimeTookStep = Time.time;
                        break;
                    }
                }
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Prevents unwanted sliding on walkable slopes when character wants to be stationary (inputVector is zero).
    /// Only affects slopes within the walkable slope limit.
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        if (lastReceivedMovementDirection.magnitude < 0.001f && other.collider.CompareTag("Untagged") && isGrounded)
        {
            if (rb.velocity.magnitude < 2 && slopeAngle < maxTraversableSlope)
            {
                rb.velocity = Vector3.zero;
            }
        }
    }
} 