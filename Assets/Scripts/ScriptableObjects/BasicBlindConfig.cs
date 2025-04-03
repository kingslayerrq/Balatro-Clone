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
            type = type,
            baseChipGoal = baseChipGoal,
            reward = reward,
            config = this,
        };
    }

    public class BasicBlind : BaseBlind
    {
        
    }
}
