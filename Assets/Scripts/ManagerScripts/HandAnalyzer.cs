using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

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

    
    [Header("For Analysis")] 
    [Tooltip("Hold Reference of Cards Selected from Hand Panel")] 
    [SerializeField] private List<Card> cardsInSelectionRef = new List<Card>();
    [Tooltip("{(Rank, Suit), Card} for (rank, suit) -> card lookups")]
    [SerializeField] private Dictionary<(int, Enums.CardSuit), List<Card>> rnsCardReferenceDictionary =
        new Dictionary<(int, Enums.CardSuit), List<Card>>();
    [Tooltip("{Rank, Card} for rank->card lookups")]
    private Dictionary<int, List<Card>> rankCardReferenceDictionary = new Dictionary<int, List<Card>>();
    [Tooltip("{(Rank, Suit), Count}")]
    private Dictionary<(int, Enums.CardSuit), int> _rnsDictionary = new Dictionary<(int, Enums.CardSuit), int>();
    [Tooltip("{Rank, Count}")]
    private Dictionary<int, int> _rankDictionary = new Dictionary<int, int>();
    [Tooltip("{Suit, Count}")]
    private Dictionary<Enums.CardSuit, int> _suitDictionary = new Dictionary<Enums.CardSuit, int>();

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
    /// Reset Dictionaries before each analysis
    /// </summary>
    private void ResetDictionaries()
    {
        handTypesContained.Clear();
        cardsInSelectionRef.Clear();
        rnsCardReferenceDictionary.Clear();
        rankCardReferenceDictionary.Clear();
        _rnsDictionary.Clear();
        _rankDictionary.Clear();
        _suitDictionary.Clear();
    }

    /// <summary>
    /// Populate dictionaries to help analyze hand
    /// </summary>
    /// <param name="ranknsuit"></param>
    private void PopulateDictionaries(List<(int, Enums.CardSuit)> ranknsuit)
    {
        foreach (var rns in ranknsuit)
        {
            var rank = rns.Item1;
            var suit = rns.Item2;
            
            if (_rnsDictionary.ContainsKey(rns))
            {
                _rnsDictionary[rns] += 1;
            }
            else
            {
                _rnsDictionary[rns] = 1;
            }

            if (_rankDictionary.ContainsKey(rank))
            {
                _rankDictionary[rank] += 1;
            }
            else
            {
                _rankDictionary[rank] = 1;
            }

            if (_suitDictionary.ContainsKey(suit))
            {
                _suitDictionary[suit] += 1;
            }
            else
            {
                _suitDictionary[suit] = 1;
            }
        }
    }

    /// <summary>
    /// Register Hand Type included in current selection
    /// Update the best Hand Type included to current hand type
    /// </summary>
    /// <param name="cards"></param>
    private void AnalyzeHand(List<Card> cards)
    {
        ResetDictionaries();
        
        
        // TODO: Apply Jokers
        int count = cards.Count;
        if (count < 1)
        {
            curHand = Enums.BasePokerHandType.None;
        }
        else
        {
            // convert to (Rank, Suit) Tuples for analysis
            List<(int, Enums.CardSuit)> ranknsuit = cards.Select(card => (card.cardData.rank, card.cardData.suit)).ToList();
            // references between (rank, suit) -> card
            // references between rank -> card
            foreach (var card in cards)
            {
                var rns = (card.cardData.rank, card.cardData.suit);
                if (!rnsCardReferenceDictionary.Keys.Contains(rns))
                {
                    rnsCardReferenceDictionary[rns] = new List<Card>();
                }
                rnsCardReferenceDictionary[rns].Add(card);
                if (!rankCardReferenceDictionary.Keys.Contains(card.cardData.rank))
                {
                    rankCardReferenceDictionary[card.cardData.rank] = new List<Card>();
                }
                rankCardReferenceDictionary[card.cardData.rank].Add(card);
                cardsInSelectionRef.Add(card);
            }
            
            
            // populate dictionaries with ranknsuit
            PopulateDictionaries(ranknsuit);
            
            // analysis
            #region Analysis Logic
            
            var maxDupeCount = _rankDictionary.Values.Max();
            bool hasDupe = maxDupeCount >= 2;
            
            if (hasDupe)
            {
                // Pair
                handTypesContained.Add(Enums.BasePokerHandType.Pair);
                // Two Pair
                if (_rankDictionary.Values.Where((i => i >= 2)).Count() >= 2)
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
                    var isFlush = _suitDictionary.Values.Count == 1;
                    if (_rankDictionary.Values.Contains(3) && _rankDictionary.Values.Contains(2))
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
                    var specialCase = (_rankDictionary.Keys.Contains(14) && _rankDictionary.Keys.Contains(2) &&
                                        _rankDictionary.Keys.Contains(3)
                                        && _rankDictionary.Keys.Contains(4) && _rankDictionary.Keys.Contains(5));
                    var maxRank = _rankDictionary.Keys.Max();
                    var minRank = _rankDictionary.Keys.Min();
                    if ((maxRank - minRank) == 4 || specialCase)
                    {
                        isStraight = true;
                        if (maxRank == 14 && minRank == 10) isAceStraight = true;
                        handTypesContained.Add(Enums.BasePokerHandType.Straight);
                    }

                    // Flush
                    if (_suitDictionary.Values.Count == 1)
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

            #endregion
            
            // return highest tier handtype
            curHand = handTypesContained.Count > 0 ? handTypesContained.Max() : Enums.BasePokerHandType.None;
        }

        //Debug.LogWarning("Cur Hand Type: " + curHand);
        UpdateHandTypeEvent?.Invoke(curHand);
    }

    /// <summary>
    /// Identify scoring cards based on handtype
    /// </summary>
    /// <param name="cards"></param>
    /// <param name="handType"></param>
    private void IdentifyScoringCards(List<Card> cards, Enums.BasePokerHandType handType)
    {
        List<Card> scoringCardsList = new List<Card>();
        
        // Selecting the cards
        int rank;
        switch (handType)
        {
            case Enums.BasePokerHandType.None:
                break;
            case Enums.BasePokerHandType.HighCard:
                rank = _rankDictionary.Keys.Max();
                var scoringCard = rankCardReferenceDictionary[rank][0];
                if (scoringCard) scoringCardsList.Add(scoringCard);
                break;
            case Enums.BasePokerHandType.Pair:
                rank = _rankDictionary.FirstOrDefault(kvp => kvp.Value == 2).Key;
                for (int i = 0; i < 2; i++)
                {
                    scoringCardsList.Add(rankCardReferenceDictionary[rank][i]);
                }
                break;
            case Enums.BasePokerHandType.TwoPair:
                var ranks = _rankDictionary.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();
                for (int i = 0; i < 2; i++)
                {
                    var rankCards = rankCardReferenceDictionary[ranks[i]];
                    for (int j = 0; j < 2; j++)
                    {
                        scoringCardsList.Add(rankCards[j]);
                    }
                }
                break;
            case Enums.BasePokerHandType.ThreeOfAKind:
                rank = _rankDictionary.FirstOrDefault(kvp => kvp.Value == 3).Key;
                for (int i = 0; i < 3; i++)
                {
                    scoringCardsList.Add(rankCardReferenceDictionary[rank][i]);
                }
                break;
            case Enums.BasePokerHandType.FourOfAKind:
                rank = _rankDictionary.FirstOrDefault(kvp => kvp.Value == 4).Key;
                for (int i = 0; i < 4; i++)
                {
                    scoringCardsList.Add(rankCardReferenceDictionary[rank][i]);
                }
                break;
            default:
                foreach (var card in cards)
                {
                    Debug.LogWarning(card);
                    scoringCardsList.Add(card);
                }
                break;
        }
        
        // Update the cards
        MarkScoringCards(scoringCardsList);
    }

    
    /// <summary>
    /// Mark the scoring cards to canScore
    /// </summary>
    /// <param name="cards"></param>
    private void MarkScoringCards(List<Card> cards)
    {
        foreach (var card in cards)
        {
            card.cardData.canScore = true;
        }
    }

    /// <summary>
    /// Update the Scoring Hand Type
    /// Called during play state
    /// </summary>
    /// <param name="state"></param>
    public void FinalizeHandType()
    {
        if (_roundManager.curState != RoundManager.State.Play) return;
        
        Debug.LogWarning("Finalizing");
        scoringHandType = curHand;
        
        IdentifyScoringCards(cardsInSelectionRef, scoringHandType);

    }
}
