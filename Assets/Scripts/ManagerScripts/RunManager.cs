using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;
    private AnteManager _anteManager;
    private DeckPanel _deckPanel;
    
    public int CurrentSeed { get; private set; }

    private int _curAnteLvl = 0;
    private int _curRoundLvl = 0;
    private int _hands;
    private int _discards;
    private int _money;
    private int _handSize = 8;
    public int AnteLvlReqToWin = 8;
    
    public AnteManager.Ante CurAnte { get; private set; }
    public int MinMoney 
    {
        get;
        private set;
    }
    public int CurAnteLvl
    {
        get => _curAnteLvl;
        private set => _curAnteLvl = Mathf.Max(0, value);
    }
    public int HandSize
    {
        get => _handSize;
        set => _handSize = Mathf.Max(0, _handSize);
    }
    public int Hands
    {
        get => _hands;
        set => _hands = Mathf.Max(0, value);
    }
    public int Discards
    {
        get => _discards;
        set => _discards = Mathf.Max(0, value);
    }
    public int Money
    {
        get => _money;
        set => _money = Mathf.Max(MinMoney, value);
    }
    public int CurRoundLvl
    {
        get => _curRoundLvl;
        set => _curRoundLvl = Mathf.Max(_curRoundLvl, value);
    }
    

    [Tooltip("Starting Deck Config")]
    public DeckParameters DeckConfig = null;
    public DeckParameters.Deck Deck;
    
    public GameObject CardSlotPrefab;
    public GameObject CardPrefab;

    [SerializeField] private List<BossBlindConfig> bossList = new List<BossBlindConfig>();
    [Tooltip("Checklist for Boss, bool - whether this boss has been encountered this run")]
    private Dictionary<BossBlindConfig, bool> _bossDictionary = new Dictionary<BossBlindConfig, bool>();
    public List<AnteManager.Ante> Antes = new List<AnteManager.Ante>();
    [Tooltip("Deck generated from starting deck config, can be modified throughout this run")]
    private List<Card> _cardsDeck = new List<Card>();
    [Tooltip("Cards for this run")] public IReadOnlyList<Card> CardsDeckRun => _cardsDeck;

    [HideInInspector] public UnityEvent<AnteManager.Ante> StartAnteEvent = new UnityEvent<AnteManager.Ante>();

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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        
    }

    private void Start()
    {
        _deckPanel = DeckPanel.Instance;
        _anteManager = AnteManager.Instance;
        
    }

    /// <summary>
    /// Initialize a Run
    /// </summary>
    public void Init()
    {
        SetRandomSeed();
        Debug.Log($"Seed for this run {CurrentSeed}");
        // Load Base HandType values
        LoadBaseHandTypes();
        // Load Deck
        LoadDeck();
        // Shuffle Boss
        RandomizeBossList();
        // Create Ante
        CreateAnte();
    }

    private void CreateAnte()
    {
        if (!_anteManager) return;
        if (_bossDictionary.Count < 1) return;

        _curAnteLvl += 1;

        var bossItem = _bossDictionary.FirstOrDefault(item => item.Value == false);
        // Refresh all bosses
        if (bossItem.Key == null)
        {
            foreach (var item in _bossDictionary.Keys)
            {
                _bossDictionary[item] = false;
            }
            bossItem = _bossDictionary.FirstOrDefault(item => item.Value == false);
        }

        var ante = _anteManager.Create(_curAnteLvl, bossItem.Key);
        
        Antes.Add(ante);
        CurAnte = ante;
        StartAnteEvent?.Invoke(CurAnte);
    }

    /// <summary>
    /// Shuffle boss and add to dictionary
    /// </summary>
    private void RandomizeBossList()
    {
        if (bossList is { Count: > 1 })
        {
            bossList = bossList.OrderBy(x => Random.value).ToList();
        }

        foreach (var boss in bossList)
        {
            _bossDictionary.Add(boss, false);
        }
    }
    private void LoadDeck()
    {
        if (DeckConfig == null) return;
        Deck = DeckConfig.Create();
        _hands = Deck.hands;
        _discards = Deck.discards;
        _money = Deck.money;
        
        var cardsConfig = Deck.cardsConfig;
        foreach (var cardConfig in cardsConfig)
        {
            var cardSlot = Instantiate(CardSlotPrefab, _deckPanel.transform);
            var cardObj = Instantiate(CardPrefab, cardSlot.transform);
            var card = cardObj.GetComponent<Card>();

 
            card.baseCardParameters = cardConfig;
            card.Init();
            
            
            // Add card
            _cardsDeck.Add(card);
            
            // Update Card State
            Card.cardStatusUpdateEvent?.Invoke(card, CardState.CardStatus.InCreation);
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
    
    
    private void SetSeed(int seed)
    {
        CurrentSeed = seed;
        Random.InitState(seed);
    }
    
    private void SetRandomSeed()
    {
        int seed = System.Environment.TickCount;
        SetSeed(seed);
    }
    
}
