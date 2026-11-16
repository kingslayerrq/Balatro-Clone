using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the running chips x mults of the current hand and scores a played hand.
/// Raises events when values change; it never waits on a listener.
/// </summary>
[RequireComponent(typeof(ScoreVisualizer))]
public class ScoreCalculator : MonoBehaviour
{
    public static ScoreCalculator Instance;

    private RunManager _runManager;
    private HandAnalyzer _handAnalyzer;

    [Tooltip("Gap between each scoring card")]
    [SerializeField] private float cardScoringGap = 0.2f;
    [Tooltip("How long the final hand score stays on screen before it is added to the round score")]
    [SerializeField] private float scoreDisplayTime = 1f;

    public Enums.BasePokerHandType curHandType = Enums.BasePokerHandType.None;
    private double _chips;
    private double _mults;

    public double curChips
    {
        get => _chips;
        set
        {
            if (_chips.Equals(value)) return;
            _chips = value;
            ChipsChanged?.Invoke(_chips);
        }
    }

    public double curMults
    {
        get => _mults;
        set
        {
            if (_mults.Equals(value)) return;
            _mults = value;
            MultsChanged?.Invoke(_mults);
        }
    }

    /// <summary>Score of the last scored hand (chips x mults). 0 while no hand is being resolved.</summary>
    public double curScore { get; private set; }

    public event Action<HandTypeConfig.HandType> HandTypeChanged;
    public event Action<double> ChipsChanged;
    public event Action<double> MultsChanged;
    /// <summary>The final score of a hand is ready to be shown.</summary>
    public event Action<double> HandScoreShown;
    /// <summary>The final score has been on screen long enough.</summary>
    public event Action HandScoreHidden;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _runManager = RunManager.Instance;
        _handAnalyzer = HandAnalyzer.Instance;

        if (!_runManager) Debug.LogError("ScoreCalculator: no RunManager in the scene", this);

        if (_handAnalyzer)
        {
            _handAnalyzer.HandTypeChanged += SetHandType;
        }
        else
        {
            Debug.LogError("ScoreCalculator: no HandAnalyzer in the scene", this);
        }
    }

    private void OnDestroy()
    {
        if (_handAnalyzer) _handAnalyzer.HandTypeChanged -= SetHandType;
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Load the base chips / mults of a hand type (None clears them)
    /// </summary>
    public void SetHandType(Enums.BasePokerHandType handType)
    {
        curHandType = handType;
        var runHandType = _runManager.GetRunHandTypeInfo(handType);
        curChips = runHandType.baseChips;
        curMults = runHandType.baseMults;
        HandTypeChanged?.Invoke(runHandType);
    }

    /// <summary>
    /// Score a played hand. When the coroutine ends, <see cref="curScore"/> holds the result.
    /// </summary>
    /// <param name="handType">Hand type being played</param>
    /// <param name="scoringCards">Cards that score, in scoring order</param>
    public IEnumerator ScoreHand(Enums.BasePokerHandType handType, List<Card> scoringCards)
    {
        SetHandType(handType);

        // TODO: Apply Jokers
        foreach (var card in scoringCards)
        {
            yield return card.cardScore.ScoreCoroutine(scoringCards, this);
            yield return new WaitForSecondsRealtime(cardScoringGap);
        }

        curScore = curChips * curMults;

        // show the total, clear the hand type panel underneath it
        HandScoreShown?.Invoke(curScore);
        SetHandType(Enums.BasePokerHandType.None);
        yield return new WaitForSecondsRealtime(scoreDisplayTime);
        HandScoreHidden?.Invoke();
    }

    /// <summary>
    /// Called once the hand score has been added to the round
    /// </summary>
    public void ResetScore()
    {
        curScore = 0;
    }
}
