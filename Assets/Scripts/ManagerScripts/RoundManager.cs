using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Runs a round (one blind) from start to finish in a single coroutine.
/// It is the only class that changes the round state; everything else listens to
/// <see cref="StateChanged"/> and asks for player actions through RequestPlay / RequestDiscard.
/// </summary>
public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;
    private RunManager _runManager;
    private ScoreCalculator _scoreCalculator;
    private HandPanel _handPanel;
    private UsedPanel _usedPanel;
    private DrawPanel _drawPanel;
    private DeckPanel _deckPanel;
    private PlayedCardPanel _playedCardPanel;

    [SerializeField] private State state = State.None;
    public State curState => state;
    [Tooltip("Scored by this hand")]
    [SerializeField] private double handScore = 0;
    public Round curRound = null;
    /// <summary>True from StartRound until the round is won or lost.</summary>
    public bool IsRoundActive { get; private set; }


    [Header("Settings")]
    [Tooltip("Wait time between discard -> draw")]
    [SerializeField] private float drawActionGapAfterDiscard = 0.5f;
    [Tooltip("Gap between drawing each card")]
    [SerializeField] private float drawCardGap = 0.1f;
    [Tooltip("Gap between discarding each card")]
    [SerializeField] private float discardCardGap = 0.1f;
    [Tooltip("Pause after the round is set up, before the first draw")]
    [SerializeField] private float setupPause = 0.5f;

    /// <summary>The round entered a new state.</summary>
    public event Action<State> StateChanged;
    /// <summary>The round's deck is loaded and its goal is known.</summary>
    public event Action<Round> RoundSetup;
    /// <summary>Hands, discards, chip goal or score of the round changed.</summary>
    public event Action<Round> RoundChanged;
    /// <summary>The round is over. bool: was the blind beaten.</summary>
    public event Action<Round, bool> RoundEnded;

    private enum PlayerAction { None, Play, Discard }
    private PlayerAction _pendingAction = PlayerAction.None;
    private readonly List<Card> _scoringCards = new List<Card>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _runManager = RunManager.Instance;
        _scoreCalculator = ScoreCalculator.Instance;
        _handPanel = HandPanel.Instance;
        _usedPanel = UsedPanel.Instance;
        _drawPanel = DrawPanel.Instance;
        _deckPanel = DeckPanel.Instance;
        _playedCardPanel = PlayedCardPanel.Instance;

        if (!_runManager || !_scoreCalculator || !_handPanel || !_usedPanel || !_drawPanel || !_deckPanel ||
            !_playedCardPanel)
        {
            Debug.LogError("RoundManager: a manager or panel it depends on is missing from the scene", this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #region Public API

    /// <summary>
    /// Start playing a round. Ignored while another round is in progress.
    /// </summary>
    public void StartRound(Round round)
    {
        if (round == null || IsRoundActive) return;
        StartCoroutine(RunRound(round));
    }

    /// <summary>
    /// Play button: play the cards selected in hand. Only valid while waiting for the player.
    /// </summary>
    public bool RequestPlay()
    {
        if (!CanAct() || curRound.hands <= 0) return false;
        _pendingAction = PlayerAction.Play;
        return true;
    }

    /// <summary>
    /// Discard button: discard the cards selected in hand. Only valid while waiting for the player.
    /// </summary>
    public bool RequestDiscard()
    {
        if (!CanAct() || curRound.discards <= 0) return false;
        _pendingAction = PlayerAction.Discard;
        return true;
    }

    private bool CanAct()
    {
        return state == State.Play && _pendingAction == PlayerAction.None && curRound != null &&
               _handPanel.numOfSelection > 0;
    }

    public static Round Create(BaseBlindParameters config)
    {
        return new Round
        {
            isComplete = false,
            isSkipped = false,
            blindConfig = config,
            blind = config.Create(),

        };
    }

    #endregion

    #region Round flow

    private void SetState(State newState)
    {
        if (state == newState) return;
        state = newState;
        StateChanged?.Invoke(newState);
    }

    private IEnumerator RunRound(Round round)
    {
        IsRoundActive = true;
        curRound = round;
        handScore = 0;
        _runManager.CurRoundLvl += 1;

        SetState(State.Init);
        yield return SetupRound();

        bool won = false;
        while (true)
        {
            SetState(State.Draw);
            yield return Draw(curRound.handSize - _handPanel.cardsInPanel.Count);
            // if there are no cards in hand -> lose
            if (_handPanel.cardsInPanel.Count == 0) break;

            // wait for the player to play or discard
            _pendingAction = PlayerAction.None;
            SetState(State.Play);
            yield return new WaitUntil(() => _pendingAction != PlayerAction.None);

            if (_pendingAction == PlayerAction.Discard)
            {
                yield return DiscardSelected();
                continue;
            }

            yield return PlaySelected();

            SetState(State.Evaluate);
            if (curRound.roundScore >= curRound.chipGoal)
            {
                won = true;
                break;
            }
            if (curRound.hands <= 0) break;
        }

        EndRound(won);
    }

    /// <summary>
    /// Copy the run's values into the round and load a shuffled deck into the draw pile
    /// </summary>
    private IEnumerator SetupRound()
    {
        curRound.hands = _runManager.Hands;
        curRound.handSize = _runManager.HandSize;
        curRound.discards = _runManager.Discards;
        curRound.roundScore = 0;
        curRound.cardsDeckRound = new List<Card>(_runManager.CardsDeckRun);
        var startingDeck = curRound.cardsDeckRound.OrderBy(x => Random.value).ToList();

        // load card into drawpile
        foreach (var card in startingDeck)
        {
            card.MoveTo(_drawPanel, CardState.CardStatus.InDeck);
            yield return null;
        }

        ExecuteBlindActions(BlindTrigger.Enter);
        RefreshChipGoal();
        RoundSetup?.Invoke(curRound);
        yield return new WaitForSecondsRealtime(setupPause);
    }

    /// <summary>
    /// Draw cards from the top of the draw pile into the hand
    /// </summary>
    /// <param name="num">Amount to draw</param>
    private IEnumerator Draw(int num)
    {
        int count = Mathf.Min(_drawPanel.cardsInPanel.Count, num);
        for (int i = 0; i < count; i++)
        {
            var card = _drawPanel.cardsInPanel[_drawPanel.cardsInPanel.Count - 1];
            card.MoveTo(_handPanel, CardState.CardStatus.InHand);
            yield return new WaitForSecondsRealtime(drawCardGap);
        }
    }

    private IEnumerator DiscardSelected()
    {
        SetState(State.Discard);
        curRound.discards -= 1;
        RoundChanged?.Invoke(curRound);

        foreach (var card in _handPanel.GetSelectionByPosition())
        {
            card.MoveTo(_usedPanel, CardState.CardStatus.InUsed);
            yield return new WaitForSecondsRealtime(discardCardGap);
        }

        yield return new WaitForSecondsRealtime(drawActionGapAfterDiscard);
    }

    private IEnumerator PlaySelected()
    {
        SetState(State.OnPlayed);

        // lock in the hand before the cards leave the selection
        var playedCards = _handPanel.GetSelectionByPosition();
        var hand = HandAnalyzer.Evaluate(playedCards, _scoringCards);

        foreach (var card in playedCards)
        {
            card.MoveTo(_playedCardPanel, CardState.CardStatus.InPlayed);
            yield return new WaitForSecondsRealtime(_handPanel.PlayCardGap);
        }

        curRound.hands -= 1;
        ExecuteBlindActions(BlindTrigger.HandPlayed);
        RefreshChipGoal();
        RoundChanged?.Invoke(curRound);

        yield return _playedCardPanel.HighlightScoringCards(_scoringCards);

        SetState(State.Score);
        yield return _scoreCalculator.ScoreHand(hand.Type, _scoringCards);

        // Add hand score to round score
        handScore = _scoreCalculator.curScore;
        curRound.roundScore += handScore;
        RoundChanged?.Invoke(curRound);

        SetState(State.OnScored);
        yield return _playedCardPanel.RecycleCards(_scoringCards, _usedPanel);
        _scoringCards.Clear();
        _scoreCalculator.ResetScore();
    }

    private void EndRound(bool won)
    {
        if (won)
        {
            curRound.isComplete = true;
            ExecuteBlindActions(BlindTrigger.Win);
            _runManager.Money += curRound.blind.reward;
        }

        // every card goes back to the run deck so the next round starts clean
        foreach (var card in _runManager.CardsDeckRun)
        {
            card.MoveTo(_deckPanel, CardState.CardStatus.InCreation);
        }

        IsRoundActive = false;
        SetState(won ? State.Complete : State.Fail);
        RoundEnded?.Invoke(curRound, won);
    }

    #endregion

    #region Blind effects

    private enum BlindTrigger { Enter, HandPlayed, Win }

    /// <summary>
    /// Run the boss blind's actions for a trigger, lowest execution order first
    /// </summary>
    private void ExecuteBlindActions(BlindTrigger trigger)
    {
        if (!(curRound.blind is BossBlindConfig.BossBlind boss)) return;

        List<BlindActionConfig> actions;
        switch (trigger)
        {
            case BlindTrigger.Enter: actions = boss.actionConfigsOnEnter; break;
            case BlindTrigger.HandPlayed: actions = boss.blindActionConfigs; break;
            default: actions = boss.actionConfigsOnWin; break;
        }
        if (actions == null) return;

        foreach (var action in actions.Where(a => a != null).OrderBy(a => a.executionOrder))
        {
            action.Execute(curRound, state);
        }
    }

    /// <summary>
    /// Chip goal = blind multiplier x base goal of the current ante
    /// </summary>
    private void RefreshChipGoal()
    {
        curRound.chipGoal = curRound.blind.baseChipGoalMultiplier *
                            Constants.GetAnteChipGoal(_runManager.CurAnteLvl);
    }

    #endregion

    public enum State
    {
        None,
        Init,
        // waiting for the player to play or discard
        Play,
        // selected cards are moving to the played panel
        OnPlayed,
        // scoring cards are being scored
        Score,
        // hand score was added to the round, played cards are being removed
        OnScored,
        Evaluate,
        Draw,
        Discard,
        Complete,
        Fail
    }

}

[Serializable]
public class Round
{
    public bool isComplete;
    public bool isSkipped;
    public BaseBlindParameters blindConfig;
    public BaseBlindParameters.BaseBlind blind;

    public int handSize;
    public int hands;
    public int discards;
    public double chipGoal;
    public double roundScore = 0;

    public List<Card> cardsDeckRound;
}
