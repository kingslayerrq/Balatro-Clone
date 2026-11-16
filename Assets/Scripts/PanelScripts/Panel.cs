using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// A pile of cards (hand, draw pile, played cards...). The panel owns its card lists:
/// cards enter and leave only through <see cref="Card.MoveTo"/>, which calls Add / Remove here.
/// </summary>
[RequireComponent(typeof(LayoutGroup))]
public abstract class Panel: MonoBehaviour
{
    public PanelParameters panelParameters;

    [Header("Panel Parameters")]
    public Enums.PanelType panelType;
    public int maxSize;
    public int maxSelectionSize;
    [Tooltip("Can player adjust cards indexes in this panel?")]
    public bool allowSwap = true;
    [Tooltip("Should the cards be curved in this panel")]
    public bool allowCurve = true;
    public bool allowDrag = true;
    public bool allowSelect = true;
    [SerializeField] private float cardReturnTransition = 0.15f;

    [Header("Panel Info")]
    private RectTransform _rect;
    public List<Card> cardsInPanel = new List<Card>();
    public List<Card> cardsInSelection = new List<Card>();
    [Tooltip("Card currently being dragged in this panel")]
    [SerializeField] private Card highlightedCard = null;

    public int numOfSelection => cardsInSelection.Count;

    protected virtual void Awake()
    {
        _rect = GetComponent<RectTransform>();
        if (panelParameters != null)
        {
            InitPanel(panelParameters);
        }
        else
        {
            Debug.LogError($"{name}: panelParameters is not assigned", this);
        }
    }

    protected virtual void Start()
    {
    }

    protected virtual void OnDestroy()
    {
    }

    protected virtual void InitPanel(PanelParameters panelParameters)
    {
        panelType = panelParameters.panelType;
        allowSwap = panelParameters.allowSwap;
        maxSize = panelParameters.maxSize;
        maxSelectionSize = panelParameters.maxSelectionSize;
        cardReturnTransition = panelParameters.cardReturnTransition;
        allowCurve = panelParameters.allowCurve;
        allowDrag = panelParameters.allowDrag;
    }

    protected virtual void Update()
    {
        // only a card being dragged can change the order
        if (!allowSwap || highlightedCard == null) return;

        for (int i = 0; i < cardsInPanel.Count; i++)
        {
            if (highlightedCard.transform.position.x < cardsInPanel[i].transform.position.x &&
                highlightedCard.GetParentIndex() > cardsInPanel[i].GetParentIndex())
            {
                Swap(i);
                break;
            }
            if (highlightedCard.transform.position.x > cardsInPanel[i].transform.position.x &&
                highlightedCard.GetParentIndex() < cardsInPanel[i].GetParentIndex())
            {
                Swap(i);
                break;
            }
        }
    }

    // Reparent instead of changing sibling index directly
    protected virtual void Swap(int ind)
    {
        var other = cardsInPanel[ind];
        Transform selectedParent = highlightedCard.transform.parent;
        Transform swappingParent = other.transform.parent;
        other.transform.SetParent(selectedParent);
        other.transform.localPosition = other.IsRaised ? new Vector3(0f, other.selectedOffset, 0f) : Vector3.zero;
        highlightedCard.transform.SetParent(swappingParent);

        if (other.cardVisuals == null) return;

        bool swapToRight = other.GetParentIndex() < highlightedCard.GetParentIndex();
        other.cardVisuals.Swap(swapToRight ? 1 : -1);

        RefreshVisualOrder();
    }

    /// <summary>
    /// Match every card visual's draw order to its slot order
    /// </summary>
    protected void RefreshVisualOrder()
    {
        foreach (Card card in cardsInPanel)
        {
            card.cardVisuals.UpdateIndex(this.transform.childCount);
        }
    }

    #region Card ownership (called by Card)

    /// <summary>
    /// Put a card (and its slot) into this panel. Use Card.MoveTo instead of calling this directly.
    /// </summary>
    public void Add(Card card)
    {
        card.curPanel = this;

        if (!cardsInPanel.Contains(card))
        {
            cardsInPanel.Add(card);
        }

        // the slot is what sits in the layout group, the card is its child
        card.transform.parent.SetParent(this.transform);
        // !!! IMPORTANT
        card.cardVisuals.curveRotationOffset = 0;
        card.cardVisuals.curveYOffset = 0;
        card.transform.localPosition = Vector3.zero;

        RefreshVisualOrder();
    }

    /// <summary>
    /// Take a card out of this panel's lists. Use Card.MoveTo instead of calling this directly.
    /// </summary>
    public void Remove(Card card)
    {
        if (highlightedCard == card) highlightedCard = null;
        cardsInPanel.Remove(card);
        if (cardsInSelection.Remove(card))
        {
            OnSelectionChanged();
        }
    }

    public void NotifyCardSelected(Card card, bool isSelected)
    {
        if (isSelected)
        {
            if (!cardsInSelection.Contains(card)) cardsInSelection.Add(card);
        }
        else
        {
            cardsInSelection.Remove(card);
        }

        OnSelectionChanged();
    }

    public void NotifyBeginDrag(Card card)
    {
        highlightedCard = card;
    }

    public void NotifyEndDrag(Card card)
    {
        // snap back local position
        card.transform.DOLocalMove(card.IsRaised
            ? new Vector3(0, card.selectedOffset, 0)
            : Vector3.zero, cardReturnTransition).SetEase(Ease.OutBack);

        // slots may have been reordered while dragging
        LayoutRebuilder.MarkLayoutForRebuild(_rect);

        if (highlightedCard == card) highlightedCard = null;
    }

    /// <summary>
    /// Called whenever cardsInSelection changes
    /// </summary>
    protected virtual void OnSelectionChanged()
    {
    }

    #endregion
}
