using UnityEngine;

/// <summary>
/// Controls enemy behavior including movement, attacks, and player interactions.
/// Requires MovementController for locomotion and DealDamage for combat.
/// </summary>
[RequireComponent(typeof(MovementController))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public bool flying = false;
    [Header("Combat")]
    [Tooltip("Force applied to player when bouncing on enemy")]
    public Vector3 playerBounceForce = new Vector3(0, 13, 0);
    [Tooltip("Sound played when player bounces on enemy")]
    public AudioClip bounceSound;
    [Tooltip("Force to push player away when attacked")]
    public float knockbackForce = 10f;
    [Tooltip("Upward force when player is knocked back")]
    public float knockbackHeight = 7f;
    [Tooltip("Damage dealt to player on contact")]
    public int contactDamage = 1;
    
    [Header("Detection")]
    [Tooltip("Whether this enemy should chase detected objects")]
    public bool enableChasing = true;
    [Tooltip("Ignore height difference when chasing")]
    public bool ignoreHeight = true;
    [Tooltip("Minimum distance to maintain from target")]
    public float minTargetDistance = 0.7f;
    [Tooltip("Vision detection sensor")]
    public TriggerSensor visionSensor;
    [Tooltip("Attack detection sensor")]
    public TriggerSensor attackSensor;
    
    private MovementController movement;
    private Animator animator;
    private HealthController health;

    void Awake()
    {
        if (!CompareTag("Enemy")) {
            tag = "Enemy";
        }
        TryGetComponent(out animator);
        TryGetComponent(out movement);
        TryGetComponent(out health);
    }
    
    void Update()
    {
        if (enableChasing && visionSensor.IsDetectingObject)
        {
            ChaseTarget(visionSensor.DetectedObject.transform.position);
        }
        
        if (attackSensor.WasDetectedThisFrame)
        {
            AttackTarget(attackSensor.DetectedObject);
        }

        if (animator)
        {
            animator.SetFloat(MovementController.AnimationID_DistanceToTarget, movement.distanceToDestination);
            animator.SetFloat(MovementController.AnimationID_YVelocity, movement.CurrentVelocity.y);
            animator.SetBool(MovementController.AnimationID_Death, health.isDead);
        }
    }
    
    void FixedUpdate()
    {
        movement.ApplyVelocityControl(movement.Friction, movement.maxVelocity, ignoreHeight);
    }
    
    /// <summary>
    /// Moves the enemy towards a target position
    /// </summary>
    private void ChaseTarget(Vector3 targetPosition)
    {
        movement.ApplyMovementForceTowards(targetPosition, movement.Acceleration, minTargetDistance, ignoreHeight);
        movement.AlignWithVelocity(!flying);
    }
    
    /// <summary>
    /// Handles attack logic when a target is in range
    /// </summary>
    private void AttackTarget(GameObject target)
    {
        gameObject.ApplyDamageAndKnockback(target, contactDamage, knockbackHeight, knockbackForce);
    }
    
    /// <summary>
    /// Called when a player bounces on this enemy
    /// </summary>
    public void HandlePlayerBounce()
    {
        if (bounceSound)
        {
            AudioSource.PlayClipAtPoint(bounceSound, transform.position);
        }
    }
} 