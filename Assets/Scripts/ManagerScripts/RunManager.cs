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

    public HandTypeConfig.HandType GetRunHandTypeInfo(Enums.BasePokerHandType handType)
    {
        switch (handType)
        {
            case Enums.BasePokerHandType.None:
                return None;
            case Enums.BasePokerHandType.HighCard:
                return HighCard;
            case Enums.BasePokerHandType.Pair:
                return Pair;
            case Enums.BasePokerHandType.TwoPair:
                return TwoPair;
            case Enums.BasePokerHandType.ThreeOfAKind:
                return ThreeOfAKind;
            case Enums.BasePokerHandType.Straight:
                return Straight;
            case Enums.BasePokerHandType.Flush:
                return Flush;
            case Enums.BasePokerHandType.FullHouse:
                return FullHouse;
            case Enums.BasePokerHandType.FourOfAKind:
                return FourOfAKind;
            case Enums.BasePokerHandType.StraightFlush:
                return StraightFlush;
            case Enums.BasePokerHandType.RoyalFlush:
                return RoyalFlush;
            case Enums.BasePokerHandType.FiveOfAKind:
                return FiveOfAKind;
            case Enums.BasePokerHandType.FlushHouse:
                return FlushHouse;
            case Enums.BasePokerHandType.FlushFive:
                return FlushFive;
            default:
                return None;
        }
    }
    
    
}
