using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class DelayAction : TriggerAction
{
    [SerializeField] private float delay = 1f;
    
    private Coroutine delayCoroutine;
    
    protected override void OnExecute()
    {
        var smartTrigger = GameObject.FindObjectOfType<SmartTrigger>();
        if (smartTrigger != null)
        {
            delayCoroutine = smartTrigger.StartCoroutine(DelayRoutine());
        }
    }
    
    private IEnumerator DelayRoutine()
    {
        yield return new WaitForSeconds(delay);
        Complete();
    }
} 