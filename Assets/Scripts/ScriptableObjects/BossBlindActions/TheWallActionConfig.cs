using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossBlindAction", menuName = "Scriptable Objects/BossBlindAction")]
public class TheWallActionConfig : BlindActionConfig
{
    public override void Execute(Round round, RoundManager.State state)
    {
        round.blind.baseChipGoalMultiplier += 10000;
        Debug.LogWarning("The wall is doing something!");
    }

    
}
