using UnityEngine;

/// <summary>
/// Represents an object that can be picked up and thrown by the player.
/// The object will be held above the player's head and can be thrown in the direction they're facing.
/// </summary>
public class PickupInteractable : Interactable
{
    [Tooltip("How much lighter the object becomes when held (0.3 means 30% of original mass)")]
    [SerializeField] private float weightMultiplierWhenHeld = 0.3f;
    
    [Tooltip("Additional height above the player's head to hold the object")]
    [SerializeField] private float holdHeight = 0.5f;


    private void OnJointBreak(float breakForce)
    {
        Debug.Log("JOINT BROKE!");
        if (currentInteractor != null)
            currentInteractor.EndCurrentInteraction();
    }

    public override bool OnInteract(InteractionController controller)
    {
        if (!base.OnInteract(controller)) return false;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.mass *= weightMultiplierWhenHeld;
        
        Vector3 holdPosition = CalculateHoldPosition(controller);
        Vector3 previousPos = transform.position;
        transform.position = holdPosition;
        transform.rotation = controller.transform.rotation;
        
        AttachToController<FixedJoint>(controller);
        transform.position = previousPos;
        
        return true;
    }

    public override void OnInteractionEnd(InteractionController controller)
    {
        if (currentInteractor == null) return;

        DetachFromController();
        rb.interpolation = defaultInterpolation;
        rb.mass /= weightMultiplierWhenHeld;
        
        rb.AddRelativeForce(controller.ThrowForce, ForceMode.VelocityChange);

        base.OnInteractionEnd(controller);
    }

    /// <summary>
    /// Calculates the position where the object should be held above the player's head.
    /// Takes into account the player's height and the object's size.
    /// </summary>
    /// <param name="controller">The player's interaction controller.</param>
    /// <returns>The world position where the object should be held.</returns>
    private Vector3 CalculateHoldPosition(InteractionController controller)
    {
        Vector3 holdPosition = controller.transform.position;
        if (TryGetComponent<MeshFilter>(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
        {
            var playerCollider = controller.GetComponent<Collider>();
            holdPosition.y += playerCollider.bounds.extents.y + meshFilter.sharedMesh.bounds.extents.y + holdHeight;
        }
        return holdPosition;
    }
} 