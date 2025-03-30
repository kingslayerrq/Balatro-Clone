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

    public enum BasePokerHandType
    {
        None = 0,
        HighCard = 1,
        Pair = 2,
        TwoPair = 3,
        ThreeOfAKind = 4,
        Straight = 5,
        Flush = 6,
        FullHouse = 7,
        FourOfAKind = 8,
        StraightFlush = 9,
        RoyalFlush = 10,
        FiveOfAKind = 11,
        FlushHouse = 12,
        FlushFive = 13
    }
}
