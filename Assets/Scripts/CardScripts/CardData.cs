using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class CardData : MonoBehaviour, IComparable
{
    [Header("References")] 
    public Card card;

    [Header("Score Parameters")] 
    [Tooltip("Can this card be scored")]
    public bool canScore = false;
    [Tooltip("Has the card been scored")]
    public bool wasScored = false;
    [Tooltip("Whether the card is being highlighted and ready to be scored")]
    public bool isScoring = false;
    
    [HideInInspector] public UnityEvent<Card, bool> onScoreCheckEvent = new UnityEvent<Card, bool>();
    
    [Header("Card Data Parameters")]
    public string id;
    public string description;
    public Enums.CardSuit suit;
    public int rank;
    public Enums.Edition edition;
    public Enums.Seal seal;
    public float chips;
    public float mults;
    public int triggerCounts;
    public Sprite rnsSprite;
    [Tooltip("Base background of card, subject to enhancement changes")]
    public Sprite baseSprite;
    public List<CardActionConfig> cardActionConfigs;
    public List<BaseActionConfig.BaseAction> cardActions = new List<BaseActionConfig.BaseAction>();
    
    
    public void Init()
    {
        card = GetComponent<Card>();
        if (card.baseCardParameters == null)
        {
            Debug.LogWarning("Warning");
        }
        else
        {
            InjectData(card.baseCardParameters);
        }
    }
    private void InjectData(BaseCardParameters cardParam)
    {
        id = cardParam.cardID;
        suit = cardParam.suit;
        rank = cardParam.rank;
        edition = cardParam.edition;
        seal = cardParam.seal;
        chips = cardParam.baseChip;
        mults = cardParam.baseMult;
        triggerCounts = cardParam.triggerCounts;
        rnsSprite = cardParam.sprite;
        baseSprite = cardParam.cardBaseSprite;
        cardActionConfigs = cardParam.cardActionConfigs;
        
        description = $"<color=#0097FE>+{rank}</color> chips";
        foreach (var config in cardActionConfigs)
        {
            // Instantiate Actions
            cardActions.Add(config.Create());
        }
    }
    
    

    #region Sort Methods

    

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;
        CardData other = obj as CardData;
        if (other != null)
        {
            return SortByRank(other);
        }
        throw new ArgumentException("Object is not a CardData");
    }

    public int SortByRank(CardData other)
    {
        int rankComparison = other.rank.CompareTo(this.rank);
        if (rankComparison != 0)
        {
            return rankComparison;
        }

        return this.suit.CompareTo(other.suit) != 0 ? this.suit.CompareTo(other.suit) : 
               this.card.GetParentIndex().CompareTo(other.card.GetParentIndex());
    }
    public int SortBySuit(CardData other)
    {
        int suitComparison = this.suit.CompareTo(other.suit);
        if (suitComparison != 0)
        {
            return suitComparison;
        }
        
        return this.rank.CompareTo(other.rank) != 0 ? this.rank.CompareTo(other.rank) : 
            this.card.GetParentIndex().CompareTo(other.card.GetParentIndex());
    }
    #endregion
    
}

