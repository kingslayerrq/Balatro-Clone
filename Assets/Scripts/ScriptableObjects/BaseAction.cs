using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Everything a scoring action may read or change while a card scores.
/// Actions go through this instead of reaching into the calculator, so later effects
/// (jokers, seals, editions) can be added in one place.
/// </summary>
public class ScoreContext
{
    private readonly ScoreCalculator _calculator;

    /// <summary>The card whose action is running</summary>
    public Card Card { get; }
    /// <summary>Every card scoring in the played hand, in scoring order</summary>
    public IReadOnlyList<Card> ScoringCards { get; }
    public Enums.BasePokerHandType HandType => _calculator.curHandType;

    public double Chips
    {
        get => _calculator.curChips;
        set => _calculator.curChips = value;
    }

    public double Mults
    {
        get => _calculator.curMults;
        set => _calculator.curMults = value;
    }

    public ScoreContext(ScoreCalculator calculator, Card card, IReadOnlyList<Card> scoringCards)
    {
        _calculator = calculator;
        Card = card;
        ScoringCards = scoringCards;
    }
}

/// <summary>
/// A scoring effect authored as an asset. Actions hold no per-run state,
/// so the asset itself is executed.
/// </summary>
public abstract class BaseActionConfig : ScriptableObject
{
    [Tooltip("Name of this scoring action")]
    public string actionName;

    [TextArea(3, 5)]
    [Tooltip("Description of what this action does")]
    public string description;

    [Tooltip("Order in which this action is executed (lower numbers go first)")]
    public int executionOrder = 0;

    public abstract void Execute(ScoreContext context);
}
