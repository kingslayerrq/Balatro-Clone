using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// The visible part of a card. Follows its parent <see cref="Card"/> (the invisible, interactable
/// object inside a panel slot) and plays every hover / select / score animation.
/// The parent card calls the On... methods directly.
/// </summary>
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

    [Header("Shader Material")]
    [SerializeField] private ShaderCode cardBaseShader;
    [SerializeField] private ShaderCode cardRnsShader;

    [Tooltip("Separate parent obj from actual card")]
    [SerializeField] private Transform tiltParent;
    [SerializeField] private Transform shakeParent;
    public Image cardBaseImage;
    public Image cardImage;
    public Color originalColor;
    public Transform cardShadow;
    private Canvas _shadowCanvas;
    private Vector2 _shadowDistance;
    private Camera _camera;
    [SerializeField] private float shadowOffset = 20f;
    public Vector3 originalScale;

    [Header("CardInfo Visuals")] public CardInfoVisual cardInfoVisual = null;

    [Header("Animation")]
    [SerializeField] private bool allowTilt = true;

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
    public bool swapAnimations = true;
    [SerializeField] private float swapRotationAngle = 20f;
    [SerializeField] private float swapTransition = 0.2f;
    [SerializeField] private int swapVibrato = 5;

    [Header("Curve")]
    [SerializeField] private CurveParameters curve;
    [HideInInspector] public float curveYOffset;
    [HideInInspector] public float curveRotationOffset;

    #endregion

    private void Update()
    {
        if (parentCard == null || !isInitialized) return;
        HandCurvePositioning();
        SmoothFollow();
        FollowRotation();
        CardTilt();
    }

    private void OnDestroy()
    {
        // tweens must not outlive the objects they animate
        DOTween.Kill(this);
        transform.DOKill();
        if (shakeParent) shakeParent.DOKill();
    }

    public void Init(Card target)
    {
        canvas = GetComponent<Canvas>();
        _shadowCanvas = cardShadow.GetComponent<Canvas>();
        _shadowDistance = cardShadow.localPosition;
        _camera = Camera.main;
        parentCard = target;
        indexInHand = parentCard.GetParentIndex();
        originalScale = GetComponent<RectTransform>().localScale;
        originalColor = cardImage.color;
        cardInfoVisual = GetComponentInChildren<CardInfoVisual>();

        if (cardInfoVisual == null)
        {
            Debug.LogError($"{name}: CardVisual prefab has no CardInfoVisual child", this);
            return;
        }
        cardInfoVisual.Init(parentCard.cardData.id, parentCard.cardData.description);

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
        if (!allowTilt) return;
        // If dragging, preserve previous index, else update
        indexInHand = parentCard.isDragging ? indexInHand : parentCard.GetParentIndex();
        float sine = Mathf.Sin(Time.time + indexInHand) * (parentCard.isHovering ? 0.2f : 1f);
        float cosine = Mathf.Cos(Time.time + indexInHand) * (parentCard.isHovering ? 0.2f : 1f);

        Vector3 offset = this.transform.position - _camera.ScreenToWorldPoint(Input.mousePosition);
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

    #region Parent Card Callbacks

    public void OnPointerEnter()
    {
        if (!isInitialized) return;

        if (parentCard.status == CardState.CardStatus.InHand)
        {
            cardInfoVisual.ShowInfo();
        }
        if (scaleAnimations)
        {
            ScaleTo(scaleOnHover);
        }

        PunchRotation(hoverPunchAngle);
    }

    public void OnPointerExit()
    {
        if (!isInitialized) return;

        cardInfoVisual.HideInfo();
        if (scaleAnimations && !parentCard.wasDragged)
        {
            ScaleTo(1f);
        }
    }

    public void OnBeginDrag()
    {
        if (!isInitialized) return;
        if (scaleAnimations)
        {
            ScaleTo(scaleOnSelect);
        }

        canvas.overrideSorting = true;
    }

    public void OnEndDrag()
    {
        if (!isInitialized) return;
        canvas.overrideSorting = false;
        ScaleTo(1f);
    }

    public void OnPointerDown()
    {
        if (!isInitialized) return;

        if (scaleAnimations)
        {
            ScaleTo(scaleOnSelect);
        }

        // show card on top
        canvas.overrideSorting = true;

        // sink down shadow
        cardShadow.localPosition += new Vector3(-1 * (1.5f / 2f * shadowOffset), -1 * shadowOffset);
        _shadowCanvas.overrideSorting = false;
    }

    public void OnPointerUp(bool shortPress)
    {
        if (!isInitialized) return;
        if (scaleAnimations)
        {
            ScaleTo(shortPress ? scaleOnSelect : scaleOnHover);
        }


        canvas.overrideSorting = false;
        // reset shadow
        cardShadow.localPosition = _shadowDistance;
        _shadowCanvas.overrideSorting = true;
    }

    /// <summary>
    /// The parent card popped up (selected / highlighted for scoring) or back down
    /// </summary>
    public void OnSelect(bool isSelected)
    {
        if (!isInitialized) return;

        DOTween.Kill(this, true);      // prevents repeatedly triggering punch rotation
        float dir = isSelected ? 1 : 0;     // Pop up or down
        cardInfoVisual.HideInfo();
        shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1).OnComplete((() =>
        {
            // only show when pointer is still hovering
            if (parentCard.curPanel == HandPanel.Instance && parentCard.isHovering)
            {
                cardInfoVisual.ShowInfo();
            }
        }));
        if (isSelected)
        {
            shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20, 1).SetId(this);
        }

        if (scaleAnimations)
        {
            ScaleTo(scaleOnHover);
        }
    }

    /// <summary>
    /// One of the parent card's scoring actions just triggered
    /// </summary>
    public void OnScore()
    {
        if (!isInitialized) return;

        PunchRotation(hoverPunchAngle / 2);
        if (scaleAnimations)
        {
            shakeParent.DOPunchScale(Vector3.one * scaleOnPunch, 0.1f, scalePunchAmount, 1);
        }
    }

    /// <summary>
    /// The parent card moved to another pile: show the matching face
    /// </summary>
    public void OnStatusChanged(CardState.CardStatus status)
    {
        if (!isInitialized) return;

        var deckSprite = RunManager.Instance.Deck.deckSprite;
        SetCardBaseImage(parentCard.cardData.baseSprite);
        switch (status)
        {
            case CardState.CardStatus.InCreation:
                allowTilt = false;
                SetCardImage(deckSprite);
                HideEdition();
                cardImage.enabled = false;
                break;
            case CardState.CardStatus.InDeck:
                cardImage.enabled = true;
                allowTilt = false;
                SetCardImage(deckSprite);
                HideEdition();
                break;
            case CardState.CardStatus.InHand:
            case CardState.CardStatus.InPlayed:
                cardImage.enabled = true;
                allowTilt = true;
                SetCardImage(parentCard.cardData.rnsSprite);
                ShowEdition();
                break;
            case CardState.CardStatus.InUsed:
                allowTilt = true;
                SetCardImage(deckSprite);
                HideEdition();
                break;
        }
    }
    #endregion

    /// <summary>
    /// Tween to a multiple of the original scale, replacing any scale tween still running
    /// </summary>
    private void ScaleTo(float multiplier)
    {
        transform.DOKill();
        transform.DOScale(multiplier * originalScale, scaleTransition).SetEase(scaleEase);
    }

    /// <summary>
    /// Punch rotation that restarts instead of stacking. Tweens are tagged with this card, not a global id.
    /// </summary>
    private void PunchRotation(float angle)
    {
        DOTween.Kill(this, true);
        shakeParent.DOPunchRotation(Vector3.forward * angle, hoverTransition, 20, 1).SetId(this);
    }

    private void ShowEdition()
    {
        if (cardBaseShader == null || parentCard == null) return;

        cardBaseShader.SetEdition((int)parentCard.cardData.edition);
        cardRnsShader.SetEdition((int)parentCard.cardData.edition);
    }

    private void HideEdition()
    {
        if (cardBaseShader) cardBaseShader.SetEdition(0);
        if (cardRnsShader) cardRnsShader.SetEdition(0);
    }
    private void SetCardImage(Sprite sprite)
    {
        if (cardImage.sprite == sprite) return;
        cardImage.sprite = sprite;
    }
    private void SetCardBaseImage(Sprite sprite)
    {
        if (cardBaseImage.sprite == sprite) return;
        cardBaseImage.sprite = sprite;
    }
    public void Swap(float dir = 1)
    {
        if (swapAnimations)
        {
            DOTween.Kill(this, true);
            shakeParent.DOPunchRotation(Vector3.forward * (dir * swapRotationAngle), swapTransition, swapVibrato, 1)
                .SetId(3);

        }
    }

    public void UpdateIndex(int length)
    {
        this.transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    }
}
