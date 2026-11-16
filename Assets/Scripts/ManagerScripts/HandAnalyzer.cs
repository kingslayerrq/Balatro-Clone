using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Scene-side wrapper around <see cref="HandEvaluator"/>: tracks the hand type of the
/// current selection and, when a hand is played, which of the played cards score.
/// </summary>
public class HandAnalyzer : MonoBehaviour
{
    public static HandAnalyzer Instance;

    private HandPanel _handPanel;
    private RoundManager _roundManager;

    [Header("For Scoring")]
    [Tooltip("The Hand Type that will be used for scoring (Updated when Played)")]
    public Enums.BasePokerHandType scoringHandType;
    [Tooltip("List of Cards that will be used for scoring (Updated when Played)")]
    public List<Card> scoringCards = new List<Card>();

    [Header("During Selection")]
    [Tooltip("The highest tier Hand Type of current selection")]
    public Enums.BasePokerHandType curHand = Enums.BasePokerHandType.None;
    [Tooltip("List of Hand Types included in the current selection")]
    public List<Enums.BasePokerHandType> handTypesContained = new List<Enums.BasePokerHandType>();


    [HideInInspector] public UnityEvent<Enums.BasePokerHandType> UpdateHandTypeEvent = new UnityEvent<Enums.BasePokerHandType>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _handPanel = HandPanel.Instance;

        if (_handPanel)
        {
            _handPanel.onCardSelectionChangedEvent.AddListener(AnalyzeHand);
        }
        _roundManager = RoundManager.Instance;
    }

    /// <summary>
    /// Evaluate a list of cards. Stateless.
    /// </summary>
    /// <param name="cards">Cards to evaluate</param>
    /// <param name="scoringCardsOut">Optional list that receives the cards that score</param>
    public static HandResult Evaluate(IReadOnlyList<Card> cards, List<Card> scoringCardsOut = null)
    {
        var faces = new List<(int rank, Enums.CardSuit suit)>(cards.Count);
        foreach (var card in cards)
        {
            faces.Add((card.cardData.rank, card.cardData.suit));
        }

        // TODO: Apply Jokers
        var result = HandEvaluator.Evaluate(faces);

        if (scoringCardsOut != null)
        {
            scoringCardsOut.Clear();
            foreach (var index in result.ScoringIndices)
            {
                scoringCardsOut.Add(cards[index]);
            }
        }

        return result;
    }

    /// <summary>
    /// Register Hand Types included in current selection
    /// Update the best Hand Type included to current hand type
    /// </summary>
    private void AnalyzeHand(List<Card> cards)
    {
        if (_roundManager.curState != RoundManager.State.Play) return;

        var result = Evaluate(cards);
        curHand = result.Type;
        handTypesContained.Clear();
        handTypesContained.AddRange(result.ContainedTypes);

        UpdateHandTypeEvent?.Invoke(curHand);
    }

    /// <summary>
    /// Lock in the hand type and scoring cards of the hand being played.
    /// Called when the play button is pressed.
    /// </summary>
    public void FinalizeHandType()
    {
        if (_roundManager.curState != RoundManager.State.Play) return;

        // re-evaluate from the live selection instead of trusting the last selection-changed event
        var result = Evaluate(_handPanel.cardsInSelection, scoringCards);
        scoringHandType = result.Type;
        ScoreCalculator.Instance.curHandType = scoringHandType;

        foreach (var card in scoringCards)
        {
            card.cardData.canScore = true;
        }
    }
}
