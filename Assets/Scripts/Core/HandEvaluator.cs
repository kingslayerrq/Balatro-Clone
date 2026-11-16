using System.Collections.Generic;

/// <summary>
/// Result of evaluating a set of played/selected cards.
/// </summary>
public sealed class HandResult
{
    /// <summary>Highest tier hand type contained in the cards.</summary>
    public Enums.BasePokerHandType Type = Enums.BasePokerHandType.None;

    /// <summary>Every hand type contained in the cards (ascending tier).</summary>
    public readonly List<Enums.BasePokerHandType> ContainedTypes = new List<Enums.BasePokerHandType>();

    /// <summary>Indices (into the evaluated list, ascending) of the cards that score for <see cref="Type"/>.</summary>
    public readonly List<int> ScoringIndices = new List<int>();

    public bool Contains(Enums.BasePokerHandType type) => ContainedTypes.Contains(type);
}

/// <summary>
/// Pure poker hand evaluation. No scene state, safe to call from anywhere and from tests.
/// </summary>
public static class HandEvaluator
{
    public const int AceRank = 14;
    private const int FullHandSize = 5;

    public static HandResult Evaluate(IReadOnlyList<(int rank, Enums.CardSuit suit)> cards)
    {
        var result = new HandResult();
        if (cards == null || cards.Count == 0) return result;

        int count = cards.Count;
        var rankCounts = new Dictionary<int, int>();
        var suits = new HashSet<Enums.CardSuit>();
        int minRank = int.MaxValue, maxRank = int.MinValue;

        for (int i = 0; i < count; i++)
        {
            int rank = cards[i].rank;
            rankCounts.TryGetValue(rank, out int c);
            rankCounts[rank] = c + 1;
            suits.Add(cards[i].suit);
            if (rank < minRank) minRank = rank;
            if (rank > maxRank) maxRank = rank;
        }

        int maxDupe = 0;        // size of the largest same-rank group
        int groupsOfTwoPlus = 0; // number of ranks appearing at least twice
        foreach (var c in rankCounts.Values)
        {
            if (c > maxDupe) maxDupe = c;
            if (c >= 2) groupsOfTwoPlus++;
        }

        bool fullHand = count == FullHandSize;
        bool isFlush = fullHand && suits.Count == 1;
        bool isWheel = rankCounts.Count == FullHandSize && rankCounts.ContainsKey(AceRank) &&
                       rankCounts.ContainsKey(2) && rankCounts.ContainsKey(3) &&
                       rankCounts.ContainsKey(4) && rankCounts.ContainsKey(5);
        bool isStraight = fullHand && rankCounts.Count == FullHandSize &&
                          (maxRank - minRank == FullHandSize - 1 || isWheel);
        bool isRoyal = isStraight && isFlush && minRank == 10 && maxRank == AceRank;
        // a group of 3+ together with a second, different group of 2+
        bool isFullHouse = fullHand && maxDupe >= 3 && groupsOfTwoPlus >= 2;
        bool isFive = maxDupe >= 5;

        var contained = result.ContainedTypes;
        contained.Add(Enums.BasePokerHandType.HighCard);
        if (maxDupe >= 2) contained.Add(Enums.BasePokerHandType.Pair);
        if (groupsOfTwoPlus >= 2) contained.Add(Enums.BasePokerHandType.TwoPair);
        if (maxDupe >= 3) contained.Add(Enums.BasePokerHandType.ThreeOfAKind);
        if (isStraight) contained.Add(Enums.BasePokerHandType.Straight);
        if (isFlush) contained.Add(Enums.BasePokerHandType.Flush);
        if (isFullHouse) contained.Add(Enums.BasePokerHandType.FullHouse);
        if (maxDupe >= 4) contained.Add(Enums.BasePokerHandType.FourOfAKind);
        if (isStraight && isFlush) contained.Add(Enums.BasePokerHandType.StraightFlush);
        if (isRoyal) contained.Add(Enums.BasePokerHandType.RoyalFlush);
        if (isFive) contained.Add(Enums.BasePokerHandType.FiveOfAKind);
        if (isFullHouse && isFlush) contained.Add(Enums.BasePokerHandType.FlushHouse);
        if (isFive && isFlush) contained.Add(Enums.BasePokerHandType.FlushFive);

        // ContainedTypes is added in ascending tier order, so the last one is the best
        result.Type = contained[contained.Count - 1];

        switch (result.Type)
        {
            case Enums.BasePokerHandType.HighCard:
                // first card holding the highest rank
                for (int i = 0; i < count; i++)
                {
                    if (cards[i].rank != maxRank) continue;
                    result.ScoringIndices.Add(i);
                    break;
                }
                break;
            case Enums.BasePokerHandType.Pair:
            case Enums.BasePokerHandType.TwoPair:
            case Enums.BasePokerHandType.ThreeOfAKind:
            case Enums.BasePokerHandType.FourOfAKind:
            case Enums.BasePokerHandType.FiveOfAKind:
                // only the grouped cards score, kickers do not
                for (int i = 0; i < count; i++)
                {
                    if (rankCounts[cards[i].rank] >= 2) result.ScoringIndices.Add(i);
                }
                break;
            default:
                // five-card hands: every card scores
                for (int i = 0; i < count; i++) result.ScoringIndices.Add(i);
                break;
        }

        return result;
    }
}
