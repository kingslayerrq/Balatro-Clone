using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "BaseCardParameters", menuName = "Scriptable Objects/BaseCardParameters")]
public class BaseCardParameters : ScriptableObject
{
    [Tooltip("Name of the card. Eg: Ace of Spades")]
    public string cardID;
    
    public Enums.CardSuit suit;
    
    [Tooltip("Rank of the card")]
    [Range(1, 14)]
    public int rank;
    
    public Enums.Edition edition = Enums.Edition.Regular;

    public Enums.Seal seal = Enums.Seal.None;
    
    [Tooltip("Actual chips the card provides")]
    public float baseChip;
    
    public float baseMult = 1f;

    public Sprite sprite;
    
    [Tooltip("How many times the card gets triggered in a poker hand. Default is 1")]
    public int triggerCounts = 1;

    public List<CardActionConfig> cardActionConfigs = new List<CardActionConfig>();


}
