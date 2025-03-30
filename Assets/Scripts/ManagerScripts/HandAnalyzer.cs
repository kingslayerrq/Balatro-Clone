using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandAnalyzer : MonoBehaviour
{
    private HandPanel _handPanel;
    // TODO: mark the scoring cards to canScore!!!
    
    [Tooltip("The highest tier Hand Type of current selection")]
    [SerializeField] private Enums.BasePokerHandType curHand = Enums.BasePokerHandType.None;
    [Tooltip("The Hand Type that will be used for scoring")]
    public Enums.BasePokerHandType scoringHand;
    [Tooltip("List of Hand Types included in the current selection")]
    public List<Enums.BasePokerHandType> handTypesContained = new List<Enums.BasePokerHandType>();
    
    private void Start()
    {
        _handPanel = HandPanel.Instance;

        if (_handPanel != null)
        {
            _handPanel.onCardSelectionChangedEvent.AddListener(AnalyzeHand);
            _handPanel.handPlayedEvent.AddListener(FinalizeHandType);
        }
    }

    /// <summary>
    /// Register Hand Type included in current selection
    /// Update the best Hand Type included to current hand type
    /// </summary>
    /// <param name="cards"></param>
    private void AnalyzeHand(List<Card> cards)
    {
        handTypesContained.Clear();

        // TODO: Apply Jokers
        int count = cards.Count;
        if (count < 1) return;
        
        List<(int, Enums.CardSuit)> ranknsuit = new List<(int, Enums.CardSuit)>();

        foreach (Card card in cards)
        {
            int rank = card.cardData.rank;
            Enums.CardSuit suit = card.cardData.suit;
            ranknsuit.Add((rank, suit));
        }
        
        Dictionary<(int, Enums.CardSuit), int> rnsDictionary = new Dictionary<(int, Enums.CardSuit), int>();
        Dictionary<int, int> rankDictionary = new Dictionary<int, int>();
        Dictionary<Enums.CardSuit, int> suitDictionary = new Dictionary<Enums.CardSuit, int>();
        
        // populate dictionaries
        foreach (var rns in ranknsuit)
        {
            var rank = rns.Item1;
            var suit = rns.Item2;
            
            if (rnsDictionary.ContainsKey(rns))
            {
                rnsDictionary[rns] += 1;
            }
            else
            {
                rnsDictionary[rns] = 1;
            }

            if (rankDictionary.ContainsKey(rank))
            {
                rankDictionary[rank] += 1;
            }
            else
            {
                rankDictionary[rank] = 1;
            }

            if (suitDictionary.ContainsKey(suit))
            {
                suitDictionary[suit] += 1;
            }
            else
            {
                suitDictionary[suit] = 1;
            }
        }

        var maxDupeCount = rankDictionary.Values.Max();
        bool hasDupe = maxDupeCount >= 2;

        
        
        // Hand with Dupes
        if (hasDupe)
        {
            // Pair
            handTypesContained.Add(Enums.BasePokerHandType.Pair);
            // Two Pair
            if (rankDictionary.Values.Where((i => i >= 2)).Count() >= 2)
            {
                handTypesContained.Add(Enums.BasePokerHandType.TwoPair);
            }
            // Three of a Kind
            if (maxDupeCount >= 3) handTypesContained.Add(Enums.BasePokerHandType.ThreeOfAKind);
            // Four of a Kind
            if (maxDupeCount >= 4) handTypesContained.Add(Enums.BasePokerHandType.FourOfAKind);
            // Full House
            if (count == 5)
            {
                var isFlush = suitDictionary.Values.Count == 1;
                if (rankDictionary.Values.Contains(3) && rankDictionary.Values.Contains(2))
                {
                    handTypesContained.Add(Enums.BasePokerHandType.FullHouse);
                    // Flush House
                    if (isFlush) handTypesContained.Add(Enums.BasePokerHandType.FlushHouse);
                }
                // Five of a Kind
                if (maxDupeCount == 5)
                {
                    handTypesContained.Add(Enums.BasePokerHandType.FiveOfAKind);
                    // Flush Five
                    if (isFlush) handTypesContained.Add(Enums.BasePokerHandType.FlushFive);
                }
            }
        }
        // No Dupes
        else
        {
            // HighCard
            handTypesContained.Add(Enums.BasePokerHandType.HighCard);
            
            if (count == 5)
            {
                bool isStraight = false;
                bool isFlush = false;
                bool isAceStraight = false;
                
                // Straight
                var specialCase = (rankDictionary.Keys.Contains(14) && rankDictionary.Keys.Contains(2) &&
                                    rankDictionary.Keys.Contains(3)
                                    && rankDictionary.Keys.Contains(4) && rankDictionary.Keys.Contains(5));
                var maxRank = rankDictionary.Keys.Max();
                var minRank = rankDictionary.Keys.Min();
                if ((maxRank - minRank) == 4 || specialCase)
                {
                    isStraight = true;
                    if (maxRank == 14 && minRank == 10) isAceStraight = true;
                    handTypesContained.Add(Enums.BasePokerHandType.Straight);
                }

                // Flush
                if (suitDictionary.Values.Count == 1)
                {
                    isFlush = true;
                    handTypesContained.Add(Enums.BasePokerHandType.Flush);
                }
                
                // Straight Flush
                if (isFlush && isStraight)
                {
                    handTypesContained.Add(Enums.BasePokerHandType.StraightFlush);
                    // Royal Flush
                    if (isAceStraight) handTypesContained.Add(Enums.BasePokerHandType.RoyalFlush);
                }
            }
        }

        curHand = handTypesContained.Count > 0 ? handTypesContained.Max() : Enums.BasePokerHandType.None;
        
        Debug.LogWarning("Cur Hand Type: " + curHand);
    }

    /// <summary>
    /// Update the Scoring Hand Type
    /// </summary>
    /// <param name="panel"></param>
    private void FinalizeHandType(Panel panel)
    {
        scoringHand = curHand;
        // reset
        curHand = Enums.BasePokerHandType.None;
    }
}
