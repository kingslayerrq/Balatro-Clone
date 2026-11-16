using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene-side wrapper around <see cref="HandEvaluator"/>: tracks the hand type of the
/// cards currently selected in the hand panel.
/// </summary>
public class HandAnalyzer : MonoBehaviour
{
    public static HandAnalyzer Instance;

    private HandPanel _handPanel;
    private RoundManager _roundManager;

    [Header("During Selection")]
    [Tooltip("The highest tier Hand Type of current selection")]
    public Enums.BasePokerHandType curHand = Enums.BasePokerHandType.None;
    [Tooltip("List of Hand Types included in the current selection")]
    public List<Enums.BasePokerHandType> handTypesContained = new List<Enums.BasePokerHandType>();

    /// <summary>Raised when the hand type of the current selection is (re)evaluated.</summary>
    public event Action<Enums.BasePokerHandType> HandTypeChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _handPanel = HandPanel.Instance;
        _roundManager = RoundManager.Instance;

        if (_handPanel)
        {
            _handPanel.SelectionChanged += AnalyzeHand;
        }
        else
        {
            Debug.LogError("HandAnalyzer: no HandPanel in the scene", this);
        }

        if (_roundManager)
        {
            _roundManager.StateChanged += OnRoundStateChanged;
        }
        else
        {
            Debug.LogError("HandAnalyzer: no RoundManager in the scene", this);
        }
    }

    private void OnDestroy()
    {
        if (_handPanel) _handPanel.SelectionChanged -= AnalyzeHand;
        if (_roundManager) _roundManager.StateChanged -= OnRoundStateChanged;
        if (Instance == this) Instance = null;
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

    private void OnRoundStateChanged(RoundManager.State state)
    {
        switch (state)
        {
            case RoundManager.State.Draw:
                // nothing is being considered while cards are drawn
                SetHand(null);
                break;
            case RoundManager.State.Play:
                // cards may already be selected when the player gets control
                AnalyzeHand(_handPanel.cardsInSelection);
                break;
        }
    }

    /// <summary>
    /// Register Hand Types included in current selection
    /// Update the best Hand Type included to current hand type
    /// </summary>
    private void AnalyzeHand(IReadOnlyList<Card> cards)
    {
        // only track the selection while the player is choosing; a hand being played keeps its type on screen
        if (_roundManager.curState != RoundManager.State.Play) return;
        SetHand(Evaluate(cards));
    }

    private void SetHand(HandResult result)
    {
        curHand = result?.Type ?? Enums.BasePokerHandType.None;
        handTypesContained.Clear();
        if (result != null) handTypesContained.AddRange(result.ContainedTypes);

        HandTypeChanged?.Invoke(curHand);
    }
}
