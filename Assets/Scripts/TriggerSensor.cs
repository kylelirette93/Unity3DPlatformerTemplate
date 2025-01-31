using UnityEngine;

/// <summary>
/// Detects and tracks objects entering its trigger zone.
/// Used for enemy vision, attack ranges, and other detection zones.
/// Requires a Collider component set to trigger mode.
/// </summary>
/// <remarks>
/// Common uses:
/// - Enemy line of sight detection
/// - Attack range detection
/// - Pickup/collection zones
/// - Area triggers for events
/// </remarks>
[RequireComponent(typeof(Collider))]
public class TriggerSensor : MonoBehaviour 
{
    [Header("Detection Settings")]
    [Tooltip("Tags to filter detection. If empty, detects all objects")]
    [SerializeField] private string[] detectionTags;
    
    /// <summary>
    /// True only during the frame when an object first enters the trigger zone.
    /// Useful for one-time trigger events.
    /// </summary>
    public bool WasDetectedThisFrame { get; private set; }
    
    /// <summary>
    /// True while any valid object remains within the trigger zone.
    /// Resets to false if no objects are detected.
    /// </summary>
    public bool IsDetectingObject { get; private set; }
    
    /// <summary>
    /// Reference to the most recently detected GameObject.
    /// Null when no object is in the trigger zone.
    /// </summary>
    /// <remarks>
    /// If multiple objects are in the zone, this will reference the most recent one.
    /// Check tags to ensure you're detecting the intended object.
    /// </remarks>
    public GameObject DetectedObject { get; private set; }
    
    /// <summary>
    /// Validates trigger settings on startup
    /// </summary>
    void Awake()
    {
        ValidateTriggerCollider();
    }
    
    /// <summary>
    /// Ensures the collider is properly set up as a trigger
    /// </summary>
    private void ValidateTriggerCollider()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (!triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
            Debug.LogWarning($"Collider on {gameObject.name} was not set as trigger. This has been fixed automatically.", this);
        }
    }
    
    /// <summary>
    /// Called when an object enters the trigger zone
    /// </summary>
    /// <param name="other">The collider that entered the trigger</param>
    void OnTriggerEnter(Collider other)
    {
        if (!ShouldDetectObject(other)) return;
        
        WasDetectedThisFrame = true;
        DetectedObject = other.gameObject;
    }
    
    /// <summary>
    /// Called every frame for objects remaining in the trigger zone
    /// </summary>
    /// <param name="other">The collider that is inside the trigger</param>
    void OnTriggerStay(Collider other)
    {
        if (!ShouldDetectObject(other)) return;
        
        IsDetectingObject = true;
        DetectedObject = other.gameObject;
    }
    
    /// <summary>
    /// Resets detection states after all updates.
    /// Ensures one-frame detection states are properly cleared.
    /// </summary>
    void LateUpdate()
    {
        WasDetectedThisFrame = false;
        IsDetectingObject = false;
        DetectedObject = null;
    }
    
    /// <summary>
    /// Determines if an object should be detected based on tag filters
    /// </summary>
    /// <param name="other">The collider to check</param>
    /// <returns>True if the object should be detected, false otherwise</returns>
    private bool ShouldDetectObject(Collider other)
    {
        // If no tags specified, detect everything
        if (detectionTags == null || detectionTags.Length == 0) 
            return true;
        
        // Check if the object has any of the specified tags
        foreach (string tag in detectionTags)
        {
            if (other.CompareTag(tag)) 
                return true;
        }
        
        return false;
    }
} 