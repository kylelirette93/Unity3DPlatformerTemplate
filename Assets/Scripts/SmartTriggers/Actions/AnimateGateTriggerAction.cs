using UnityEngine;
using System;

[Serializable]
public class AnimateGateTriggerAction : TriggerAction
{
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private string parameterName = "IsOpen";
    [SerializeField] private bool value = true;

    protected override void OnExecute()
    {
        if (gateAnimator != null)
        {
            gateAnimator.SetBool(parameterName, value);
        }
        Complete();
    }
} 