using System;
using TMPro;
using UnityEngine;

public class HandTypeVisualizer : MonoBehaviour
{
    public static HandTypeVisualizer Instance;
    
    private HandAnalyzer _handAnalyzer;
    private ScoreCalculator _scoreCalculator;

    [SerializeField] private TextMeshProUGUI handTypeName;
    [SerializeField] private TextMeshProUGUI handTypeLvl;
    [SerializeField] private TextMeshProUGUI handTypeBaseChips;
    [SerializeField] private TextMeshProUGUI handTypeBaseMults;
    [SerializeField] private TextMeshProUGUI handScore;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _scoreCalculator = ScoreCalculator.Instance;
        if (_scoreCalculator)
        {
            _scoreCalculator.UpdateHandTypeVisualEvent.AddListener(UpdateHandTypePanelVisuals);
            _scoreCalculator.UpdateChipsVisualEvent.AddListener(UpdateChipsPanelVisuals);
            _scoreCalculator.UpdateMultsVisualEvent.AddListener(UpdateMultsPanelVisuals);
            _scoreCalculator.UpdateScoreVisualEvent.AddListener(UpdateScorePanelVisuals);
        }


    }

    private void UpdateHandTypePanelVisuals(HandTypeConfig.HandType handType)
    {
        handTypeName.text = handType.handTypeName;
        handTypeLvl.text = handType.type == Enums.BasePokerHandType.None ? "" : "lvl. " + handType.lvl;
        handTypeBaseChips.text = handType.baseChips.ToString();
        handTypeBaseMults.text = handType.baseMults.ToString();

    }

    private void UpdateChipsPanelVisuals(float chips)
    {
        handTypeBaseChips.text = chips.ToString();
    }

    private void UpdateMultsPanelVisuals(float mults)
    {
        handTypeBaseMults.text = mults.ToString();
    }

    private void UpdateScorePanelVisuals(float score)
    {
        handScore.text = score.ToString();
    }
}
