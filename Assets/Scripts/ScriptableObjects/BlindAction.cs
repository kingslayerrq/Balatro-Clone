using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicBlindAction", menuName = "Scriptable Objects/BasicBlindAction")]
public class BlindActionConfig : BaseBlindActionParameters
{
    public override void Execute(Round round, RoundManager.State state)
    {
        Debug.LogWarning("Doing nothing as blind");
    }

    public override BaseBlindAction Create()
    {
        return new BlindAction
        {
            actionName = actionName,
            description = description,
            executionOrder = executionOrder,
            config = this
        };
    }

    public class BlindAction : BaseBlindAction
    {
        
    }
}
