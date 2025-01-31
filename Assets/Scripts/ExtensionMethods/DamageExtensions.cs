using UnityEngine;

/// <summary>
/// Provides extension methods for dealing damage and applying knockback to objects.
/// </summary>
public static class DamageExtensions
{
    /// <summary>
    /// Applies damage and knockback to a target GameObject.
    /// </summary>
    /// <param name="attacker">The GameObject dealing the damage</param>
    /// <param name="target">The GameObject receiving the damage</param>
    /// <param name="damageAmount">Amount of damage to deal (negative for healing)</param>
    /// <param name="knockbackHeight">Upward force of the knockback</param>
    /// <param name="knockbackForce">Horizontal force of the knockback</param>
    public static void ApplyDamageAndKnockback(
        this GameObject attacker,
        GameObject target,
        int damageAmount,
        float knockbackHeight,
        float knockbackForce)
    {
        if (target == null) return;

        // Calculate knockback direction
        Vector3 knockbackDirection = (target.transform.position - attacker.transform.position).normalized;
        knockbackDirection.y = 0; // Flatten the direction for consistent horizontal force

        // Apply knockback if target has a non-kinematic rigidbody
        if (target.TryGetComponent<Rigidbody>(out Rigidbody targetRigidbody) && !targetRigidbody.isKinematic)
        {
            ApplyKnockback(targetRigidbody, knockbackDirection, knockbackHeight, knockbackForce);

            if (target.TryGetComponent<MovementController>(out MovementController targetMovement))
            {
                targetMovement.ApplyJumpSquashEffect();
            }
        }

        // Apply damage if target has health component
        if (target.TryGetComponent<HealthController>(out HealthController targetHealth) && !targetHealth.isInvulnerable)
        {
            targetHealth.ApplyDamage(damageAmount);
        }
    }

    /// <summary>
    /// Applies damage to a target GameObject.
    /// </summary>
    /// <param name="attacker">The GameObject dealing the damage</param>
    /// <param name="target">The GameObject receiving the damage</param>
    /// <param name="damageAmount">Amount of damage to deal (negative for healing)</param>
    public static void ApplyDamage(
        this GameObject attacker,
        GameObject target,
        int damageAmount)
    {
        if (target == null) return;

        // Apply damage if target has health component
        if (target.TryGetComponent<HealthController>(out HealthController targetHealth) && !targetHealth.isInvulnerable)
        {
            targetHealth.ApplyDamage(damageAmount);
        }
    }

    private static void ApplyKnockback(Rigidbody targetRigidbody, Vector3 direction, float height, float force)
    {
        targetRigidbody.velocity = Vector3.zero; // Reset velocity before applying knockback
        targetRigidbody.AddForce(direction * force, ForceMode.VelocityChange);
        targetRigidbody.AddForce(Vector3.up * height, ForceMode.VelocityChange);
    }
} 