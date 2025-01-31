using UnityEngine;

/// <summary>
/// Base class for all objects that can be interacted with by the player.
/// Provides common functionality for physics-based interactions.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class Interactable : MonoBehaviour
{
    protected Rigidbody rb;
    protected RigidbodyInterpolation defaultInterpolation;
    protected Joint physicsJoint;
    protected Collider col;
    protected InteractionController currentInteractor;

    protected virtual void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out col);
        defaultInterpolation = rb.interpolation;
    }

    public float GetInteractableHeight()
    {
        return col != null ? col.bounds.extents.y : 1.0f; 
    }

    /// <summary>
    /// Called when the player initiates an interaction with this object.
    /// </summary>
    /// <param name="controller">The player's interaction controller.</param>
    public virtual bool OnInteract(InteractionController controller)
    {
        if (currentInteractor == null)
            currentInteractor = controller;
        else
            return false;
        return true;
    }

    /// <summary>
    /// Called when the player ends their interaction with this object.
    /// </summary>
    /// <param name="controller">The player's interaction controller.</param>
    public virtual void OnInteractionEnd(InteractionController controller)
    {
        if (controller != null && controller.IsCurrentInteractingWithThis(this)) {
            controller.EndCurrentInteraction();
        }    
        currentInteractor = null;
    }
    
    /// <summary>
    /// Creates a physics joint between this object and the player controller.
    /// </summary>
    /// <param name="controller">The player's interaction controller to attach to.</param>
    protected virtual void AttachToController<T>(InteractionController controller) where T :Joint
    {
        physicsJoint = gameObject.AddComponent<T>();
        physicsJoint.connectedBody = controller.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Removes the physics joint between this object and the player.
    /// </summary>
    protected virtual void DetachFromController()
    {
        if (physicsJoint != null)
        {
            Destroy(physicsJoint);
            physicsJoint = null;
        }
    }
} 