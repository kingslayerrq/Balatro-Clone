using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Random = UnityEngine.Random;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;
    private AnteManager _anteManager;
    private DeckPanel _deckPanel;
    
    public int curAnteLvl = 1;
    public int handSize = 8;
    public int hands;
    public int discards;
    public int money = 0;
    public int round = 1;

    [Tooltip("Starting Deck Config")]
    public DeckParameters deckConfig = null;
    public DeckParameters.Deck deck;
    
    public GameObject cardSlotPrefab;
    public GameObject cardPrefab;

    [SerializeField] private List<BossBlindConfig> bossList = new List<BossBlindConfig>();
    public List<AnteManager.Ante> antes = new List<AnteManager.Ante>();
    private List<Card> _cardsDeck = new List<Card>();
    [Tooltip("Cards for this run")]
    public IReadOnlyList<Card> CardsDeckRun => _cardsDeck;

    #region HandTypeConfig && Reference
    
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

    #endregion
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        
    }

    private void Start()
    {
        _deckPanel = DeckPanel.Instance;
        _anteManager = AnteManager.Instance;
        Init();
    }

    private void Init()
    {
        // Load Base HandType values
        LoadBaseHandTypes();
        // Load Deck
        LoadDeck();
        // Shuffle Boss
        RandomizeBossList();
        // Init Ante
        InitAnte(curAnteLvl);
    }

    private void InitAnte(int lvl)
    {
        if (!_anteManager) return;
        if (bossList == null) return;
        
        var boss = bossList[curAnteLvl - 1];
        antes.Add(_anteManager.Create(lvl, boss));
        bossList.Remove(boss);
    }

    // Shuffle boss
    private void RandomizeBossList()
    {
        if (bossList is { Count: > 1 })
        {
            bossList = bossList.OrderBy(x => Random.value).ToList();
        }
    }
    private void LoadDeck()
    {
        if (deckConfig == null) return;
        deck = deckConfig.Create();
        hands = deck.hands;
        discards = deck.discards;
        money = deck.money;
        
        var cardsConfig = deck.cardsConfig;
        foreach (var cardConfig in cardsConfig)
        {
            var cardSlot = Instantiate(cardSlotPrefab, _deckPanel.transform);
            var cardObj = Instantiate(cardPrefab, cardSlot.transform);
            var card = cardObj.GetComponent<Card>();
            card.baseCardParameters = cardConfig;
            card.Init();
            // Update Card State
            Card.cardStatusUpdateEvent?.Invoke(card, CardState.CardStatus.InCreation);
            
            // Add card
            _cardsDeck.Add(card);
        }
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
