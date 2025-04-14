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
    private RoundManager _roundManager;
    private UsedPanel _usedPanel;
    
    [SerializeField] private float playCardGap = 0.5f;
    
    [SerializeField] private Button playHandButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button sortRankButton;
    [SerializeField] private Button sortSuitButton;
    
    
    [Header("Hand Panel Specific Events")]
    [HideInInspector] public UnityEvent<List<Card>> onCardSelectionChangedEvent = new UnityEvent<List<Card>>();
    [HideInInspector] public UnityEvent<Card, Panel> playCardEvent = new UnityEvent<Card, Panel>();
    
    //[HideInInspector] 
    //[Tooltip("Events to trigger after all cards has been played")]// public UnityEvent<Panel> handPlayedEvent = new UnityEvent<Panel>();
    [HideInInspector] 
    [Tooltip("Events to trigger as the played button is pressed")] public UnityEvent<Panel> playHandEvent = new UnityEvent<Panel>();
    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
        
    }

    protected override void Start()
    {
        base.Start();
        _roundManager = RoundManager.Instance;
        _usedPanel = UsedPanel.Instance;

        if (_roundManager)
        {
            _roundManager.discardEndEvent.AddListener(OnDiscardEnd);
        }
       
    }

    protected override void UpdateSelectedCards()
    {
        // Check if the lists have different counts or if selection has changed
        bool selectionChanged = prevCardsInSelection.Count != cardsInSelection.Count;
    
        // If counts are the same, check if the contents are different
        if (!selectionChanged && cardsInSelection.Count > 0)
        {
            // Compare each card to see if selection has changed
            for (int i = 0; i < cardsInSelection.Count; i++)
            {
                if (!prevCardsInSelection.Contains(cardsInSelection[i]))
                {
                    selectionChanged = true;
                    break;
                }
            }
        }

        if (!selectionChanged) return;
        // Create a new list to avoid reference issues
        prevCardsInSelection = new List<Card>(cardsInSelection);
        var copy = new List<Card>(cardsInSelection);
        onCardSelectionChangedEvent?.Invoke(copy);
    }

    #region Hand Panel Button Func
    
    /// <summary>
    /// Invoke PlayHand event
    /// Clear the selection list
    /// </summary>
    public void PlayHand()
    {
        if (_roundManager.curState != RoundManager.State.Play) return;
        HandAnalyzer.Instance.FinalizeHandType();
        StartCoroutine(PlayCardCoroutine());

        IEnumerator PlayCardCoroutine()
        {
            playHandEvent?.Invoke(this);
            // Sort the card based on their x position
            cardsInSelection.Sort(((card1, card2) => card1.transform.position.x.CompareTo(card2.transform.position.x)));
            
            foreach (var card in cardsInSelection)
            {
                playCardEvent?.Invoke(card, PlayedCardPanel.Instance);
                cardsInPanel.Remove(card);
            
                yield return new WaitForSecondsRealtime(playCardGap);
            }
            // clear selection
            cardsInSelection.Clear();
            numOfSelection = 0;
            
            playHandButton.interactable = false;
            
            // trigger event after all cards has been played
            _roundManager.updateRoundStateEvent?.Invoke(RoundManager.State.OnPlayed);
            //handPlayedEvent?.Invoke(PlayedCardPanel.Instance);
        }
    }

    public void DiscardSelected()
    {
        if (_roundManager.curState != RoundManager.State.Play) return;
        // Sort the card based on their x position
        cardsInSelection.Sort(((card1, card2) => card1.transform.position.x.CompareTo(card2.transform.position.x)));
        _roundManager.updateRoundStateEvent?.Invoke(RoundManager.State.Discard);
    }
    public void SortByRank()
    {
        cardsInPanel.Sort((card1, card2) => card1.GetComponent<CardData>().SortByRank(card2.GetComponent<CardData>()));
        for (int i = 0; i < cardsInPanel.Count; i++)
        {
            cardsInPanel[i].transform.parent.SetSiblingIndex(i);
            cardsInPanel[i].cardVisuals.UpdateIndex(cardsInPanel.Count);
        }
    }

    public void SortBySuit()
    {
        cardsInPanel.Sort((card1, card2) => card1.GetComponent<CardData>().SortBySuit(card2.GetComponent<CardData>()));
        for (int i = 0; i < cardsInPanel.Count; i++)
        {
            cardsInPanel[i].transform.parent.SetSiblingIndex(i);
            cardsInPanel[i].cardVisuals.UpdateIndex(cardsInPanel.Count);
        }
    }

    #endregion

    /// <summary>
    /// When Round Manager finishes discarding, handpanel should clean up card lists
    /// </summary>
    /// <param name="round"></param>
    private void OnDiscardEnd(Round round)
    {
        if (round != _roundManager.curRound) return;

        foreach (var card in cardsInSelection)
        {
            cardsInPanel.Remove(card);
        }
        cardsInSelection.Clear();
        
        // Update buttons
        numOfSelection = 0;
        discardButton.interactable = false;
        playHandButton.interactable = false;
    }
    protected override void SelectCard(Card card, bool isSelected, Panel panel)
    {
        base.SelectCard(card, isSelected, panel);
        playHandButton.interactable = (numOfSelection > 0 && _roundManager.curRound.hands > 0) ? true : false;
        discardButton.interactable = (numOfSelection > 0 && _roundManager.curRound.discards > 0) ? true : false;
        
        // TODO: Check what triggered this after hand played

    }
    
    
}