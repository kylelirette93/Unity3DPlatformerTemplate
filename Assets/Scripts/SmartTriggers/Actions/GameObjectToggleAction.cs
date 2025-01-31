using UnityEngine;
using System;

[Serializable]
public class GameObjectToggleAction : TriggerAction
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private bool setActive = true;
    
    protected override void OnExecute()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(setActive);
        }
        Complete();
    }
} 