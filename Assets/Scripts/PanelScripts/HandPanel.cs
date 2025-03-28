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

    protected override void SelectCard(Card card, bool isSelected, Panel panel)
    {
        base.SelectCard(card, isSelected, panel);
        playHandButton.interactable = numOfSelection > 0 ? true : false;
    }
}