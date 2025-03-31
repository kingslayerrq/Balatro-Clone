using System;
using TMPro;
using UnityEngine;

public class HandTypeVisualizer : MonoBehaviour
{
    public static HandTypeVisualizer Instance;

    private RunManager _runManager;
    private HandAnalyzer _handAnalyzer;

    [SerializeField] private TextMeshProUGUI handTypeName;
    [SerializeField] private TextMeshProUGUI handTypeLvl;
    [SerializeField] private TextMeshProUGUI handTypeBaseChips;
    [SerializeField] private TextMeshProUGUI handTypeBaseMults;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _runManager = RunManager.Instance;
        _handAnalyzer = HandAnalyzer.Instance;

        if (_handAnalyzer)
        {
            _handAnalyzer.UpdateHandTypeEvent.AddListener(UpdateHandTypePanelVisuals);
        }

    }

    private void UpdateHandTypePanelVisuals(Enums.BasePokerHandType handType)
    {
        var handTypeInfo = _runManager.GetRunHandTypeInfo(handType);
        handTypeName.text = handTypeInfo.name;
        handTypeLvl.text = handTypeInfo.name == "" ? "" : "lvl. " + handTypeInfo.lvl;
        handTypeBaseChips.text = handTypeInfo.baseChips.ToString();
        handTypeBaseMults.text = handTypeInfo.baseMults.ToString();
        

    }
}
