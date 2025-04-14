using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;
    private RunManager _runManager;
    private ScoreCalculator _scoreCalculator;
    private HandPanel _handPanel;
    private UsedPanel _usedPanel;
    private DrawPanel _drawPanel;
    private HandTypeVisualizer _handTypeVisualizer;
    
    public State curState = State.None ;
    [Tooltip("Scored by this hand")]
    [SerializeField] private float handScore = 0;
    public Round curRound = null;


    [Header("Settings")] 
    [Tooltip("Wait time between discard -> draw")]
    [SerializeField] private float drawActionGapAfterDiscard = 0.5f;
    [Tooltip("Gap between drawing each card")]
    [SerializeField] private float drawCardGap = 0.1f;
    [Tooltip("Gap between discarding each card")]
    [SerializeField] private float discardCardGap = 0.1f;
    
    [HideInInspector] public UnityEvent<State> updateRoundStateEvent = new UnityEvent<State>();
    [HideInInspector] public UnityEvent<Card> loadCardEvent = new UnityEvent<Card>();
    [HideInInspector] public UnityEvent<Card> drawCardEvent = new UnityEvent<Card>();
    [HideInInspector] public UnityEvent<Round> roundSetupVisualEvent = new UnityEvent<Round>();
    [HideInInspector] public UnityEvent<Round> discardStartEvent = new UnityEvent<Round>();
    [HideInInspector] public UnityEvent<Card> discardCardEvent = new UnityEvent<Card>();
    [HideInInspector] public UnityEvent<Round> discardEndEvent = new UnityEvent<Round>();
    [HideInInspector] public UnityEvent<Round, bool> roundEndEvent = new UnityEvent<Round, bool>();
    [HideInInspector] public UnityEvent<Round> scoreUpdateEndEvent = new UnityEvent<Round>();
    
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
        _handTypeVisualizer = HandTypeVisualizer.Instance;

        if (_handTypeVisualizer)
        {
            _handTypeVisualizer.UpdateRoundScoreEvent.AddListener(RegisterHandScore);
        }
        
        
        updateRoundStateEvent.AddListener(HandleStateUpdate);
        
    }

    private void HandleStateUpdate(State newState)
    {
        if (curState == newState) return;
        
        curState = newState;
        switch (newState)
        {
            case State.None:
                break;
            case State.Init:
                HandleInitState();
                break;
            case State.Draw:
                StartCoroutine(HandleDrawState());
                break;
            case State.Discard:
                HandleDiscard();
                break;
            case State.Play:
                break;
            case State.OnPlayed:
                OnPlayed();
                break;
            case State.Score:
                break;
            case State.OnScored:
                break;
            case State.Evaluate:
                HandleEvalState();
                break;
            case State.Complete:
                HandleCompleteState();
                break;
            case State.Fail:
                HandleFailState();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Called by Ante Manager to Skip the round
    /// </summary>
    /// <param name="round"></param>
    public void SkipRound(Round round)
    {
        round.isSkipped = true;
    }
    
    /// <summary>
    /// Called by Ante Manager to Start the round
    /// </summary>
    /// <param name="round"></param>
    public void StartRound(Round round)
    {
        curRound = round;
        // Reset
        curState = State.None;
        handScore = 0;
        _runManager.CurRoundLvl += 1;
        updateRoundStateEvent?.Invoke(State.Init);
    }

    private void HandleInitState()
    {
        StartCoroutine(SetupRound());
    }
    
    /// <summary>
    /// Get variable references from RunManager
    /// </summary>
    private IEnumerator SetupRound()
    {
        if (_runManager == null || curRound == null) yield break;
        curRound.hands = _runManager.Hands;
        curRound.handSize = _runManager.HandSize;
        curRound.discards = _runManager.Discards;
        curRound.cardsDeckRound = new List<Card>(_runManager.CardsDeckRun);
        var startingDeck = curRound.cardsDeckRound.OrderBy(x => Random.value).ToList();
        
        // load card into drawpile
        foreach (var card in startingDeck)
        {
            loadCardEvent?.Invoke(card);
            yield return new WaitForEndOfFrame();
        }
        
        curRound.chipGoal = curRound.blind.baseChipGoal * _runManager.CurAnteLvl *
                            Constants.BASE_BLIND_CHIPGOAL[curRound.blind.type];
        roundSetupVisualEvent?.Invoke(curRound);
        yield return new WaitForSecondsRealtime(0.5f);
        updateRoundStateEvent?.Invoke(State.Draw);
    }

    private void HandleDiscard()
    {
        curRound.discards -= 1;
        
        StartCoroutine(OnDiscard());
        IEnumerator OnDiscard()
        {
            discardStartEvent?.Invoke(curRound);
            yield return StartCoroutine(Discard(_handPanel.cardsInSelection, _usedPanel.cardsInPanel));
            discardEndEvent?.Invoke(curRound);
            yield return new WaitForSecondsRealtime(drawActionGapAfterDiscard);
            updateRoundStateEvent?.Invoke(State.Draw);
        }
    }

    private void OnPlayed()
    {
        curRound.hands -= 1;
    }
    /// <summary>
    /// Add hand score to round score
    /// </summary>
    /// <param name="score"></param>
    private void RegisterHandScore(float score)
    {
        handScore = score;
        // Update round score
        curRound.roundScore += handScore;
        
        updateRoundStateEvent?.Invoke(State.OnScored);
        
        
    }

    private void HandleCompleteState()
    {
        curRound.isComplete = true;
        roundEndEvent?.Invoke(curRound, curRound.isComplete);
    }

    private void HandleFailState()
    {
        roundEndEvent?.Invoke(curRound, curRound.isComplete);
    }
    private IEnumerator HandleDrawState()
    {
        yield return StartCoroutine(Draw(curRound.handSize - _handPanel.cardsInPanel.Count,
            _drawPanel.cardsInPanel, _handPanel.cardsInPanel));
        
        // if there are no cards in hand -> lose
        updateRoundStateEvent?.Invoke(_handPanel.cardsInPanel.Count == 0 ? State.Fail : State.Play);
    }

    private void HandleEvalState()
    {
        var isComplete = EvaluateScore();
        if (isComplete)
        {
            updateRoundStateEvent?.Invoke(State.Complete);
        }
        else
        {
            if (curRound.hands > 0)
            {
                updateRoundStateEvent?.Invoke(State.Draw);
            }
            else
            {
                updateRoundStateEvent?.Invoke(State.Fail);
            }
        }
    }

    /// <summary>
    /// Check if round score suffice the round goal
    /// </summary>
    /// <returns></returns>
    private bool EvaluateScore()
    {
        return curRound.roundScore >= curRound.chipGoal;
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
    
    /// <summary>
    /// Draw num amount of cards 
    /// </summary>
    /// <param name="num">Amount to draw</param>
    /// <param name="from">List to draw from</param>
    /// <param name="to">List to draw into</param>
    public IEnumerator Draw(int num, List<Card> from, List<Card> to)
    {
        if (from.Count == 0 || to == null) yield break;
        
        int count = Mathf.Min(from.Count, num);
        var drawn = from.GetRange(from.Count - count, count);
        drawn.Reverse();
        
        foreach (var card in drawn)
        {
            drawCardEvent?.Invoke(card);
            yield return new WaitForSecondsRealtime(drawCardGap);

        }
        from.RemoveRange(from.Count - count, count);
    }

    /// <summary>
    /// Discard X amount 
    /// </summary>
    /// <param name="selection"></param>
    /// <param name="to"></param>
    public IEnumerator Discard(List<Card> selection, List<Card> to)
    {
        if (selection == null) yield break;
        
        foreach (var card in selection)
        {
            discardCardEvent?.Invoke(card);
            yield return new WaitForSecondsRealtime(discardCardGap);
        }

        to.AddRange(selection);
        
    }

    public enum State
    {
        None,
        Init,
        Play,
        OnPlayed,
        Score,
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
    public float chipGoal;
    public float roundScore = 0;
    
    public List<Card> cardsDeckRound;

    // public List<Card> DrawPile
    // {
    //     get => DrawPanel.Instance.cardsInPanel;
    //     set => DrawPanel.Instance.cardsInPanel = value;
    // }
    //
    // public List<Card> HandPile
    // {
    //     get => HandPanel.Instance.cardsInPanel;
    //     set => HandPanel.Instance.cardsInPanel = value;
    // }
    //
    // public List<Card> UsedPile
    // {
    //     get => UsedPanel.Instance.cardsInPanel;
    //     set => UsedPanel.Instance.cardsInPanel = value;
    // }
    
}

