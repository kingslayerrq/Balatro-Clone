using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HandPanel : Panel
{
    public static HandPanel Instance;
    [SerializeField] private Button playHandButton;
    [HideInInspector] public UnityEvent<Card, Panel> playHandEvent = new UnityEvent<Card, Panel>();

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
        
    }

    /// <summary>
    /// Invoke PlayHand event
    /// Clear the selection list
    /// </summary>
    public void PlayHand()
    {
        foreach (var card in cardsInSelection)
        {
            Debug.LogWarning("Invoked" + card.name);
            playHandEvent.Invoke(card, PlayedCardPanel.Instance);
            cardsInPanel.Remove(card);
        }
        
        // clear hand
        cardsInSelection.Clear();
        numOfSelection = 0;
        playHandButton.interactable = false;
    }
    
    public void SortByRank()
    {
        // Debug.Log("Prev Sort by Rank: " + cardsInPanel);
        cardsInPanel.Sort((card1, card2) => card1.GetComponent<CardData>().SortByRank(card2.GetComponent<CardData>()));
        // Debug.Log("After Sort by Rank: " + cardsInPanel);
        for (int i = 0; i < cardsInPanel.Count; i++)
        {
            cardsInPanel[i].transform.parent.SetSiblingIndex(i);
            cardsInPanel[i].cardVisuals.UpdateIndex(cardsInPanel.Count);
        }
    }

    public void SortBySuit()
    {
        // Debug.Log("Prev Sort by Suit: " + cardsInPanel);
        cardsInPanel.Sort((card1, card2) => card1.GetComponent<CardData>().SortBySuit(card2.GetComponent<CardData>()));
        // Debug.Log("After Sort by Suit: " + cardsInPanel);
        for (int i = 0; i < cardsInPanel.Count; i++)
        {
            cardsInPanel[i].transform.parent.SetSiblingIndex(i);
            cardsInPanel[i].cardVisuals.UpdateIndex(cardsInPanel.Count);
        }
    }

    protected override void SelectCard(Card card, bool isSelected, Panel panel)
    {
        base.SelectCard(card, isSelected, panel);
        playHandButton.interactable = numOfSelection > 0 ? true : false;
    }
}