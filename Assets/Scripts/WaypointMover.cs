using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Moves an object along a sequence of waypoints with various movement patterns.
/// Uses kinematic rigidbody movement, making it suitable for moving platforms that can carry other objects.
/// </summary>
/// <remarks>
/// Setup instructions:
/// 1. Add empty GameObjects as children and tag them as "Waypoint"
/// 2. Position the waypoints in the order you want them to be followed
/// 3. Choose a movement pattern (Once, Loop, or PingPong)
/// 4. Tag the platform as "Moving Platform" if players should move with it
/// </remarks>
[RequireComponent(typeof(Rigidbody))]
public class WaypointMover : MonoBehaviour 
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed between waypoints in units per second")]
    [Min(0)]
    public float moveSpeed = 5f;

    [Tooltip("Time to wait at each waypoint before moving to the next")]
    [Min(0)]
    public float waitTime = 1f;

    [Tooltip("How the object should move through the waypoint sequence")]
    public MovementPattern pattern;
    
    /// <summary>
    /// Defines how the object moves through its waypoints
    /// </summary>
    public enum MovementPattern 
    { 
        Once,       // Move through waypoints once and stop at the end
        Loop,       // Continuously loop through waypoints from start to end
        PingPong    // Move back and forth through waypoints
    }
    
    // Internal state tracking
    private List<Transform> waypoints = new List<Transform>();
    private Rigidbody rb;
    private int currentWaypointIndex;
    private float waitUntilTime;
    private bool isMovingForward = true;
    private bool isWaiting;
    
    /// <summary>
    /// Initializes the platform's movement components and waypoints
    /// </summary>
    void Awake()
    {
        SetupRigidbody();
        CollectWaypoints();
    }
    
    /// <summary>
    /// Sets up the rigidbody for kinematic platform movement.
    /// Kinematic rigidbodies can move and affect other objects without being affected by physics themselves.
    /// </summary>
    private void SetupRigidbody()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;        // Moves by script, not physics
        rb.useGravity = false;        // Ignore gravity
        rb.interpolation = RigidbodyInterpolation.Interpolate;  // Smoother movement
    }
    
    /// <summary>
    /// Finds and stores all waypoint children, then detaches them so they don't move with the platform
    /// </summary>
    private void CollectWaypoints()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Waypoint")) {
                waypoints.Add(child);
            }
        }
        
        if (waypoints.Count == 0)
        {
            Debug.LogError($"No waypoints found on {gameObject.name}. Add child objects with 'Waypoint' tag.", this);
            enabled = false;
            return;
        }
        
        // Detach waypoints so they stay in place while platform moves
        transform.DetachChildren();
    }
    
    /// <summary>
    /// Handles physics-based movement updates
    /// </summary>
    void FixedUpdate()
    {
        if (waypoints.Count == 0 || isWaiting) return;
        
        MoveToCurrentWaypoint();
    }
    
    /// <summary>
    /// Handles non-physics updates like waypoint waiting time
    /// </summary>
    void Update()
    {
        if (isWaiting)
        {
            HandleWaiting();
        }
    }
    
    /// <summary>
    /// Moves the platform towards the current waypoint using kinematic movement
    /// </summary>
    private void MoveToCurrentWaypoint()
    {
        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        Vector3 movement = targetPosition - rb.position;

        // Move using kinematic rigidbody for proper platform behavior
        Vector3 newPosition = rb.position + (movement.normalized * moveSpeed * Time.fixedDeltaTime);

        // Check if we've reached the waypoint
        if (movement.magnitude < 0.1f || (movement.magnitude < 0.35f && !newPosition.directionTo(targetPosition).areAligned(rb.position.directionTo(targetPosition),1.0f)))
        {
            BeginWaiting();
            return;
        }
        
        rb.MovePosition(newPosition);
    }
    
    /// <summary>
    /// Starts the waiting period at a waypoint
    /// </summary>
    private void BeginWaiting()
    {
        isWaiting = true;
        waitUntilTime = Time.time + waitTime;
    }
    
    /// <summary>
    /// Handles the waiting state at waypoints
    /// </summary>
    private void HandleWaiting()
    {
        if (Time.time >= waitUntilTime)
        {
            isWaiting = false;
            UpdateWaypointIndex();
        }
    }
    
    /// <summary>
    /// Updates the target waypoint based on the current movement pattern
    /// </summary>
    private void UpdateWaypointIndex()
    {
        switch (pattern)
        {
            case MovementPattern.Once:
                // Move to next waypoint or stop if we're at the end
                if (currentWaypointIndex < waypoints.Count - 1)
                {
                    currentWaypointIndex++;
                }
                else
                {
                    enabled = false;  // Stop moving when we reach the end
                }
                break;
                
            case MovementPattern.Loop:
                // Wrap around to start when reaching the end
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                break;
                
            case MovementPattern.PingPong:
                if (isMovingForward)
                {
                    // Moving forward until we hit the end
                    if (currentWaypointIndex >= waypoints.Count - 1)
                    {
                        isMovingForward = false;
                        currentWaypointIndex--;
                    }
                    else
                    {
                        currentWaypointIndex++;
                    }
                }
                else
                {
                    // Moving backward until we hit the start
                    if (currentWaypointIndex <= 0)
                    {
                        isMovingForward = true;
                        currentWaypointIndex++;
                    }
                    else
                    {
                        currentWaypointIndex--;
                    }
                }
                break;
        }
    }
    
    /// <summary>
    /// Draws waypoint visualization in the editor
    /// </summary>
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;
            
        Gizmos.color = Color.cyan;
        Transform firstWaypointFound = null;
        Transform currentWaypointProcessing = null;
        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);
            
            if (!child.CompareTag("Waypoint"))
                continue;
            // Draw waypoint markers
            Gizmos.DrawWireSphere(child.position, 0.7f);
            currentWaypointProcessing = child;
            if (firstWaypointFound == null)
                firstWaypointFound = child;
            // Draw lines connecting sequential waypoints
            if (child.GetSiblingIndex() <= 0)
                continue;

            Transform prevWaypoint = transform.GetChild(child.GetSiblingIndex() - 1);

            if (!prevWaypoint.CompareTag("Waypoint"))
                continue;

            Gizmos.DrawLine(prevWaypoint.position, child.position);
        }
        if (pattern == MovementPattern.Loop && firstWaypointFound != null && currentWaypointProcessing != null && currentWaypointProcessing != firstWaypointFound)
        {
            Gizmos.DrawLine(currentWaypointProcessing.position, firstWaypointFound.position);
        }
    }
} 