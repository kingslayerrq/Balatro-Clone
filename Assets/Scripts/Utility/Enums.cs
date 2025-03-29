using UnityEngine;

public class Enums
{
    public enum CardSuit
    {
        Spade,
        Heart,
        Club,
        Diamond,
        None
    }

    public enum Edition
    {
        Basic,
        Foil,
        Holographic,
        Polychrome,
        Negative
    }

    public enum Seal
    {
        Gold,
        Red,
        Blue,
        Purple,
        None
    }

    public enum PanelType
    {
        Hand,
        Joker,
        Consumable,
        Deck,
        Basic
    }

    public enum CardCompareType
    {
        Rank,
        Suit
    }
}
