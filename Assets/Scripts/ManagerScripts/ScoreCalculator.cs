using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ScoreVisualizer))]
public class ScoreCalculator : MonoBehaviour
{
    public static ScoreCalculator Instance;
    
    
    private JokerPanel _jokerPanel;
    private PlayedCardPanel _playedCardPanel;
    private HandPanel _handPanel;
    private RunManager _runManager;
    private RoundManager _roundManager;
    private HandAnalyzer _handAnalyzer;

    [SerializeField] private float cardScoringGap = 0.2f;

    public Enums.BasePokerHandType curHandType = Enums.BasePokerHandType.None;
    public float curChips = 0;
    public float curMults = 0;
    public float curScore = 0;
    private float _lastChips = 0;
    private float _lastMults = 0;
    private float _lastScore = 0;

    [HideInInspector]
    public UnityEvent<HandTypeConfig.HandType> UpdateHandTypeVisualEvent = new UnityEvent<HandTypeConfig.HandType>();

    [HideInInspector] public UnityEvent<float> UpdateChipsVisualEvent = new UnityEvent<float>();
    [HideInInspector] public UnityEvent<float> UpdateMultsVisualEvent = new UnityEvent<float>();
    [HideInInspector] public UnityEvent<float> UpdateScoreVisualEvent = new UnityEvent<float>();
    //[HideInInspector] public UnityEvent<float> OnScoreEndEvent = new UnityEvent<float>();
    [HideInInspector] public UnityEvent<Card> OnCardUsedEvent = new UnityEvent<Card>();
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _jokerPanel = JokerPanel.Instance;
        _playedCardPanel = PlayedCardPanel.Instance;
        _runManager = RunManager.Instance;
        _roundManager = RoundManager.Instance;
        _handAnalyzer = HandAnalyzer.Instance;
    
        
        if (_handAnalyzer && _runManager)
        {
            _handAnalyzer.UpdateHandTypeEvent.AddListener(UpdateCalculatorByHandType);
        }
        if (_playedCardPanel)
        {
            _playedCardPanel.calculateScoringCardsEvent.AddListener(CalculateScore);
        }

        if (_roundManager)
        {
            _roundManager.updateRoundStateEvent.AddListener(ScoreStateHandler);
            _roundManager.updateRoundStateEvent.AddListener(CleanUpAfterScore);
        }

        
    }

    private void Update()
    {
        if (_lastChips != curChips)
        {
            _lastChips = curChips;
            UpdateChipsVisualEvent?.Invoke(curChips);
        }

        if (_lastMults != curMults)
        {
            _lastMults = curMults;
            UpdateMultsVisualEvent?.Invoke(curMults);
        }

        if (_lastScore != curScore)
        {
            _lastScore = curScore;
            UpdateScoreVisualEvent?.Invoke(curScore);
            
            _handAnalyzer.curHand = Enums.BasePokerHandType.None;
            _handAnalyzer.UpdateHandTypeEvent?.Invoke(_handAnalyzer.curHand);
        } 
    }

    private void ScoreStateHandler(RoundManager.State state)
    {
        if (state != RoundManager.State.Score) return;
        CalculateScore(_playedCardPanel.cardsInSelection);
        
    }
    private void CalculateScore(List<Card> cards)
    {
        // TODO: Apply Jokers
        StartCoroutine(ScoreCards(cards));
    }

    private IEnumerator ScoreCards(List<Card> cards)
    {
        foreach (var card in cards)
        {
            yield return StartCoroutine(card.cardScore.ScoreCoroutine());
            yield return new WaitForSecondsRealtime(cardScoringGap);
        }

        curScore = curChips * curMults;
        

        yield return new WaitForSecondsRealtime(0.5f);

        //_roundManager.updateRoundStateEvent?.Invoke(RoundManager.State.OnScored);
        // yield return StartCoroutine(RecycleCards(cards));


    }

    private void CleanUpAfterScore(RoundManager.State state)
    {
        if (state != RoundManager.State.OnScored) return;
        StartCoroutine(RecycleCards(_playedCardPanel.cardsInSelection));
    }

    /// <summary>
    /// push down the used cards and put them into usedcard panel
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    private IEnumerator RecycleCards(List<Card> cards)
    {
        var cardsCopy = cards.ToArray();
        foreach (var card in cardsCopy)
        {
            card.isSelected = false;
            Card.selectEvent?.Invoke(card, card.isSelected, card.curPanel);
            card.SelectTransformUpdate();
            
            yield return new WaitForSecondsRealtime(0.1f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        var cardsInPanelCopy = _playedCardPanel.cardsInPanel.ToArray();
        foreach (var card in cardsInPanelCopy)
        {
            
            yield return new WaitForEndOfFrame();
            OnCardUsedEvent?.Invoke(card);
            _playedCardPanel.cardsInPanel.Remove(card);
            yield return new WaitForSecondsRealtime(0.1f);
        }
        
        //OnScoreEndEvent?.Invoke(curScore);
        
        
        curScore = 0;
        _roundManager.updateRoundStateEvent?.Invoke(RoundManager.State.Evaluate);
        
    }
    
    private void UpdateCalculatorByHandType(Enums.BasePokerHandType handType)
    {
        var runHandType = _runManager.GetRunHandTypeInfo(handType);
        curChips = runHandType.baseChips;
        curMults = runHandType.baseMults;
        UpdateHandTypeVisualEvent?.Invoke(runHandType);
    }
}
