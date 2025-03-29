using System;
using UnityEngine;
using UnityEngine.UI;


public class CardData : MonoBehaviour, IComparable
{
    public BaseCardParameters baseCardParameters;

    [Header("References")] 
    private Card _card;
    
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
        _card = GetComponent<Card>();
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
               this._card.GetParentIndex().CompareTo(other._card.GetParentIndex());
    }
    public int SortBySuit(CardData other)
    {
        int suitComparison = this.suit.CompareTo(other.suit);
        if (suitComparison != 0)
        {
            return suitComparison;
        }
        
        return this.rank.CompareTo(other.rank) != 0 ? this.rank.CompareTo(other.rank) : 
            this._card.GetParentIndex().CompareTo(other._card.GetParentIndex());
    }
}
