using UnityEngine;

/// <summary>
/// Represents a level completion trigger that transitions to the next scene.
/// Attach to the level's end goal area.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class Goal : MonoBehaviour 
{
	[Header("Goal Effects")]
	[Tooltip("Upward force applied to player while in the goal area")]
	public float upwardsForce = 28f;
	
	[Header("Level Transition")]
	[Tooltip("Time player must remain in goal area before transitioning")]
	public float transitionDelay = 1.0f;
	[Tooltip("Name of the scene to load when goal is reached")]
	public string nextSceneName;
	
	private float timeInGoalArea;

	private bool alreadyInTransition = false;
	
	/// <summary>
	/// Ensures the collider is set up as a trigger
	/// </summary>
	void Awake()
	{
		GetComponent<Collider>().isTrigger = true;
		
		if (string.IsNullOrEmpty(nextSceneName))
		{
			Debug.LogWarning($"No next scene specified in {gameObject.name}'s Goal component!", this);
		}
	}
	
	/// <summary>
	/// Applies upwards force and handles level completion when player is in the goal area
	/// </summary>
	void OnTriggerStay(Collider other)
	{
		if (!other.CompareTag("Player") || alreadyInTransition) return;
		
		// Apply upward force if object has physics
		if (other.TryGetComponent<Rigidbody>(out var rigidbody))
		{
			rigidbody.AddForce(Vector3.up * upwardsForce, ForceMode.Force);
		}
		
		// Track time in goal area and transition when ready
		timeInGoalArea += Time.deltaTime;
		if (timeInGoalArea >= transitionDelay)
		{
			CompleteLevel();
		}
	}
	
	/// <summary>
	/// Resets the goal area timer when player exits
	/// </summary>
	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			timeInGoalArea = 0f;
		}
	}
	
	/// <summary>
	/// Handles the level completion and scene transition
	/// </summary>
	private void CompleteLevel()
	{
		alreadyInTransition = true;

        if (string.IsNullOrEmpty(nextSceneName))
		{
			Debug.LogError("Cannot complete level: No next scene specified!", this);
			return;
		}
		
		SceneTransitioner.LoadScene(nextSceneName);
	}
}