using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(CardData))]
public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler,
    IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    #region Card Variables
    
    public Panel curPanel;
    private Canvas _canvas;
    private Image _image;
    public CardData cardData = null;

    [Header("Panels")] 
    [SerializeField] private HandPanel handPanel;
    [SerializeField] private JokerPanel jokerPanel;
    [SerializeField] private ConsumablePanel consumablePanel;
    [SerializeField] private PlayedCardPanel playedCardPanel;
    
    [Header("Drag Debug")]
    private Vector2 _pointerOffset;
    public bool isDragging = false;
    public bool wasDragged = false;
    
    [Header("Selected Settings")]
    public bool isSelected = false;
    public float selectedOffset = 50f;
    [SerializeField] private float selectTimerThreshold = 0.2f;
    private float _pointerDownTime;
    private float _pointerUpTime;

    [Header("Hover Settings")] 
    public bool isHovering = false;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeedLimit = 50f;

    [Header("Card Visuals")]
    public GameObject cvPrefab;
    public CardVisuals cardVisuals;
    public Transform cardVisualizer;
    
    [Header("Events")]
    [HideInInspector] public static UnityEvent<Card, Panel> pointerEnterEvent = new UnityEvent<Card, Panel>();
    [HideInInspector] public static UnityEvent<Card, Panel> pointerExitEvent = new UnityEvent<Card, Panel>();
    [HideInInspector] public static UnityEvent<Card, bool, Panel> pointerUpEvent = new UnityEvent<Card, bool, Panel>();
    [HideInInspector] public static UnityEvent<Card, Panel> pointerDownEvent = new UnityEvent<Card, Panel>();
    [HideInInspector] public static UnityEvent<Card, Panel> beginDragEvent = new UnityEvent<Card, Panel>();
    [HideInInspector] public static UnityEvent<Card, Panel> endDragEvent = new UnityEvent<Card, Panel>();
    [HideInInspector] public static UnityEvent<Card, bool, Panel> selectEvent = new UnityEvent<Card, bool, Panel>();
    [HideInInspector] public static UnityEvent<Card, Panel> reparentPanelEvent = new UnityEvent<Card, Panel>();
    
    #endregion
    private void Start()
    {
        curPanel = GetComponentInParent<Panel>();
        cardData = GetComponent<CardData>();
        cardVisualizer = FindFirstObjectByType<CardVisualizer>().transform;
        if (cardData == null) return;
  
        
        // Initialize
        this.name = cardData.id;
        _canvas = GetComponentInParent<Canvas>();
        _image = GetComponent<Image>();
        cardVisuals = Instantiate(cvPrefab, cardVisualizer ? cardVisualizer : _canvas.transform).GetComponent<CardVisuals>();
        cardVisuals.name = cardData.id;
        cardVisuals.Init(this);

        handPanel = HandPanel.Instance;
        consumablePanel = ConsumablePanel.Instance;
        jokerPanel = JokerPanel.Instance;
        playedCardPanel = PlayedCardPanel.Instance;
        
        // Events
        if (handPanel != null)
        {
            Debug.Log("Added" + handPanel);
            handPanel.playHandEvent.AddListener(Played);
        }
    }

    private void Update()
    {
        ClampPosition();
        if (isDragging)
        {
            Vector2 targetPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - _pointerOffset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }

    #region General Card Methods
    
    private void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }
    
    /// <summary>
    /// Get the parent card slot's sibling index
    /// </summary>
    public int GetParentIndex()
    {
        return this.transform.parent.GetSiblingIndex();
    }

    public float NormalizedPosition()
    {
        return ExtensionMethods.Remap((float)GetParentIndex(), 0, (float)(curPanel.transform.childCount - 1),
            0, 1);
    }

    #endregion

    #region Interactable Methods

    public void OnDrag(PointerEventData eventData)
    {
        
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        beginDragEvent.Invoke(this, curPanel);
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pointerOffset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        _canvas.GetComponent<GraphicRaycaster>().enabled = false;
        _image.raycastTarget = false;
        wasDragged = true;
    }
    

    public void OnEndDrag(PointerEventData eventData)
    {
        endDragEvent.Invoke(this, curPanel);
        isDragging = false;
        _canvas.GetComponent<GraphicRaycaster>().enabled = true;
        _image.raycastTarget = true;

        StartCoroutine(FrameWait());
        
        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;         // not updating wasDragged immediately
        }
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerEnterEvent.Invoke(this, curPanel);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerExitEvent.Invoke(this, curPanel);
        isHovering = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!curPanel || eventData.button != PointerEventData.InputButton.Left) return;

        _pointerUpTime = Time.time;
        
        // shortPress: selection
        // longPress: dragging
        bool shortPress = (_pointerUpTime - _pointerDownTime) <= selectTimerThreshold;
        pointerUpEvent.Invoke(this, shortPress, curPanel);
        
        // cant select if at max selection count (eg: cannot select 6 cards in a poker hand)
        if (!isSelected && (curPanel.numOfSelection >= curPanel.maxSelectionSize)) return;
        
        // Dont trigger selection if it was dragged
        if (shortPress && !wasDragged)
        {
            isSelected = !isSelected;
            selectEvent.Invoke(this, isSelected, curPanel);
            if (isSelected)
            {
                this.transform.localPosition += cardVisuals.transform.up * selectedOffset;
            }
            else
            {
                this.transform.localPosition = Vector3.zero;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!curPanel || eventData.button != PointerEventData.InputButton.Left) return;

        pointerDownEvent.Invoke(this, curPanel);
        _pointerDownTime = Time.time;
    }
    
    #endregion

    private void Played(Card card, Panel targetPanel)
    {
        // only respond if invoked on self
        if (card == this)
        {
            isSelected = false;
            this.curPanel = targetPanel;
            reparentPanelEvent.Invoke(this, targetPanel);
            
            // stop calculation of curve positioning when parenting to different panel
            this.cardVisuals.stopFollow = true;

            StartCoroutine(StopFollow());
            
        }

        IEnumerator StopFollow()
        {
            yield return new WaitForEndOfFrame();
            this.cardVisuals.stopFollow = false;
        }
    }
    
}
