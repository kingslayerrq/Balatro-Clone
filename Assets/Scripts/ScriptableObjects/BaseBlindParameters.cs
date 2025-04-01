using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseBlindParameters", menuName = "Scriptable Objects/BaseBlindParameters")]
public class BaseBlindParameters : ScriptableObject
{
    public float baseChipGoal;
    
    public int reward;
    
    [Tooltip("Actions upon selecting this blind")]
    public List<BlindActionConfig> actionConfigsOnEnter = new List<BlindActionConfig>();
    
    [Tooltip("Actions upon played cards")]
    public List<BlindActionConfig> blindActionConfigs = new List<BlindActionConfig>();

    [Tooltip("Actions upon beating this blind")]
    public List<BlindActionConfig> actionConfigsOnWin = new List<BlindActionConfig>();
}
