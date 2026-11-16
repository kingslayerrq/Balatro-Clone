using TMPro;
using UnityEngine;

/// <summary>
/// Shows the current hand type, its chips x mults, and the final score of a played hand.
/// Display only: nothing in the game flow waits on this component.
/// </summary>
public class HandTypeVisualizer : Singleton<HandTypeVisualizer>
{
    private ScoreCalculator _scoreCalculator;

    [SerializeField] private TextMeshProUGUI handTypeName;
    [SerializeField] private TextMeshProUGUI handTypeLvl;
    [SerializeField] private TextMeshProUGUI handTypeBaseChips;
    [SerializeField] private TextMeshProUGUI handTypeBaseMults;
    [SerializeField] private TextMeshProUGUI handScore;
    [SerializeField] private GameObject handTypeObj;
    [SerializeField] private GameObject handTypeLvlObj;
    [SerializeField] private GameObject handScoreObj;

    private void Start()
    {
        _scoreCalculator = ScoreCalculator.Instance;
        if (!_scoreCalculator)
        {
            Debug.LogError("HandTypeVisualizer: no ScoreCalculator in the scene", this);
            return;
        }

        _scoreCalculator.HandTypeChanged += UpdateHandTypePanelVisuals;
        _scoreCalculator.ChipsChanged += UpdateChipsPanelVisuals;
        _scoreCalculator.MultsChanged += UpdateMultsPanelVisuals;
        _scoreCalculator.HandScoreShown += ShowHandScore;
        _scoreCalculator.HandScoreHidden += HideHandScore;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_scoreCalculator)
        {
            _scoreCalculator.HandTypeChanged -= UpdateHandTypePanelVisuals;
            _scoreCalculator.ChipsChanged -= UpdateChipsPanelVisuals;
            _scoreCalculator.MultsChanged -= UpdateMultsPanelVisuals;
            _scoreCalculator.HandScoreShown -= ShowHandScore;
            _scoreCalculator.HandScoreHidden -= HideHandScore;
        }
    }

    private void UpdateHandTypePanelVisuals(HandTypeConfig.HandType handType)
    {
        handTypeName.text = handType.handTypeName;
        handTypeLvl.text = handType.type == Enums.BasePokerHandType.None ? "" : "lvl. " + handType.lvl;
        handTypeBaseChips.text = handType.baseChips.FormatFloat();
        handTypeBaseMults.text = handType.baseMults.FormatFloat();
    }

    private void UpdateChipsPanelVisuals(double chips)
    {
        handTypeBaseChips.text = chips.FormatDouble();
    }

    private void UpdateMultsPanelVisuals(double mults)
    {
        handTypeBaseMults.text = mults.FormatDouble();
    }

    /// <summary>
    /// Swap the hand type panel for the calculated score of the played hand
    /// </summary>
    private void ShowHandScore(double score)
    {
        if (handTypeObj) handTypeObj.SetActive(false);
        if (handTypeLvlObj) handTypeLvlObj.SetActive(false);
        if (handScoreObj) handScoreObj.SetActive(true);
        if (handScore) handScore.text = score.FormatDouble();
    }

    private void HideHandScore()
    {
        if (handScoreObj) handScoreObj.SetActive(false);
        if (handTypeObj) handTypeObj.SetActive(true);
        if (handTypeLvlObj) handTypeLvlObj.SetActive(true);
    }
}
