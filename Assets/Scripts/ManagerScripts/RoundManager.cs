using System;
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
    
    public State curState = State.None ;
    public Round curRound = null;
 
    

    
    [HideInInspector] public UnityEvent<State> updateRoundStateEvent = new UnityEvent<State>();
    [HideInInspector] public UnityEvent<Card> drawCardEvent = new UnityEvent<Card>();
    [HideInInspector] public UnityEvent<Card> discardCardEvent = new UnityEvent<Card>();
    [HideInInspector] public UnityEvent<Round> roundSetupVisualEvent = new UnityEvent<Round>();
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        
    }

    private void Start()
    {
        _runManager = RunManager.Instance;
        updateRoundStateEvent.AddListener(HandleStateUpdate);
    }

    private void HandleStateUpdate(State newState)
    {
        if (curState == newState) return;
        
        curState = newState;
        switch (newState)
        {
            case State.Init:
                HandleInitState();
                break;
            case State.Play:
                break;
            case State.Score:
                break;
            case State.Evaluate:
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
        _runManager.CurRoundLvl += 1;
        updateRoundStateEvent.Invoke(State.Init);
        
    }

    private void HandleInitState()
    {
        Debug.LogWarning("Init state");
        SetupRound();
        Draw(curRound.handSize, curRound.drawPile, curRound.handPile);
        updateRoundStateEvent?.Invoke(State.Play);
    }
    
    /// <summary>
    /// Get variable references from RunManager
    /// </summary>
    private void SetupRound()
    {
        if (_runManager == null || curRound == null) return;
        curRound.hands = _runManager.Hands;
        curRound.handSize = _runManager.HandSize;
        curRound.discards = _runManager.Discards;
        curRound.cardsDeckRound = new List<Card>(_runManager.CardsDeckRun);
        curRound.drawPile = curRound.cardsDeckRound.OrderBy(x => Random.value).ToList();
        curRound.chipGoal = curRound.blind.baseChipGoal * _runManager.CurAnteLvl *
                            Constants.BASE_BLIND_CHIPGOAL[curRound.blind.type];
        
        roundSetupVisualEvent?.Invoke(curRound);
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
    public void Draw(int num, List<Card> from, List<Card> to)
    {
        if (from.Count == 0 || to == null) return;
        
        int count = Mathf.Min(from.Count, num);
        
        var drawn = from.GetRange(0, count);
        
        to.AddRange(drawn);
        
        foreach (var card in drawn)
        {
            drawCardEvent.Invoke(card);
        }
        from.RemoveRange(0, count);
    }

    /// <summary>
    /// Discard X amount then draw to fill hands
    /// </summary>
    /// <param name="selection"></param>
    /// <param name="to"></param>
    public void Discard(List<Card> selection, List<Card> to)
    {
        if (selection == null) return;
        
        foreach (var card in selection)
        {
            discardCardEvent.Invoke(card);
        }

        to.AddRange(selection);
        
        selection.Clear();

        // Draw to fill hands
        var drawAmount = this.curRound.handSize - this.curRound.handPile.Count;
        
        Draw(drawAmount, this.curRound.drawPile, this.curRound.handPile);
    }

    public enum State
    {
        None,
        Init,
        Play,
        Score,
        Evaluate,
        Draw,
        Result
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
    
    public List<Card> cardsDeckRound;
    public List<Card> drawPile = new List<Card>();
    public List<Card> handPile = new List<Card>();
    public List<Card> usedPile = new List<Card>();
    
}

