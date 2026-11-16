using System.Collections.Generic;
using NUnit.Framework;
using HandType = Enums.BasePokerHandType;
using Suit = Enums.CardSuit;

public class HandEvaluatorTests
{
    private const Suit S = Suit.Spade, H = Suit.Heart, C = Suit.Club, D = Suit.Diamond;

    private static HandResult Eval(params (int, Suit)[] cards) => HandEvaluator.Evaluate(cards);

    [Test]
    public void EmptySelection_IsNone()
    {
        var r = Eval();
        Assert.AreEqual(HandType.None, r.Type);
        Assert.IsEmpty(r.ScoringIndices);
        Assert.AreEqual(HandType.None, HandEvaluator.Evaluate(null).Type);
    }

    [Test]
    public void HighCard_ScoresOnlyHighestCard()
    {
        var r = Eval((3, S), (13, H), (9, C));
        Assert.AreEqual(HandType.HighCard, r.Type);
        CollectionAssert.AreEqual(new[] { 1 }, r.ScoringIndices);
    }

    [Test]
    public void Pair_ScoresOnlyThePair()
    {
        var r = Eval((7, S), (2, H), (7, C), (14, D));
        Assert.AreEqual(HandType.Pair, r.Type);
        CollectionAssert.AreEqual(new[] { 0, 2 }, r.ScoringIndices);
    }

    [Test]
    public void TwoPair_ScoresBothPairs()
    {
        var r = Eval((7, S), (9, H), (7, C), (9, D), (2, S));
        Assert.AreEqual(HandType.TwoPair, r.Type);
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, r.ScoringIndices);
    }

    [Test]
    public void ThreeOfAKind_ScoresTheTriple()
    {
        var r = Eval((5, S), (5, H), (11, C), (5, D), (2, S));
        Assert.AreEqual(HandType.ThreeOfAKind, r.Type);
        CollectionAssert.AreEqual(new[] { 0, 1, 3 }, r.ScoringIndices);
    }

    [Test]
    public void Straight_AllCardsScore()
    {
        var r = Eval((6, S), (4, H), (5, C), (8, D), (7, S));
        Assert.AreEqual(HandType.Straight, r.Type);
        Assert.AreEqual(5, r.ScoringIndices.Count);
    }

    [Test]
    public void WheelStraight_AceCountsLow()
    {
        var r = Eval((14, S), (2, H), (3, C), (4, D), (5, S));
        Assert.AreEqual(HandType.Straight, r.Type);
    }

    [Test]
    public void AceDoesNotWrapAround()
    {
        var r = Eval((12, S), (13, H), (14, C), (2, D), (3, S));
        Assert.AreEqual(HandType.HighCard, r.Type);
    }

    [Test]
    public void FourCards_AreNeverStraightOrFlush()
    {
        var r = Eval((6, S), (7, S), (8, S), (9, S));
        Assert.AreEqual(HandType.HighCard, r.Type);
    }

    [Test]
    public void Flush_AllCardsScore()
    {
        var r = Eval((2, H), (9, H), (11, H), (4, H), (13, H));
        Assert.AreEqual(HandType.Flush, r.Type);
        Assert.AreEqual(5, r.ScoringIndices.Count);
    }

    [Test]
    public void PairInsideFlush_IsFlush()
    {
        // only possible with a modified deck (two cards of the same rank and suit)
        var r = Eval((9, H), (9, H), (11, H), (4, H), (13, H));
        Assert.AreEqual(HandType.Flush, r.Type);
        Assert.IsTrue(r.Contains(HandType.Pair));
        Assert.AreEqual(5, r.ScoringIndices.Count);
    }

    [Test]
    public void FullHouse()
    {
        var r = Eval((9, H), (9, S), (4, C), (4, H), (9, D));
        Assert.AreEqual(HandType.FullHouse, r.Type);
        Assert.IsTrue(r.Contains(HandType.TwoPair));
        Assert.IsTrue(r.Contains(HandType.ThreeOfAKind));
        Assert.AreEqual(5, r.ScoringIndices.Count);
    }

    [Test]
    public void FourOfAKind_KickerDoesNotScore()
    {
        var r = Eval((9, H), (9, S), (4, C), (9, C), (9, D));
        Assert.AreEqual(HandType.FourOfAKind, r.Type);
        CollectionAssert.AreEqual(new[] { 0, 1, 3, 4 }, r.ScoringIndices);
    }

    [Test]
    public void StraightFlush()
    {
        var r = Eval((5, C), (6, C), (7, C), (8, C), (9, C));
        Assert.AreEqual(HandType.StraightFlush, r.Type);
        Assert.IsFalse(r.Contains(HandType.RoyalFlush));
    }

    [Test]
    public void RoyalFlush()
    {
        var r = Eval((10, D), (11, D), (12, D), (13, D), (14, D));
        Assert.AreEqual(HandType.RoyalFlush, r.Type);
    }

    [Test]
    public void FiveOfAKind_IsNotAFullHouse()
    {
        var r = Eval((8, S), (8, H), (8, C), (8, D), (8, S));
        Assert.AreEqual(HandType.FiveOfAKind, r.Type);
        Assert.IsFalse(r.Contains(HandType.FullHouse));
        Assert.AreEqual(5, r.ScoringIndices.Count);
    }

    [Test]
    public void FlushHouse()
    {
        var r = Eval((9, H), (9, H), (4, H), (4, H), (9, H));
        Assert.AreEqual(HandType.FlushHouse, r.Type);
    }

    [Test]
    public void FlushFive()
    {
        var r = Eval((8, S), (8, S), (8, S), (8, S), (8, S));
        Assert.AreEqual(HandType.FlushFive, r.Type);
    }

    [Test]
    public void ContainedTypes_AreAscendingAndEndWithType()
    {
        var r = Eval((9, H), (9, S), (4, C), (4, H), (9, D));
        var sorted = new List<HandType>(r.ContainedTypes);
        sorted.Sort();
        CollectionAssert.AreEqual(sorted, r.ContainedTypes);
        Assert.AreEqual(r.Type, r.ContainedTypes[r.ContainedTypes.Count - 1]);
    }
}
