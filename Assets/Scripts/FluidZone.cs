using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Applies fluid physics effects (buoyancy, drag) to objects entering the zone.
/// Commonly used for water, but can be adapted for other fluid-like behaviors.
/// </summary>
/// <remarks>
/// Features:
/// - Applies buoyancy force based on object depth
/// - Configurable drag effects for fluid resistance
/// - Optional player interaction
/// - Splash sound effects with velocity-based volume
/// - Automatic physics restoration on exit
/// 
/// Setup:
/// 1. Add to an object with a BoxCollider
/// 2. Adjust the collider size for your fluid area
/// 3. Configure buoyancy and drag settings
/// 4. Optionally add splash sounds
/// </remarks>
[RequireComponent(typeof(BoxCollider))]
public class FluidZone : MonoBehaviour 
{
    [Header("Fluid Effects")]
    [Tooltip("Sound played when objects enter the fluid")]
    public AudioClip immersionSound;
    
    [Tooltip("Force applied to objects in the fluid (e.g., buoyancy, current)")]
    public Vector3 buoyancyForce = new Vector3(0, 29.0f, 0);
    
    [Header("Physics Settings")]
    [Tooltip("Whether to apply fluid physics to the player")]
    public bool affectPlayer = true;
    
    [Tooltip("Fluid resistance applied to object movement (0 = none, 1 = maximum)")]
    [Range(0, 1)]
    public float linearDrag = 0.4f;
    
    [Tooltip("Fluid resistance applied to object rotation (0 = none, 1 = maximum)")]
    [Range(0, 1)]
    public float angularDrag = 0.2f;
    
    /// <summary>
    /// Stores original physics values as (linearDrag, angularDrag) tuples
    /// </summary>
    private Dictionary<GameObject, (float linear, float angular)> originalDragValues 
        = new Dictionary<GameObject, (float linear, float angular)>();
    
    /// <summary>
    /// Validates component requirements and settings on startup
    /// </summary>
    void Awake()
    {
        ValidateSetup();
    }
    
    /// <summary>
    /// Ensures proper tag and trigger settings
    /// </summary>
    private void ValidateSetup()
    {
        if (!CompareTag("Water"))
        {
            tag = "Water";
            Debug.LogWarning($"FluidZone on {gameObject.name} requires 'Water' tag. Tag has been added.", this);
        }
        
        GetComponent<Collider>().isTrigger = true;
    }
    
    /// <summary>
    /// Applies continuous buoyancy force to objects in the fluid
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        if (!other.TryGetComponent<Rigidbody>(out var rigidbody)) return;
        
        ApplyBuoyancy(rigidbody);
    }
    
    /// <summary>
    /// Calculates and applies depth-based buoyancy force
    /// </summary>
    /// <param name="rigidbody">The rigidbody to apply forces to</param>
    private void ApplyBuoyancy(Rigidbody rigidbody)
    {
        float surfaceHeight = transform.position.y + GetComponent<Collider>().bounds.extents.y;
        float objectDepth = surfaceHeight - rigidbody.position.y;
        
        // Apply full force when deep, reduced force near surface to prevent jittering
        Vector3 force = objectDepth > 0.4f ? 
            buoyancyForce : 
            buoyancyForce * (objectDepth * 2);
            
        rigidbody.AddForce(force, ForceMode.Force);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Rigidbody>(out var rigidbody)) 
        {
            PlaySplashSound(null);
            return;
        }
        
        if (!ShouldAffectObject(rigidbody)) return;
        
        PlaySplashSound(rigidbody);
        StoreDragValues(rigidbody);
        ApplyFluidPhysics(rigidbody);
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Rigidbody>(out var rigidbody)) return;
        if (!ShouldAffectObject(rigidbody)) return;
        
        RestoreOriginalPhysics(rigidbody);
    }
    
    private bool ShouldAffectObject(Rigidbody rigidbody)
    {
        return affectPlayer || !rigidbody.CompareTag("Player");
    }
    
    private void PlaySplashSound(Rigidbody rigidbody)
    {
        if (!immersionSound) return;
        
        float volume = rigidbody != null ? 
            Mathf.Clamp01(rigidbody.velocity.magnitude / 5f) : 
            1f;
            
        AudioSource.PlayClipAtPoint(immersionSound, transform.position, volume);
    }
    
    private void StoreDragValues(Rigidbody rigidbody)
    {
        originalDragValues[rigidbody.gameObject] = (rigidbody.drag, rigidbody.angularDrag);
    }
    
    private void ApplyFluidPhysics(Rigidbody rigidbody)
    {
        rigidbody.drag = linearDrag;
        rigidbody.angularDrag = angularDrag;
    }
    
    private void RestoreOriginalPhysics(Rigidbody rigidbody)
    {
        if (originalDragValues.TryGetValue(rigidbody.gameObject, out var dragValues))
        {
            rigidbody.drag = dragValues.linear;
            rigidbody.angularDrag = dragValues.angular;
            originalDragValues.Remove(rigidbody.gameObject);
        }
        else
        {
            // Fallback to Unity's default values
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0.05f;
            Debug.LogWarning($"Original physics values not found for {rigidbody.gameObject.name}. Restored defaults.", rigidbody);
        }
    }
} 