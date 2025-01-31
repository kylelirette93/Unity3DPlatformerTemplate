using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class MultiActionTrigger : TriggerAction
{
    [SerializeField] private TriggerAction[] actions;
    [SerializeField] private float delayBetweenActions = 0f;
    
    protected override void OnExecute()
    {
        if (delayBetweenActions <= 0)
        {
            // Execute all immediately
            foreach (var action in actions)
            {
                if (action != null)
                    action.Execute();
            }
            Complete();
        }
        else
        {
            // Start a coroutine in the SmartTrigger
            var smartTrigger = GameObject.FindObjectOfType<SmartTrigger>();
            if (smartTrigger != null)
            {
                smartTrigger.StartCoroutine(ExecuteWithDelay());
            }
        }
    }

    private IEnumerator ExecuteWithDelay()
    {
        foreach (var action in actions)
        {
            if (action != null)
            {
                action.Execute();
                yield return new WaitForSeconds(delayBetweenActions);
            }
        }
        Complete();
    }
} 