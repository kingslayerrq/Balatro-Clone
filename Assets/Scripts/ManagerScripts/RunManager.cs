using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Owns everything that lasts for a whole run: the deck, money, hand type levels,
/// and the progression through blinds and antes.
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance;
    private AnteManager _anteManager;
    private RoundManager _roundManager;
    private DeckPanel _deckPanel;

    public int CurrentSeed { get; private set; }

    private int _curAnteLvl = 0;
    private int _curRoundLvl = 0;
    private int _hands;
    private int _discards;
    private int _money;
    private int _handSize = 8;
    public int AnteLvlReqToWin = 8;

    /// <summary>True between Init and the run being won or lost.</summary>
    public bool IsRunActive { get; private set; }
    public AnteManager.Ante CurAnte { get; private set; }
    public int MinMoney
    {
        get;
        private set;
    }
    public int CurAnteLvl
    {
        get => _curAnteLvl;
        private set => SetStat(ref _curAnteLvl, Mathf.Max(0, value));
    }
    public int HandSize
    {
        get => _handSize;
        set => SetStat(ref _handSize, Mathf.Max(0, value));
    }
    public int Hands
    {
        get => _hands;
        set => SetStat(ref _hands, Mathf.Max(0, value));
    }
    public int Discards
    {
        get => _discards;
        set => SetStat(ref _discards, Mathf.Max(0, value));
    }
    public int Money
    {
        get => _money;
        set => SetStat(ref _money, Mathf.Max(MinMoney, value));
    }
    public int CurRoundLvl
    {
        get => _curRoundLvl;
        set => SetStat(ref _curRoundLvl, Mathf.Max(_curRoundLvl, value));
    }


    [Tooltip("Starting Deck Config")]
    public DeckParameters DeckConfig = null;
    public DeckParameters.Deck Deck;

    public GameObject CardSlotPrefab;
    public GameObject CardPrefab;

    [SerializeField] private List<BossBlindConfig> bossList = new List<BossBlindConfig>();
    // Bosses not yet encountered this cycle; refilled (reshuffled) once every boss has been used
    private readonly Queue<BossBlindConfig> _bossQueue = new Queue<BossBlindConfig>();
    public List<AnteManager.Ante> Antes = new List<AnteManager.Ante>();
    // Deck generated from starting deck config, can be modified throughout this run
    private readonly List<Card> _cardsDeck = new List<Card>();
    /// <summary>Cards for this run</summary>
    public IReadOnlyList<Card> CardsDeckRun => _cardsDeck;

    /// <summary>A new ante started, or the blinds of the current ante should be shown again.</summary>
    public event Action<AnteManager.Ante> AnteDisplayRequested;
    /// <summary>Money, hands, discards, hand size, ante level or round level changed.</summary>
    public event Action RunStatsChanged;
    /// <summary>The run is over. bool: did the player win.</summary>
    public event Action<bool> RunEnded;

    #region HandTypeConfig

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

        // This run's copy of every hand type (levels up during the run), keyed by each config's own type
        private readonly Dictionary<Enums.BasePokerHandType, HandTypeConfig.HandType> _handTypes =
            new Dictionary<Enums.BasePokerHandType, HandTypeConfig.HandType>();

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
        _roundManager = RoundManager.Instance;

        if (!_deckPanel || !_anteManager || !_roundManager)
        {
            Debug.LogError("RunManager: DeckPanel, AnteManager or RoundManager is missing from the scene", this);
            return;
        }

        _roundManager.RoundEnded += OnRoundEnded;
    }

    private void OnDestroy()
    {
        if (_roundManager) _roundManager.RoundEnded -= OnRoundEnded;
        if (Instance == this) Instance = null;
    }

    private void SetStat(ref int field, int value)
    {
        if (field == value) return;
        field = value;
        RunStatsChanged?.Invoke();
    }

    /// <summary>
    /// Start a new run. Ignored while a run is in progress.
    /// </summary>
    public void Init()
    {
        if (IsRunActive) return;

        ResetRun();
        IsRunActive = true;

        SetRandomSeed();
        Debug.Log($"Seed for this run {CurrentSeed}");
        // Load Base HandType values
        LoadBaseHandTypes();
        // Load Deck
        LoadDeck();
        // Shuffle Boss
        RandomizeBossList();
        RunStatsChanged?.Invoke();
        // Create Ante
        CreateAnte();
    }

    /// <summary>
    /// Clear everything left over from a previous run
    /// </summary>
    private void ResetRun()
    {
        foreach (var card in _cardsDeck)
        {
            if (!card) continue;
            if (card.curPanel) card.curPanel.Remove(card);
            if (card.cardVisuals) Destroy(card.cardVisuals.gameObject);
            // the slot holds the card
            Destroy(card.transform.parent.gameObject);
        }
        _cardsDeck.Clear();
        _handTypes.Clear();
        _bossQueue.Clear();
        Antes.Clear();
        CurAnte = null;
        _curAnteLvl = 0;
        _curRoundLvl = 0;
    }

    #region Ante progression

    private void CreateAnte()
    {
        if (!_anteManager) return;
        if (bossList == null || bossList.Count < 1)
        {
            Debug.LogError("RunManager: bossList is empty, cannot create an ante", this);
            return;
        }

        // Every boss has been encountered -> start a new shuffled cycle
        if (_bossQueue.Count == 0) RandomizeBossList();

        var ante = _anteManager.Create(_curAnteLvl + 1, _bossQueue.Dequeue());
        if (ante == null) return;

        Antes.Add(ante);
        CurAnte = ante;
        CurAnteLvl += 1;
        AnteDisplayRequested?.Invoke(CurAnte);
    }

    private void OnRoundEnded(Round round, bool won)
    {
        if (!IsRunActive) return;

        if (!won)
        {
            EndRun(false);
            return;
        }

        AdvanceAnte();
    }

    /// <summary>
    /// Mark a blind as skipped and move on to the next one
    /// </summary>
    public void SkipRound(Round round)
    {
        if (!IsRunActive || round == null || round.isComplete) return;
        round.isSkipped = true;
        AdvanceAnte();
    }

    /// <summary>
    /// Show the remaining blinds of the ante, or move to the next ante once all are done
    /// </summary>
    private void AdvanceAnte()
    {
        bool anteCleared = CurAnte.rounds.All(r => r.isComplete || r.isSkipped);
        if (!anteCleared)
        {
            AnteDisplayRequested?.Invoke(CurAnte);
            return;
        }

        if (_curAnteLvl >= AnteLvlReqToWin)
        {
            EndRun(true);
            return;
        }

        CreateAnte();
    }

    private void EndRun(bool won)
    {
        IsRunActive = false;
        Debug.Log(won ? "Run won" : "Run lost");
        RunEnded?.Invoke(won);
    }

    /// <summary>
    /// Shuffle bosses into the queue
    /// </summary>
    private void RandomizeBossList()
    {
        _bossQueue.Clear();
        if (bossList == null) return;

        foreach (var boss in bossList.OrderBy(x => Random.value))
        {
            _bossQueue.Enqueue(boss);
        }
    }

    #endregion

    private void LoadDeck()
    {
        if (DeckConfig == null)
        {
            Debug.LogError("RunManager: DeckConfig is not assigned", this);
            return;
        }
        Deck = DeckConfig.Create();
        _hands = Deck.hands;
        _discards = Deck.discards;
        _money = Deck.money;

        // every card visual lives under one object so visuals can overlap freely between panels
        var visualizer = FindFirstObjectByType<CardVisualizer>();
        var visualsRoot = visualizer ? visualizer.transform : null;

        foreach (var cardConfig in Deck.cardsConfig)
        {
            var cardSlot = Instantiate(CardSlotPrefab, _deckPanel.transform);
            var cardObj = Instantiate(CardPrefab, cardSlot.transform);
            var card = cardObj.GetComponent<Card>();

            card.baseCardParameters = cardConfig;
            card.Init(visualsRoot);
            _cardsDeck.Add(card);
            card.MoveTo(_deckPanel, CardState.CardStatus.InCreation);
        }
    }

    private void LoadBaseHandTypes()
    {
        _handTypes.Clear();
        var configs = new[]
        {
            noneConfig, highCardConfig, pairConfig, twoPairConfig, threeOfAKindConfig, straightConfig, flushConfig,
            fullHouseConfig, fourOfAKindConfig, straightFlushConfig, royalFlushConfig, fiveOfAKindConfig,
            flushHouseConfig, flushFiveConfig
        };

        foreach (var config in configs)
        {
            if (config == null)
            {
                Debug.LogError("RunManager: a hand type config is not assigned", this);
                continue;
            }
            _handTypes[config.type] = config.Create();
        }
    }

    /// <summary>
    /// This run's (levelled) version of a hand type
    /// </summary>
    public HandTypeConfig.HandType GetRunHandTypeInfo(Enums.BasePokerHandType handType)
    {
        if (_handTypes.TryGetValue(handType, out var info)) return info;
        return _handTypes[Enums.BasePokerHandType.None];
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
