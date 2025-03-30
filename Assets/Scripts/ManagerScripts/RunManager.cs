using System;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    [Header("Poker Hand Type Instance References")]
    public HandTypeConfig.HandType HighCard = null;
    public HandTypeConfig.HandType Pair = null;
    public HandTypeConfig.HandType TwoPair = null;
    public HandTypeConfig.HandType ThreeOfAKind = null;
    public HandTypeConfig.HandType Straight = null;
    public HandTypeConfig.HandType Flush = null;
    public HandTypeConfig.HandType FullHouse = null;
    public HandTypeConfig.HandType FourOfAKind = null;
    public HandTypeConfig.HandType StraightFlush = null;
    public HandTypeConfig.HandType RoyalFlush = null;
    public HandTypeConfig.HandType FiveOfAKind = null;
    public HandTypeConfig.HandType FlushHouse = null;
    public HandTypeConfig.HandType FlushFive = null;
    public HandTypeConfig.HandType None = null;
    
    [Header("Poker Hand Type base configs")]
    [SerializeField] private HandTypeConfig highCardConfig;
    [SerializeField] private HandTypeConfig pairConfig;
    [SerializeField] private HandTypeConfig twoPairConfig;
    [SerializeField] private HandTypeConfig threeOfAKindConfig;
    [SerializeField] private HandTypeConfig straightConfig;
    [SerializeField] private HandTypeConfig flushConfig;
    [SerializeField] private HandTypeConfig fullHouseConfig;
    [SerializeField] private HandTypeConfig fourOfAKindConfig;
    [SerializeField] private HandTypeConfig straightFlushConfig;
    [SerializeField] private HandTypeConfig royalFlushConfig;
    [SerializeField] private HandTypeConfig fiveOfAKindConfig;
    [SerializeField] private HandTypeConfig flushHouseConfig;
    [SerializeField] private HandTypeConfig flushFiveConfig;
    [SerializeField] private HandTypeConfig noneConfig;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        // Load Base HandType
        LoadBaseHandTypes();
    }

    private void LoadBaseHandTypes()
    {
        HighCard = highCardConfig.Create();
        Pair = pairConfig.Create();
        TwoPair = twoPairConfig.Create();
        ThreeOfAKind = threeOfAKindConfig.Create();
        Straight = straightConfig.Create();
        Flush = flushConfig.Create();
        FullHouse = fullHouseConfig.Create();
        FourOfAKind = fourOfAKindConfig.Create();
        StraightFlush = straightFlushConfig.Create();
        RoyalFlush = royalFlushConfig.Create();
        FiveOfAKind = fiveOfAKindConfig.Create();
        FlushHouse = flushHouseConfig.Create();
        FlushFive = flushFiveConfig.Create();
        None = noneConfig.Create();
    }
    
    
}
