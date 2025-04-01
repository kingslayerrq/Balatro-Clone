using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckParameters", menuName = "Scriptable Objects/DeckParameters")]
public class DeckParameters : ScriptableObject
{
    public string deckName;
    public List<BaseCardParameters> cardsConfig = new List<BaseCardParameters>();
    public int hands = 4;
    public int discards = 3;
    public int money = 0;
    public Sprite deckSprite;

    public Deck Create()
    {
        return new Deck
        {
            deckName = deckName,
            cardsConfig = cardsConfig,
            hands = hands,
            money = money,
            discards = discards,
            deckSprite = deckSprite
        };
    }
    
    [Serializable]
    public class Deck
    {
        public string deckName;
        public List<BaseCardParameters> cardsConfig = new List<BaseCardParameters>();
        public int hands;
        public int discards;
        public int money;
        public Sprite deckSprite;
        public List<Card> cards = new List<Card>();
    }
}
