using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    [Tooltip("How fast the player moves during dash")]
    public float dashSpeed = 20f;
    [Tooltip("How long the dash lasts")]
    public float dashDuration = 0.3f;
    [Tooltip("Cooldown before player can dash again")]
    public float dashCooldown = 1f;
    [Tooltip("How much to keep the player up in the air during dash")]
    [Range(0, 1)]
    public float airDashUpwardsForce = 0.5f;
    [Tooltip("Sound played when dashing")]
    public AudioClip dashAudio;
    [Tooltip("Particle system for dash effect")]
    public ParticleSystem dashParticles;
    [Tooltip("Whether player is invulnerable while dashing")]
    public bool dashInvulnerability = true;

    [Tooltip("Curve to control dash force along the dash.")]
    public AnimationCurve dashCurve = new AnimationCurve();
    // Dash state
    private bool isDashing;
    private float dashEndTime, dashStartTime;
    private float nextDashTime;
    private Vector3 dashDirection;


    private Animator animator;
    private MovementController moveController;
    private Rigidbody rb;
    private HealthController healthComponent;

    private void Start()
    {
        TryGetComponent(out rb);
        TryGetComponent(out healthComponent);
        TryGetComponent(out moveController);
        TryGetComponent(out animator);
    }

    void FixedUpdate()
    {
        // Handle dash movement
        if (isDashing)
        {
            UpdateDash();
            return; // Skip normal movement while dashing
        }
    }

    /// <summary>
    /// Attempts to start a dash if conditions are met
    /// </summary>
    public void TryStartDash(Vector3 _dashDirection)
    {
        if (isDashing || Time.time < nextDashTime) return;

        // Get dash direction based on input or facing direction
        dashDirection = _dashDirection.normalized;
        if (dashDirection == Vector3.zero)
        {
            dashDirection = animator ? animator.transform.forward : transform.forward;
        }

        // Start dash
        isDashing = true;
        dashStartTime = Time.time;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = dashEndTime + dashCooldown;
        moveController.enabled = false;

        if (animator)
            animator.SetTrigger(MovementController.AnimationID_Dash);

        if (dashAudio)
            dashAudio.PlaySound(transform.position);

        if (dashParticles)
            dashParticles.Play();

        // Enable invulnerability
        if (dashInvulnerability && healthComponent != null)
            healthComponent.SetInvulnerable(true);
    }

    /// <summary>
    /// Update dash state and handle dash movement
    /// </summary>
    private void UpdateDash()
    {
        if (!isDashing) return;

        // Check if dash should end
        if (Time.time >= dashEndTime)
        {
            EndDash();
            return;
        }

        float dashProgress = Mathf.InverseLerp(dashStartTime, dashEndTime, Time.time);
        // Apply dash movement
        rb.velocity = dashDirection * dashSpeed * dashCurve.Evaluate(dashProgress);
        if (dashProgress > 0.85f && moveController != null)
            moveController.enabled = true;


        // Reduce gravity if dashing in air
        if ((moveController is AdvancedMoveController) && !((AdvancedMoveController)moveController).isGrounded)
        {
            rb.AddForce(new Vector3(0.0f, airDashUpwardsForce, 0.0f), ForceMode.Force);
        }
    }

    /// <summary>
    /// End dash and restore normal movement
    /// </summary>
    private void EndDash()
    {
        isDashing = false;

        if (moveController) moveController.enabled = true;
        // Disable invulnerability
        if (dashInvulnerability && healthComponent != null)
        {
            healthComponent.SetInvulnerable(false);
        }

        if (dashParticles)
        {
            dashParticles.Stop();
        }
    }
}
