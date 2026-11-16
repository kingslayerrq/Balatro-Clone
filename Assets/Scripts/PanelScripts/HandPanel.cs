using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandPanel : Panel
{
    public static HandPanel Instance;
    private RoundManager _roundManager;

    [Tooltip("Gap between each card moving to the played panel")]
    [SerializeField] private float playCardGap = 0.5f;

    [SerializeField] private Button playHandButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button sortRankButton;
    [SerializeField] private Button sortSuitButton;

    public float PlayCardGap => playCardGap;

    /// <summary>
    /// Raised whenever the selected cards change. The list is the live selection: do not modify or keep it.
    /// </summary>
    public event Action<IReadOnlyList<Card>> SelectionChanged;

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

        if (_roundManager)
        {
            _roundManager.StateChanged += OnRoundStateChanged;
        }
        else
        {
            Debug.LogError("HandPanel: no RoundManager in the scene", this);
        }

        RefreshButtons();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_roundManager) _roundManager.StateChanged -= OnRoundStateChanged;
        if (Instance == this) Instance = null;
    }

    private void OnRoundStateChanged(RoundManager.State state)
    {
        RefreshButtons();
    }

    protected override void OnSelectionChanged()
    {
        RefreshButtons();
        SelectionChanged?.Invoke(cardsInSelection);
    }

    /// <summary>
    /// Play / Discard are only available while waiting for the player with something selected
    /// </summary>
    private void RefreshButtons()
    {
        bool canAct = _roundManager && _roundManager.curState == RoundManager.State.Play &&
                      _roundManager.curRound != null && numOfSelection > 0;
        if (playHandButton) playHandButton.interactable = canAct && _roundManager.curRound.hands > 0;
        if (discardButton) discardButton.interactable = canAct && _roundManager.curRound.discards > 0;
    }

    /// <summary>
    /// Copy of the selection ordered left to right as shown on screen
    /// </summary>
    public List<Card> GetSelectionByPosition()
    {
        var cards = new List<Card>(cardsInSelection);
        cards.Sort((card1, card2) => card1.transform.position.x.CompareTo(card2.transform.position.x));
        return cards;
    }

    #region Hand Panel Button Func

    /// <summary>
    /// Play button: ask the round to play the selected cards
    /// </summary>
    public void PlayHand()
    {
        if (_roundManager) _roundManager.RequestPlay();
    }

    /// <summary>
    /// Discard button: ask the round to discard the selected cards
    /// </summary>
    public void DiscardSelected()
    {
        if (_roundManager) _roundManager.RequestDiscard();
    }

    public void SortByRank()
    {
        cardsInPanel.Sort((card1, card2) => card1.cardData.SortByRank(card2.cardData));
        ApplyListOrderToSlots();
    }

    public void SortBySuit()
    {
        cardsInPanel.Sort((card1, card2) => card1.cardData.SortBySuit(card2.cardData));
        ApplyListOrderToSlots();
    }

    private void ApplyListOrderToSlots()
    {
        for (int i = 0; i < cardsInPanel.Count; i++)
        {
            cardsInPanel[i].transform.parent.SetSiblingIndex(i);
        }
        RefreshVisualOrder();
    }

    #endregion
}
