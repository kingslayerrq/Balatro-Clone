using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CardData))]
[RequireComponent(typeof(CardScore))]
public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler,
    IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    #region Card Variables

    public BaseCardParameters baseCardParameters = null;
    [Tooltip("Panel this card currently belongs to. Change it with MoveTo")]
    public Panel curPanel;
    private Canvas _canvas;
    private GraphicRaycaster _raycaster;
    private Image _image;
    private Camera _camera;
    public CardData cardData = null;
    public CardScore cardScore = null;
    public CardState.CardStatus status = CardState.CardStatus.InCreation;

    [Header("Drag Debug")]
    private Vector2 _pointerOffset;
    public bool isDragging = false;
    public bool wasDragged = false;

    [Header("Selected Settings")]
    public bool isSelected = false;
    public float selectedOffset = 50f;
    [SerializeField] private float selectTimerThreshold = 0.2f;
    private float _pointerDownTime;

    [Header("Hover Settings")]
    public bool isHovering = false;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeedLimit = 50f;


    [Header("Card Visuals")]
    public GameObject cvPrefab;
    public CardVisuals cardVisuals;
    public Transform cardVisualizer;

    /// <summary>
    /// Is the card popped up in its slot (selected by the player or highlighted for scoring)
    /// </summary>
    public bool IsRaised => isSelected || (cardData != null && cardData.isScoring);

    #endregion

    /// <summary>
    /// Set up the card from its baseCardParameters and spawn its visual
    /// </summary>
    /// <param name="visualsRoot">Parent for the card's visual object</param>
    public void Init(Transform visualsRoot)
    {
        if (baseCardParameters == null)
        {
            Debug.LogError($"{name}: baseCardParameters must be assigned before Init", this);
            return;
        }

        curPanel = GetComponentInParent<Panel>();
        cardData = GetComponent<CardData>();
        cardScore = GetComponent<CardScore>();
        cardData.Init();
        name = cardData.id;

        _canvas = GetComponentInParent<Canvas>();
        _raycaster = _canvas.GetComponent<GraphicRaycaster>();
        _image = GetComponent<Image>();
        _camera = Camera.main;

        cardVisualizer = visualsRoot;
        cardVisuals = Instantiate(cvPrefab, visualsRoot ? visualsRoot : _canvas.transform).GetComponent<CardVisuals>();
        cardVisuals.name = cardData.id;
        cardVisuals.Init(this);
    }

    private void Update()
    {
        if (isDragging)
        {
            ClampPosition();
            Vector2 targetPosition = (Vector2)_camera.ScreenToWorldPoint(Input.mousePosition) - _pointerOffset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }

    #region General Card Methods

    /// <summary>
    /// The single way to move a card between panels.
    /// Removes it from its current panel, adds it to the target and updates its status and visuals.
    /// </summary>
    public void MoveTo(Panel panel, CardState.CardStatus newStatus)
    {
        if (panel == null)
        {
            Debug.LogError($"{name}: cannot move to a null panel", this);
            return;
        }

        if (isDragging) FinishDrag();
        transform.DOKill();
        if (curPanel != null) curPanel.Remove(this);

        // selection and scoring flags never carry over to another panel
        isSelected = false;
        cardData.canScore = false;
        cardData.isScoring = false;

        status = newStatus;
        panel.Add(this);
        cardVisuals.OnStatusChanged(newStatus);
    }

    /// <summary>
    /// Select / deselect the card in its current panel
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (isSelected == selected) return;
        isSelected = selected;
        ApplyRaise(selected);
        if (curPanel) curPanel.NotifyCardSelected(this, selected);
    }

    /// <summary>
    /// Pop the card up as one of the scoring cards of a played hand (or put it back down).
    /// Visual only: does not touch the panel's selection.
    /// </summary>
    public void SetScoringHighlight(bool highlighted)
    {
        if (cardData.isScoring == highlighted) return;
        cardData.canScore = highlighted;
        cardData.isScoring = highlighted;
        ApplyRaise(highlighted);
    }

    private void ApplyRaise(bool raised)
    {
        transform.localPosition = raised
            ? transform.localPosition + cardVisuals.transform.up * selectedOffset
            : Vector3.zero;
        cardVisuals.OnSelect(raised);
    }

    private void ClampPosition()
    {
        Vector2 screenBounds = _camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, _camera.transform.position.z));
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
        // required by Unity for OnBeginDrag / OnEndDrag to fire
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!curPanel || !curPanel.allowDrag) return;

        curPanel.NotifyBeginDrag(this);
        cardVisuals.OnBeginDrag();
        Vector2 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        _pointerOffset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        _raycaster.enabled = false;
        _image.raycastTarget = false;
        wasDragged = true;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        // nothing to end if the drag never started or was already cancelled by MoveTo
        if (!isDragging) return;
        FinishDrag();
    }

    private void FinishDrag()
    {
        isDragging = false;
        _raycaster.enabled = true;
        _image.raycastTarget = true;
        if (curPanel) curPanel.NotifyEndDrag(this);
        cardVisuals.OnEndDrag();

        StartCoroutine(FrameWait());

        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;         // not updating wasDragged immediately
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        cardVisuals.OnPointerEnter();
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cardVisuals.OnPointerExit();
        isHovering = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!curPanel || eventData.button != PointerEventData.InputButton.Left) return;

        // shortPress: selection
        // longPress: dragging
        bool shortPress = (Time.time - _pointerDownTime) <= selectTimerThreshold;
        cardVisuals.OnPointerUp(shortPress);

        // cant select if at max selection count (eg: cannot select 6 cards in a poker hand)
        if (!isSelected && (curPanel.numOfSelection >= curPanel.maxSelectionSize)) return;

        // Dont trigger selection if it was dragged
        if (shortPress && !wasDragged)
        {
            SetSelected(!isSelected);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!curPanel || eventData.button != PointerEventData.InputButton.Left) return;

        cardVisuals.OnPointerDown();
        _pointerDownTime = Time.time;
    }

    #endregion

    public override string ToString()
    {
        return $"Card: {this.name} ";
    }
}
