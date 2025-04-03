using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossBlindConfig", menuName = "Scriptable Objects/BossBlindConfig")]
public class BossBlindConfig : BaseBlindParameters
{
    
    [Tooltip("Actions upon selecting this blind")]
    public List<BlindActionConfig> actionConfigsOnEnter = new List<BlindActionConfig>();
    
    [Tooltip("Actions upon played cards")]
    public List<BlindActionConfig> blindActionConfigs = new List<BlindActionConfig>();

    [Tooltip("Actions upon beating this blind")]
    public List<BlindActionConfig> actionConfigsOnWin = new List<BlindActionConfig>();

    public override BaseBlind Create()
    {
        return new BossBlind
        {
            blindName = blindName,
            description = description,
            baseChipGoal = baseChipGoal,
            reward = reward,
            config = this,
            actionConfigsOnEnter = actionConfigsOnEnter,
            blindActionConfigs = blindActionConfigs,
            actionConfigsOnWin = actionConfigsOnWin,
        };
    }
    
    public class BossBlind : BaseBlind
    {
        public List<BlindActionConfig> actionConfigsOnEnter = new List<BlindActionConfig>();
        
        public List<BlindActionConfig> blindActionConfigs = new List<BlindActionConfig>();
        
        public List<BlindActionConfig> actionConfigsOnWin = new List<BlindActionConfig>();

    }
}
