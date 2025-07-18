using UnityEngine;


[CreateAssetMenu(fileName = "BasicBlindConfig", menuName = "Scriptable Objects/BasicBlindConfig")]
public class BasicBlindConfig : BaseBlindParameters
{
    public override BaseBlind Create()
    {
        return new BasicBlind
        {
            blindName = blindName,
            description = description,
            blindSprites = blindSprites,
            blindColor = blindColor,
            type = type,
            baseChipGoalMultiplier = baseChipGoalMultiplier,
            reward = reward,
            config = this,
        };
    }

    public class BasicBlind : BaseBlind
    {
        
    }
}
