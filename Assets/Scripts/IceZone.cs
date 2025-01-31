using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates a slippery area where objects have reduced friction.
/// Useful for ice patches, slick surfaces, or other low-friction areas.
/// </summary>
/// <remarks>
/// Features:
/// - Configurable slipperiness level
/// - Dynamic physics material modification
/// - Optional player interaction
/// - Slide sound effects
/// - Automatic friction restoration
/// 
/// Setup:
/// 1. Add to an object with any Collider type
/// 2. Configure the slipperiness value (0-1)
/// 3. Optionally add slide sounds
/// 4. Ensure affected objects have physics materials
/// 
/// Note: Objects entering the zone must have a physics material assigned
/// to their collider for the effect to work.
/// </remarks>
[RequireComponent(typeof(Collider))]
public class IceZone : MonoBehaviour
{
    [Header("Ice Physics")]
    [Tooltip("How slippery the ice is (0 = normal friction, 1 = no friction)")]
    [Range(0, 1)]
    public float slipperiness = 0.9f;
    
    [Tooltip("Whether to affect the player's friction")]
    public bool affectPlayer = true;
    
    [Header("Effects")]
    [Tooltip("Sound played when objects start sliding")]
    public AudioClip slideSound;

    /// <summary>
    /// Stores original friction values for restoration
    /// </summary>
    private Dictionary<GameObject, float> originalFriction = new Dictionary<GameObject, float>();
    
    /// <summary>
    /// Tracks objects currently affected by the ice zone
    /// </summary>
    private List<GameObject> slidingObjects = new List<GameObject>();
    
    /// <summary>
    /// Ensures the collider is set up as a trigger
    /// </summary>
    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }
    
    /// <summary>
    /// Applies ice physics when objects enter the zone
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Collider>(out var collider)) return;
        if (!ShouldAffectObject(other)) return;
        
        ApplyIcePhysics(collider);
    }
    
    /// <summary>
    /// Applies ice physics to the given collider
    /// </summary>
    /// <param name="collider">The collider to modify</param>
    private void ApplyIcePhysics(Collider collider)
    {
        if (collider.material == null) return;
        
        // Store original friction
        originalFriction[collider.gameObject] = collider.material.dynamicFriction;


        if (collider.TryGetComponent(out MovementController mover))
        {
            mover.SetOverridingForceAndFriction(new Vector2(slipperiness, 1.0f - slipperiness));
        }

        // Create ice physics material
        PhysicMaterial iceMaterial = new PhysicMaterial
        {
            dynamicFriction = collider.material.dynamicFriction * (1 - slipperiness),
            staticFriction = 0, // Remove static friction for consistent sliding
            frictionCombine = PhysicMaterialCombine.Minimum,
            bounceCombine = PhysicMaterialCombine.Minimum
        };
        
        // Apply ice physics
        collider.material = iceMaterial;
        slidingObjects.Add(collider.gameObject);
        
        PlaySlideSound(collider);
    }
    
    /// <summary>
    /// Plays slide sound if conditions are met
    /// </summary>
    private void PlaySlideSound(Collider collider)
    {
        if (slideSound && collider.attachedRigidbody) {
            slideSound.PlaySound(collider.transform.position);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!ShouldAffectObject(other)) return;
        
        RestoreOriginalFriction(other);
    }
    
    private bool ShouldAffectObject(Collider other)
    {
        return affectPlayer || !other.CompareTag("Player");
    }
    
    private void RestoreOriginalFriction(Collider other)
    {
        if (other.TryGetComponent(out MovementController mover))
        {
            mover.SetOverridingForceAndFriction(Vector2.zero);
        }
        if (originalFriction.TryGetValue(other.gameObject, out float friction))
        {
            if (other.material != null)
            {
                other.material.dynamicFriction = friction;
                other.material.staticFriction = friction;
            }
            
            originalFriction.Remove(other.gameObject);
            slidingObjects.Remove(other.gameObject);
        }
    }
    
    void OnDestroy()
    {
        // Cleanup any remaining modified physics materials
        for (int i = slidingObjects.Count - 1; i >= 0; i--)
        {
            if (slidingObjects[i] != null && slidingObjects[i].TryGetComponent<Collider>(out var collider))
                RestoreOriginalFriction(collider);
        }
    }
} 