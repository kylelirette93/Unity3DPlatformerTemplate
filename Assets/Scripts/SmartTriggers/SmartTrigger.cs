using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

/// <summary>
/// A flexible trigger system that can execute a sequence of actions based on various trigger conditions.
/// </summary>
[Flags]
public enum TriggerOptions
{
    None = 0,
    TriggerOnlyOnce = 1,        // The trigger will only execute its actions once
    TriggerWhenEnter = 2,       // Execute when objects enter the trigger
    TriggerWhenExit = 4,        // Execute when objects exit the trigger
    TriggerWhileInside = 8,     // Continuously execute while objects are inside
    UntriggerOtherwise = 16     // Execute untrigger actions when no valid objects are inside
}

/// <summary>
/// Main component for the Smart Trigger system. Handles collision detection and orchestrates the execution of trigger actions.
/// Place this component on a GameObject with a Collider (set to "Is Trigger").
/// </summary>
public class SmartTrigger : MonoBehaviour
{
    [Tooltip("Configure how and when the trigger should activate")]
    [SerializeField] private TriggerOptions options;

    [Tooltip("Which layers can activate this trigger")]
    [SerializeField] private LayerMask triggerLayers;

    [Tooltip("Which tags can activate this trigger (leave empty to accept any tag)")]
    [SerializeField] private string[] triggerTags;

    [Tooltip("Actions to execute when the trigger activates")]
    [SerializeField] private List<TriggerAction> onTriggerActions = new List<TriggerAction>();

    [Tooltip("Actions to execute when the trigger deactivates (only used with UntriggerOtherwise option)")]
    [SerializeField] private List<TriggerAction> onUntriggerActions = new List<TriggerAction>();

    private bool hasAlreadyTriggered;
    private HashSet<Collider> triggeredColliders = new HashSet<Collider>();

    protected bool hasAnyTriggerActions { get => onTriggerActions.Count > 0; }
    protected bool hasAnyUntriggerActions { get => onUntriggerActions.Count > 0; }

    public int GetTriggerListCount()
    {
        return onTriggerActions.Count;
    }
    public void SetTriggerListElement(int indx, TriggerAction newAction)
    {
        onTriggerActions[indx] = newAction;
    }
    public TriggerAction GetTriggerListElement(int indx)
    {
        return onTriggerActions[indx];
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTrigger(other)) return;
        
        triggeredColliders.Add(other);

        if ((options & TriggerOptions.TriggerWhenEnter) != 0)
        {
            if (options.HasFlag(TriggerOptions.TriggerOnlyOnce) && hasAlreadyTriggered) return;
            ExecuteTriggerActions();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidTrigger(other)) return;

        triggeredColliders.Remove(other);

        if ((options & TriggerOptions.TriggerWhenExit) != 0)
        {
            if (options.HasFlag(TriggerOptions.TriggerOnlyOnce) && hasAlreadyTriggered) return;
            ExecuteTriggerActions();
        }

        if (options.HasFlag(TriggerOptions.UntriggerOtherwise) && triggeredColliders.Count == 0)
        {
            ExecuteUntriggerActions();
        }
    }

    private void Update()
    {
        if ((options & TriggerOptions.TriggerWhileInside) != 0 && triggeredColliders.Count > 0)
        {
            if (options.HasFlag(TriggerOptions.TriggerOnlyOnce) && hasAlreadyTriggered) return;
            ExecuteTriggerActions();
        }
    }

    /// <summary>
    /// Checks if a collider meets the layer and tag requirements to activate this trigger.
    /// </summary>
    protected bool IsValidTrigger(Collider other)
    {
        // Check layer
        if ((triggerLayers.value & (1 << other.gameObject.layer)) == 0)
            return false;

        // Check tags
        if (triggerTags != null && triggerTags.Length > 0)
        {
            bool hasValidTag = false;
            foreach (string tag in triggerTags)
            {
                if (other.CompareTag(tag))
                {
                    hasValidTag = true;
                    break;
                }
            }
            if (!hasValidTag) return false;
        }

        return true;
    }

    protected void ExecuteTriggerActions()
    {
        StartCoroutine(ExecuteActionsRoutine(onTriggerActions));
        hasAlreadyTriggered = true;
    }

    protected void ExecuteUntriggerActions()
    {
        StartCoroutine(ExecuteActionsRoutine(onUntriggerActions));
    }

    /// <summary>
    /// Executes the trigger actions in sequence, respecting parallel execution flags.
    /// </summary>
    private IEnumerator ExecuteActionsRoutine(List<TriggerAction> actions)
    {
        List<TriggerAction> runningActions = new List<TriggerAction>();
        
        foreach (var action in actions)
        {
            if (action == null) continue;

            // If previous actions aren't running in parallel, wait for them to complete
            if (runningActions.Count > 0 && !action.RunInParallel)
            {
                while (runningActions.Any(a => !a.IsComplete))
                {
                    yield return null;
                }
                runningActions.Clear();
            }
            
            action.Execute();
            runningActions.Add(action);
            
            // If this action runs in parallel, continue immediately to the next action
            if (action.RunInParallel)
            {
                continue;
            }
        }
        
        // Wait for any remaining actions to complete
        while (runningActions.Any(a => !a.IsComplete))
        {
            yield return null;
        }
    }
}
