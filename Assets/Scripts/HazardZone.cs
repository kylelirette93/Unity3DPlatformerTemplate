using System;
using UnityEngine;

[Flags]
public enum HazardDetection
{
    None = 0,
    TriggerEnter = 1,
    ColliderEnter = 2,
    ColliderContinuous = 4,
    TriggerContinuous = 8
}
/// <summary>
/// Represents an area or object that can damage and apply forces to specific tagged entities.
/// Examples include lava, spikes, or any dangerous environment elements.
/// 
/// If no vulnerable tags are specified, the hazard will affect all objects that collide with it.
/// This can be useful for universal hazards like lava or deadly pits.
/// </summary>
public class HazardZone : MonoBehaviour 
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage to deal on contact (negative values will heal instead)")]
    [SerializeField] private int damageAmount = 1;
    
    [Tooltip("Tags of entities that can be affected by this hazard. Leave empty to affect all objects")]
    [SerializeField] private string[] vulnerableTags = { "Player" };
    
    [Header("Force Settings")]
    [Tooltip("Horizontal force to apply when entity is hit (in units per second)")]
    [SerializeField] private float repulsionForce = 25f;
    
    [Tooltip("Upward force to apply when entity is hit (in units per second)")]
    [SerializeField] private float launchForce = 6f;

    [Tooltip("Cooldown between hits")]
    [SerializeField] private float cooldown = 0.0f;

    [Header("Collision Detection")]
    [Tooltip("How the hazard should detect objects")]
    [SerializeField] private HazardDetection DetectionMethod = HazardDetection.ColliderEnter;
    
    [Header("Feedback")]
    [Tooltip("Sound to play when the hazard affects an entity (optional)")]
    [SerializeField] private AudioClip impactSound;

    private float lastTimeAppliedHazard = 0.0f;

    /// <summary>
    /// Initialize components and validate setup
    /// </summary>
    private void Awake()
    {
        if (DetectionMethod == HazardDetection.None)
        {
            Debug.LogWarning($"HazardZone on {gameObject.name} has no detection types.", this);
        }
    }


    private void OnCollisionStay(Collision other)
    {
        if (other.collider.isTrigger) return;
        if (DetectionMethod.isDetectionMethodSet(HazardDetection.ColliderEnter) || !DetectionMethod.isDetectionMethodSet(HazardDetection.ColliderContinuous)) return;

        AttemptApplyDamageAndKnockback(other.collider);
    }

    /// <summary>
    /// Applies continuous force to objects staying within the trigger volume
    /// Only active if applyContinuousForce is enabled
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) return;
        if (DetectionMethod.isDetectionMethodSet(HazardDetection.TriggerEnter) || !DetectionMethod.isDetectionMethodSet(HazardDetection.TriggerContinuous)) return;

        AttemptApplyDamageAndKnockback(other);
    }

    private void AttemptApplyDamageAndKnockback(Collider other)
    {
        if (other.isTrigger) return;
        if (other.attachedRigidbody == null || !IsVulnerable(other.tag)) {
            return;
        }

        if (cooldown < 0.01f)
        {
            other.attachedRigidbody.AddForce(transform.up * launchForce, ForceMode.Force);
            
            if(damageAmount != 0)
                gameObject.ApplyDamage(other.gameObject, damageAmount);
            
            lastTimeAppliedHazard = Time.time;
        }
        else
        {
            lastTimeAppliedHazard = Time.time;
            gameObject.ApplyDamageAndKnockback(other.gameObject, damageAmount, launchForce, repulsionForce);
            PlayImpactFeedback();
        }
    }

    /// <summary>
    /// Handles physical collisions with the hazard
    /// Only active if detectCollisionEnter is enabled
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.isTrigger) return;
        if (!DetectionMethod.isDetectionMethodSet(HazardDetection.ColliderEnter) || DetectionMethod.isDetectionMethodSet(HazardDetection.ColliderContinuous)) return;
        
        if (IsVulnerable(collision.gameObject.tag)) {
            ApplyHazardEffect(collision.gameObject);
        }
    }

    /// <summary>
    /// Handles trigger volume entry
    /// Only active if detectTriggerEnter is enabled and not using continuous force
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (!DetectionMethod.isDetectionMethodSet(HazardDetection.TriggerEnter) || DetectionMethod.isDetectionMethodSet(HazardDetection.TriggerContinuous)) return;

        if (IsVulnerable(other.tag)) {
            ApplyHazardEffect(other.gameObject);
        }
    }

    /// <summary>
    /// Applies damage and force to the affected entity and plays feedback
    /// </summary>
    /// <param name="target">The GameObject to affect</param>
    private void ApplyHazardEffect(GameObject target)
    {
        gameObject.ApplyDamageAndKnockback(
            target: target,
            damageAmount: damageAmount,
            knockbackHeight: launchForce,
            knockbackForce: repulsionForce
        );
        lastTimeAppliedHazard = Time.time;
        PlayImpactFeedback();
    }

    /// <summary>
    /// Checks if the given tag is vulnerable to this hazard
    /// Returns true if no specific tags are set (affecting all objects)
    /// </summary>
    /// <param name="targetTag">The tag to check</param>
    /// <returns>True if the object should be affected by this hazard</returns>
    private bool IsVulnerable(string targetTag)
    {
        if (!lastTimeAppliedHazard.HasTimeElapsedSince(cooldown)) return false;
        // If no tags are specified, affect all objects
        if (vulnerableTags == null || vulnerableTags.Length == 0) {
            return true;
        }

        return System.Array.Exists(vulnerableTags, tag => tag == targetTag);
    }

    /// <summary>
    /// Plays audio feedback when the hazard affects an entity
    /// Only plays if an impact sound is assigned
    /// </summary>
    private void PlayImpactFeedback()
    {
        if (impactSound != null) {
            impactSound.PlaySound(transform.position);
        }
    }
} 

public static class HazardDetectionEnumExtensionMethods
{
    public static bool isDetectionMethodSet(this HazardDetection checkingIn, HazardDetection hazardFlag)
    {
        return (checkingIn & hazardFlag) == hazardFlag;
    }

}