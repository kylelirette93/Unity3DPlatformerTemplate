using UnityEngine;

/// <summary>
/// Handles the "explosion" effect of an object by detaching and propelling its child objects outward.
/// Commonly used for breaking objects into pieces or spawning collectibles.
/// </summary>
public class ExplosionSpawner : MonoBehaviour 
{
    [Header("Explosion Settings")]
    [Tooltip("Sound played when the object explodes")]
    public AudioClip explosionSound;
    
    [Tooltip("Time before the parent object is destroyed")]
    [Min(0)]
    public float destructionDelay;
    
    [Tooltip("Whether child objects should be destroyed with the parent")]
    public bool destroyChildren;
    
    [Tooltip("Force applied to push children away from the center")]
    [Min(0)]
    public float explosionForce;
    
    /// <summary>
    /// Initializes the explosion effect on start, handling child objects and playing effects
    /// </summary>
    void Start()
    {
        Transform[] fragments = GetChildObjects();
        
        if (!destroyChildren)
        {
            DetachAndPropelFragments(fragments);
        }
        
        TriggerExplosion();
    }
    
    /// <summary>
    /// Creates an array of all child object transforms
    /// </summary>
    /// <returns>Array of child transforms that will be affected by the explosion</returns>
    private Transform[] GetChildObjects()
    {
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }
        return children;
    }
    
    /// <summary>
    /// Detaches children from the parent and applies explosion forces to those with Rigidbodies
    /// </summary>
    /// <param name="fragments">Array of transforms to be propelled outward</param>
    private void DetachAndPropelFragments(Transform[] fragments)
    {
        transform.DetachChildren();
        
        if (explosionForce <= 0) return;
        
        foreach (Transform fragment in fragments)
        {
            if (fragment.TryGetComponent<Rigidbody>(out var rigidbody))
            {
                Vector3 explosionDirection = fragment.position - transform.position;
                rigidbody.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
                rigidbody.AddTorque(Random.insideUnitSphere * explosionForce, ForceMode.Impulse);
            }
        }
    }
    
    /// <summary>
    /// Plays explosion sound effect and schedules the object for destruction
    /// </summary>
    private void TriggerExplosion()
    {
        if (explosionSound)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }
        
        Destroy(gameObject, destructionDelay);
    }
} 