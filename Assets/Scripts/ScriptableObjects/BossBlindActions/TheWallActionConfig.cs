using UnityEngine;

[CreateAssetMenu(fileName = "TheWallAction", menuName = "Scriptable Objects/Boss Blind Actions/The Wall")]
public class TheWallActionConfig : BlindActionConfig
{
    public override void Execute(Round round, RoundManager.State state)
    {
        // The Wall's extra-large goal comes from the blind's own baseChipGoalMultiplier (4x).
        // Blind actions are now executed by RoundManager, so the old placeholder
        // (+10000 to the multiplier) was removed: it would make the blind unbeatable.
        Debug.Log($"{round.blind.blindName} entered ({state})");
    }

    
}
