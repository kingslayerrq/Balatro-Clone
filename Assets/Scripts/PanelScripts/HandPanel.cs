using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HandPanel : Panel
{
    public static HandPanel Instance;
    
    [SerializeField] private float playCardGap = 0.5f;
    
    [SerializeField] private Button playHandButton;
    [SerializeField] private Button sortRankButton;
    [SerializeField] private Button sortSuitButton;
    
    [Header("Hand Panel Specific Events")]
    [HideInInspector] public UnityEvent<List<Card>> onCardSelectionChangedEvent = new UnityEvent<List<Card>>();
    [HideInInspector] public UnityEvent<Card, Panel> playCardEvent = new UnityEvent<Card, Panel>();
    [HideInInspector] public UnityEvent<Panel> handPlayedEvent = new UnityEvent<Panel>();
    
    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
        
    }

    

    #region Hand Panel Button Func
    
    /// <summary>
    /// Invoke PlayHand event
    /// Clear the selection list
    /// </summary>
    public void PlayHand()
    {
        StartCoroutine(PlayCardCoroutine());

        IEnumerator PlayCardCoroutine()
        {
            // Sort the card based on their x position
            cardsInSelection.Sort(((card1, card2) => card1.transform.position.x.CompareTo(card2.transform.position.x)));
            
            foreach (var card in cardsInSelection)
            {
                playCardEvent.Invoke(card, PlayedCardPanel.Instance);
                cardsInPanel.Remove(card);
            
                yield return new WaitForSecondsRealtime(playCardGap);
            }
            // clear hand
            cardsInSelection.Clear();
            numOfSelection = 0;
            
            playHandButton.interactable = false;
            
            // trigger event after all cards has been played
            handPlayedEvent?.Invoke(PlayedCardPanel.Instance);
        }
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

    #endregion


    protected override void SelectCard(Card card, bool isSelected, Panel panel)
    {
        base.SelectCard(card, isSelected, panel);
        playHandButton.interactable = numOfSelection > 0 ? true : false;
        
        // TODO: Check what triggered this after hand played
        // Trigger Card Analyzer ONLY when triggered in hand
        if (card.curPanel == this)
        { 
            onCardSelectionChangedEvent.Invoke(cardsInSelection);
        }
    }
}