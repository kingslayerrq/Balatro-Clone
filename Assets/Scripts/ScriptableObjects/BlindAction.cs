using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlindAction", menuName = "Scriptable Objects/BlindAction")]
public class BlindActionConfig : BaseActionConfig
{
    public override void Execute(Card card, List<Card> allCards, ScoreCalculator calculator)
    {
        Debug.LogWarning("Doing nothing");
        return;
    }

    public override BaseAction Create()
    {
        return new BlindAction
        {
            actionName = actionName,
            description = description,
            executionOrder = executionOrder,
            config = this
        };
    }

    public class BlindAction : BaseAction
    {
        
    }
}
