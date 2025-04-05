using System;
using UnityEngine;

public class CardState : MonoBehaviour
{

    public enum CardStatus
    {
        [Tooltip("Offscreen")]
        InCreation,
        [Tooltip("In Draw Panel")]
        InDeck,
        [Tooltip("In Hand Panel")]
        InHand,
        [Tooltip("In Played Panel for ready to be scored")]
        InPlayed,
        [Tooltip("Offscreen, just for reference")]
        InUsed
    }
}
