using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutGroup))]
public abstract class Panel: MonoBehaviour
{
    public PanelParameters panelParameters;
    
    [Header("Panel Parameters")]
    //public static Panel Instance;
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
    public List<Card> cardsInPanel;
    public List<Card> cardsInSelection;
    public int numOfSelection = 0;
    [Tooltip("Is there 2 cards swapping?")]
    private bool _isSwapping = false;
    [SerializeField] private Card hoverCard = null;
    [SerializeField] private Card highlightedCard = null;
    
    protected virtual void Awake()
    {
        
    }

    protected virtual void Start()
    {
        InitPanel(panelParameters);
        
        cardsInPanel = this.transform.GetComponentsInChildren<Card>().ToList();
        _rect = GetComponent<RectTransform>();
        
        // register listener
        Card.pointerEnterEvent.AddListener(PointerEnter);
        Card.reparentPanelEvent.AddListener(OnParent);
        Card.pointerExitEvent.AddListener(PointerExit);
        Card.beginDragEvent.AddListener(BeginDrag);
        Card.endDragEvent.AddListener(EndDrag);
        Card.selectEvent.AddListener(SelectCard);
        

        StartCoroutine(Frame());
        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            foreach (Card card in cardsInPanel)
            {
                card.cardVisuals.UpdateIndex(this.transform.childCount);
            }
        }
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
        if (_isSwapping || !allowSwap) return;
        if (highlightedCard != null)
        {
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
    }

    // Reparent instead of changing sibling index directly
    protected virtual void Swap(int ind)
    {
        _isSwapping = true;
        Transform selectedParent = highlightedCard.transform.parent;
        Transform swappingParent = cardsInPanel[ind].transform.parent;
        cardsInPanel[ind].transform.SetParent(selectedParent);
        cardsInPanel[ind].transform.localPosition = cardsInPanel[ind].isSelected ? new Vector3(0f, cardsInPanel[ind].selectedOffset, 0f) : Vector3.zero;
        highlightedCard.transform.SetParent(swappingParent);
        _isSwapping = false;

        if (cardsInPanel[ind].cardVisuals == null) return;

        bool swapToRight = cardsInPanel[ind].GetParentIndex() < highlightedCard.GetParentIndex();
        cardsInPanel[ind].cardVisuals.Swap(swapToRight ? 1 : -1);

        // updated visual indexes
        foreach (Card card in cardsInPanel)
        {
            card.cardVisuals.UpdateIndex(this.transform.childCount);
        }

    }

    #region Card Event Handlers

    protected virtual void PointerEnter(Card card, Panel panel)
    {
        if (panel != this) return;
        hoverCard = card;
    }

    protected virtual void PointerExit(Card card, Panel panel)
    {

        if (panel != this) return;
        hoverCard = null;
    }

    protected virtual void BeginDrag(Card card, Panel panel)
    {   
        if (panel != this) return;
        highlightedCard = card;
    }

    protected virtual void EndDrag(Card card, Panel panel)
    {
        if (panel != this) return;
        if (highlightedCard)
        {
            // snap back local position
            highlightedCard.transform.DOLocalMove(highlightedCard.isSelected
                ? new Vector3(0, highlightedCard.selectedOffset, 0)
                : Vector3.zero, cardReturnTransition).SetEase(Ease.OutBack);
            
            // force refresh/update
            _rect.sizeDelta += Vector2.right;
            _rect.sizeDelta -= Vector2.right;

            highlightedCard = null;
        }
    }

    /// <summary>
    /// On select card event
    /// add/remove card to selection list
    /// </summary>
    /// <param name="card"></param>
    /// <param name="isSelected"></param>
    /// <param name="panel"></param>
    protected virtual void SelectCard(Card card, bool isSelected, Panel panel)
    {
        if (panel != this) return;
        if (isSelected)
        {
            cardsInSelection.Add(card);
            numOfSelection++;
        }
        else
        {
            cardsInSelection.Remove(card);
            numOfSelection--;
        }
        
    }

    protected virtual void OnParent(Card card, Panel panel)
    {
        if (panel != this) return;
        card.curPanel = this;

        if (!cardsInPanel.Contains(card))
        {
            cardsInPanel.Add(card);
        }
        
        card.transform.parent.SetParent(this.transform);
        card.isSelected = false;
        // !!! IMPORTANT
        card.cardVisuals.curveRotationOffset = 0;
        card.cardVisuals.curveYOffset = 0;
        card.transform.localPosition = Vector3.zero;

        foreach (var cd in panel.cardsInPanel)
        {
            cd.cardVisuals.UpdateIndex(this.transform.childCount);
        }
            
        
        
    }
    #endregion
}
