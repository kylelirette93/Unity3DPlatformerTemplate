using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerController))]
/// <summary>
/// Manages player interactions with Interactable objects in the game world.
/// Handles detection of interactable objects and mediates the interaction process.
/// </summary>
public class InteractionController : MonoBehaviour 
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionZone;
    [SerializeField] private float interactionRadius = 0.5f;
    
    [Header("Throwing Settings")]
    [SerializeField] private Vector3 throwForce = new Vector3(0, 5, 7);
    
    [Header("Audio")]
    [SerializeField] private AudioClip interactSound;
    [SerializeField] private AudioClip releaseSound;
    
    [Header("Animation")]
    [SerializeField] private int armsAnimationLayer;
    
    private Animator animator;
    private PlayerController playerController;
    private MovementController movementController;
    private Collider playerCollider;
    private AudioSource audioSource;
    private float defaultRotationSpeed;
    
    private Interactable currentInteractable;
    private FixedJoint physicsJoint;
    private float lastInteractionTime;
    
    private List<Interactable> interactablesInRange = new List<Interactable>();
    
    public Vector3 ThrowForce => throwForce;

    public bool IsCurrentInteractingWithThis(Interactable otherInteractable)
    {
        return currentInteractable == otherInteractable;
    }

    private void Awake()
    {
        SetupComponents();
        SetupInteractionZone();
    }

    private void SetupComponents()
    {
        TryGetComponent(out playerCollider);
        TryGetComponent(out playerController);
        TryGetComponent(out audioSource);
        TryGetComponent(out movementController);
        animator = GetComponentInChildren<Animator>();
        
        if (animator)
            animator.SetLayerWeight(armsAnimationLayer, 1);
            
        if (movementController)
            defaultRotationSpeed = movementController.rotationSpeed;
        
        if (TryGetComponent(out HealthController health))
            health.OnDeath.AddListener(EndCurrentInteraction);
    }

    private void SetupInteractionZone()
    {
        if (!interactionZone)
        {
            interactionZone = new GameObject("InteractionZone");
            var boxCollider = interactionZone.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            interactionZone.transform.SetParent(transform);
            interactionZone.transform.localPosition = new Vector3(0f, 0f, 0.5f);
        }
    }

    /// <summary>
    /// Called when the player presses the interact button.
    /// Either begins a new interaction or ends the current one.
    /// </summary>
    public void OnInteract()
    {
        if (currentInteractable != null && Time.time > lastInteractionTime + 0.1f)
        {
            EndCurrentInteraction();
        }
        else if (interactablesInRange.Count > 0 && Time.time > lastInteractionTime + 0.2f)
        {
            BeginInteraction(interactablesInRange[0]);
        }
    }

    private void BeginInteraction(Interactable interactable)
    {
        if (!CanInteract(interactable)) return;
        
        currentInteractable = interactable;
        PlaySound(interactSound);
        interactable.OnInteract(this);
        lastInteractionTime = Time.time;
        
        UpdateAnimator();
    }

    public void EndCurrentInteraction()
    {
        if (currentInteractable == null) return;

        var endingInteraction = currentInteractable;
        currentInteractable = null;
        
        PlaySound(releaseSound);
        endingInteraction.OnInteractionEnd(this);
        lastInteractionTime = Time.time;
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (!animator) return;
        
        animator.SetBool("HoldingPickup", currentInteractable is PickupInteractable);
        animator.SetBool("HoldingPushable", currentInteractable is PushableInteractable);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Interactable>(out Interactable interactable) 
            && !interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Interactable>(out Interactable interactable))
        {
            interactablesInRange.Remove(interactable);
        }
    }

    /// <summary>
    /// Checks if an interaction can be started with the given interactable.
    /// For pickups, checks if there's enough space above the player's head.
    /// </summary>
    private bool CanInteract(Interactable interactable)
    {
        if (interactable is PickupInteractable)
        {
            Vector3 holdPosition = transform.position.SetY(transform.position.y + interactable.GetInteractableHeight() + playerCollider.bounds.extents.y);
            return !Physics.CheckSphere(holdPosition, interactionRadius, GameManager.Instance.groundMask);
        }
        return true;
    }
} 