using UnityEngine;

public class AnimatorTriggerAction : TriggerAction
{
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string triggerName;
    [SerializeField] private bool statusToSet;
    [SerializeField] private bool setAsTrigger;


    protected override void OnExecute()
    {
        if (targetAnimator != null) {
            if (setAsTrigger)
            {
                if (statusToSet)
                {
                    targetAnimator.SetTrigger(triggerName);
                } else
                {
                    targetAnimator.ResetTrigger(triggerName);
                }
            } else
            {
                targetAnimator.SetBool(triggerName, statusToSet);
            }
        }
        Complete();
    }
} 