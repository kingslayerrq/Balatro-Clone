using System;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardVisuals : MonoBehaviour
{
    #region CardVisuals Variables
    
    [Header("Card")]
    public Card parentCard = null;
    [SerializeField] private int indexInHand;
    [SerializeField] private bool isInitialized = false;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector3 movementDelta;
    [SerializeField] private Vector3 rotationDelta;

    [Header("References")] 
    private RunManager _runManager;
    [Tooltip("Separate parent obj from actual card")]
    [SerializeField] private Transform tiltParent;
    [SerializeField] private Transform shakeParent;
    public Image cardImage;
    public Color originalColor;
    public Transform cardShadow;
    private Canvas _shadowCanvas;
    private Vector2 _shadowDistance;
    [SerializeField] private float shadowOffset = 20f;
    public Vector3 originalScale;

    [Header("CardInfo Visuals")] public CardInfoVisual cardInfoVisual = null;

    [Header("Animation")] [SerializeField] private bool allowAnim = true;
    
    [Header("Movement and Rotation Settings")]
    [SerializeField] private float followSpeed = 50f;
    [SerializeField] private float rotationAmount = 20f;
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Tilt Settings")] 
    [SerializeField] private float autoTiltAmount = 15f;
    [SerializeField] private float manualTiltAmount = 30f;
    [SerializeField] private float tiltSpeed = 40f;
    
    [Header("Scale Parameters")]
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.1f;
    [SerializeField] private float scaleOnSelect = 1.2f;
    [SerializeField] private float scaleOnPunch = 0.5f;
    [SerializeField] private int scalePunchAmount = 0;
    [SerializeField] private float scaleTransition = 0.15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Select Parameters")]
    [SerializeField] private float selectPunchAmount = 20f;

    [Header("Hover Parameters")] 
    [SerializeField] private float hoverPunchAngle = 5f;
    [SerializeField] private float hoverTransition = 0.15f;

    [Header("Swap Parameters")] 
    [SerializeField] private bool swapAnimations = true;
    [SerializeField] private float swapRotationAngle = 20f;
    [SerializeField] private float swapTransition = 0.2f;
    [SerializeField] private int swapVibrato = 5;
    
    [Header("Curve")]
    [SerializeField] private CurveParameters curve;
    public float curveYOffset;
    public float curveRotationOffset;

    #endregion

    private void Start()
    {
        _shadowDistance = cardShadow.localPosition;
        _runManager = RunManager.Instance;
    }

    private void Update()
    {
        if (parentCard == null || !isInitialized) return;
        HandCurvePositioning();
        SmoothFollow();
        FollowRotation();
        CardTilt();
    }

    public void Init(Card target)
    {
        
        canvas = GetComponent<Canvas>();
        _shadowCanvas = cardShadow.GetComponent<Canvas>();
        parentCard = target;
        indexInHand = parentCard.GetParentIndex();
        originalScale = GetComponent<RectTransform>().localScale;
        //SetCardImage(target.);
        originalColor = cardImage.color;
        cardInfoVisual = GetComponentInChildren<CardInfoVisual>();
        
        if (cardInfoVisual == null) return;
        cardInfoVisual.Init(parentCard.cardData.id, parentCard.cardData.description);
        
        //Event Listening
        Card.pointerEnterEvent.AddListener(PointerEnter);
        Card.pointerExitEvent.AddListener(PointerExit);
        Card.beginDragEvent.AddListener(BeginDrag);
        Card.endDragEvent.AddListener(EndDrag);
        Card.pointerDownEvent.AddListener(PointerDown);
        Card.pointerUpEvent.AddListener(PointerUp);
        Card.selectEvent.AddListener(Select);
        Card.scoreEvent.AddListener(Score);
        Card.cardStatusUpdateEvent.AddListener(StatusUpdate);
        
        
        
        // Set true
        isInitialized = true;
    }

    #region CardVFX

    /// <summary>
    /// Curve Hand Panel Cards
    /// </summary>
    private void HandCurvePositioning()
    {
        if (!parentCard.curPanel.allowCurve) return;
        curveYOffset = (curve.positioning.Evaluate(parentCard.NormalizedPosition()) * curve.positioningInfluence) *
                       (parentCard.curPanel.cardsInPanel.Count - 1);
        curveYOffset = (parentCard.curPanel.cardsInPanel.Count - 1) < 5 ? 0 : curveYOffset;
        curveRotationOffset = curve.rotation.Evaluate((parentCard.NormalizedPosition()));
    }
    private void SmoothFollow()
    {
        // Omit Offset when dragging / stop followed / the panel disables curve
        bool omit = (parentCard.isDragging  || !parentCard.curPanel.allowCurve);
        Vector3 verticalOffset = Vector3.up * (omit ? 0 : curveYOffset);
        this.transform.position = Vector3.Lerp(this.transform.position, parentCard.transform.position + verticalOffset,
            followSpeed * Time.deltaTime);
    }

    private void FollowRotation()
    {
        Vector3 movement = (transform.position - parentCard.transform.position);
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
        Vector3 movementRotation = (parentCard.isDragging ? movementDelta : movement) * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));
    }

    private void CardTilt()
    {
        if (!allowAnim) return;
        // If dragging, preserve previous index, else update
        indexInHand = parentCard.isDragging ? indexInHand : parentCard.GetParentIndex();
        float sine = Mathf.Sin(Time.time + indexInHand) * (parentCard.isHovering ? 0.2f : 1f);
        float cosine = Mathf.Cos(Time.time + indexInHand) * (parentCard.isHovering ? 0.2f : 1f);

        Vector3 offset = this.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float tiltX = parentCard.isHovering ? (offset.y * -1) * manualTiltAmount : 0;
        float tiltY = parentCard.isHovering ? (offset.x) * manualTiltAmount : 0;
        float tiltZ = (parentCard.isDragging )
            ? tiltParent.eulerAngles.z
            : (curveRotationOffset * (curve.rotationInfluence * (parentCard.curPanel.cardsInPanel.Count - 1)));
        float lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + (sine * autoTiltAmount),
            tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + (cosine * autoTiltAmount),
            tiltSpeed * Time.deltaTime);
        float lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }
    
    #endregion

    #region Parent Card Event Handlers

    private void PointerEnter(Card card, Panel panel)
    {
        if (card != parentCard || panel != parentCard.curPanel) return;
        
        cardInfoVisual.ShowInfo();
        if (scaleAnimations)
        {
            this.transform.DOScale(scaleOnHover * originalScale, scaleTransition).SetEase(scaleEase);
        }
        
        DOTween.Kill(2, true);      // prevents repeatedly triggering punch rotation
        shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
    }

    private void PointerExit(Card card, Panel panel)
    {
        if (card != parentCard || panel != parentCard.curPanel) return;
        
        cardInfoVisual.HideInfo();
        if (scaleAnimations && !parentCard.wasDragged)
        {
            this.transform.DOScale(originalScale, scaleTransition).SetEase(scaleEase);
        }
    }

    private void BeginDrag(Card card, Panel panel)
    {
        if (card != parentCard || panel != parentCard.curPanel) return;
        if (scaleAnimations)
        {
            this.transform.DOScale(scaleOnSelect * originalScale, scaleTransition).SetEase(scaleEase);
        }
        
        
        canvas.overrideSorting = true;

    }

    private void EndDrag(Card card, Panel panel)
    {
        if (card != parentCard || panel != parentCard.curPanel) return;
        canvas.overrideSorting = false;
        this.transform.DOScale(originalScale, scaleTransition).SetEase(scaleEase);
    }

    private void PointerDown(Card card, Panel panel)
    {
        if (card != parentCard || panel != parentCard.curPanel) return;
  
        if (scaleAnimations)
        {
            this.transform.DOScale(scaleOnSelect * originalScale, scaleTransition).SetEase(scaleEase);
        }
        
        // show card on top
        canvas.overrideSorting = true;
        
        // sink down shadow
        cardShadow.localPosition += new Vector3(-1 * (1.5f / 2f * shadowOffset), -1 * shadowOffset);
        _shadowCanvas.overrideSorting = false;
    }

    private void PointerUp(Card card, bool shortPress, Panel panel)
    {
        if (card != parentCard || panel != parentCard.curPanel) return;
        if (scaleAnimations)
        {
            this.transform.DOScale(shortPress ? scaleOnSelect * originalScale : scaleOnHover * originalScale, scaleTransition).SetEase(scaleEase);
        }
        

        canvas.overrideSorting = false;
        // reset shadow
        cardShadow.localPosition = _shadowDistance;
        _shadowCanvas.overrideSorting = true;
    }

    private void Select(Card card, bool isSelected, Panel panel)
    {
        if (card != parentCard || panel != parentCard.curPanel) return;
        cardInfoVisual.HideInfo();
        DOTween.Kill(2, true);      // prevents repeatedly triggering punch rotation
        float dir = isSelected ? 1 : 0;     // Pop up or down
        shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1).OnComplete((() =>
        {
            if (panel == HandPanel.Instance)
            {
                cardInfoVisual.ShowInfo();
            }
        }));
        shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20, 1).SetId(2);

        if (scaleAnimations)
        {
            this.transform.DOScale(scaleOnHover * originalScale, scaleTransition).SetEase(scaleEase);
        }
       
    }


    private void Score(Card card)
    {
        if (card != parentCard) return;
        DOTween.Kill(2, true);

        shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20, 1).SetId(2);
        if (scaleAnimations)
        {
            shakeParent.DOPunchScale(Vector3.one * scaleOnPunch, 0.1f, scalePunchAmount, 1);
        }
    }

    private void StatusUpdate(Card card, CardState.CardStatus status)
    {
        if (card != parentCard) return;
        switch (status)
        {
            case CardState.CardStatus.InCreation:
                allowAnim = false;
                SetCardImage(RunManager.Instance.deck.deckSprite);
                break;
            case CardState.CardStatus.InDeck:
                allowAnim = false;
                SetCardImage(RunManager.Instance.deck.deckSprite);
                break;
            case CardState.CardStatus.InHand:
                allowAnim = true;
                SetCardImage(parentCard.cardData.sprite);
                break;
            case CardState.CardStatus.InUsed:
                allowAnim = false;
                SetCardImage(parentCard.cardData.sprite);
                break;
            default:
                break;
        }
    }
    #endregion

    private void SetCardImage(Sprite sprite)
    {
        if (cardImage.sprite == sprite) return;
        cardImage.sprite = sprite;
    }
    public void Swap(float dir = 1)
    {
        if (swapAnimations)
        {
            DOTween.Kill(2, true);
            shakeParent.DOPunchRotation(Vector3.forward * (dir * swapRotationAngle), swapTransition, swapVibrato, 1)
                .SetId(3);
            
        }
    }

    public void UpdateIndex(int length)
    {
        this.transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    }
}
