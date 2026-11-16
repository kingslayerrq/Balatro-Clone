public static class CardState
{
    public enum CardStatus
    {
        // Offscreen, in the Deck Panel (run deck, outside of a round)
        InCreation,
        // In Draw Panel
        InDeck,
        // In Hand Panel
        InHand,
        // In Played Panel, ready to be scored
        InPlayed,
        // In Used Panel (discarded or already played this round)
        InUsed
    }
}
