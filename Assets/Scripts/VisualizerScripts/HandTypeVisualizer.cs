using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HandTypeVisualizer : MonoBehaviour
{
    public static HandTypeVisualizer Instance;
    
    private HandAnalyzer _handAnalyzer;
    private ScoreCalculator _scoreCalculator;
    private RoundManager _roundManager;

    [SerializeField] private TextMeshProUGUI handTypeName;
    [SerializeField] private TextMeshProUGUI handTypeLvl;
    [SerializeField] private TextMeshProUGUI handTypeBaseChips;
    [SerializeField] private TextMeshProUGUI handTypeBaseMults;
    [SerializeField] private TextMeshProUGUI handScore;
    [SerializeField] private GameObject handTypeObj;
    [SerializeField] private GameObject handTypeLvlObj;
    [SerializeField] private GameObject handScoreObj;

    [HideInInspector] public UnityEvent<double> UpdateRoundScoreEvent = new UnityEvent<double>();
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _roundManager = RoundManager.Instance;
        _scoreCalculator = ScoreCalculator.Instance;
        if (_scoreCalculator)
        {
            _scoreCalculator.UpdateHandTypeVisualEvent.AddListener(UpdateHandTypePanelVisuals);
            _scoreCalculator.UpdateChipsVisualEvent.AddListener(UpdateChipsPanelVisuals);
            _scoreCalculator.UpdateMultsVisualEvent.AddListener(UpdateMultsPanelVisuals);
            _scoreCalculator.UpdateScoreVisualEvent.AddListener(UpdateHandScorePanelVisualWrapper);
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

    private void UpdateHandScorePanelVisualWrapper(double score)
    {
        if (score == 0) return;
        if (!_roundManager || _roundManager.curState != RoundManager.State.Score) return;
        StartCoroutine(UpdateScorePanelVisuals(score));
    }

    /// <summary>
    /// Updates the calculated score during score state
    /// Invokes UpdateRoundScoreEvent when done
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    private IEnumerator UpdateScorePanelVisuals(double score)
    {
        if (handTypeObj == null || handTypeLvlObj == null) yield break;
        handTypeLvlObj.SetActive(false);
        handTypeObj.SetActive(false);
        handScoreObj.SetActive(true);
        handScore.text = score.FormatDouble();
        yield return new WaitForSecondsRealtime(1f);
        handScoreObj.SetActive(false);
        UpdateRoundScoreEvent?.Invoke(score);
        handTypeObj.SetActive(true);
        handTypeLvlObj.SetActive(true);
    }
}
