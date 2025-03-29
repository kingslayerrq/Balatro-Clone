using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class CardData : MonoBehaviour, IComparable
{
    public BaseCardParameters baseCardParameters;

    [Header("References")] 
    public Card card;

    [Header("Score Parameters")] 
    [Tooltip("Can this card be scored at all")]
    public bool canScore = false;
    public bool wasScored = false;
    public UnityEvent<Card, bool> onScoreCheckEvent = new UnityEvent<Card, bool>();
    
    [Header("Card Data Parameters")]
    public string id;
    public Enums.CardSuit suit;
    public int rank;
    public Enums.Edition edition;
    public Enums.Seal seal;
    public float chips;
    public float mults;
    public int triggerCounts;
    public Sprite sprite;
    
    
    private void Awake()
    {
        InjectData(baseCardParameters);
    }

    private void Start()
    {
        card = GetComponent<Card>();
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
        sprite = cardParam.sprite;
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
        int rankComparison = this.rank.CompareTo(other.rank);
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
