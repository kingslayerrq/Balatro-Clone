using System;
using UnityEngine;
using UnityEngine.UI;


public class CardData : MonoBehaviour
{
    public BaseCardParameters baseCardParameters;

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
}
