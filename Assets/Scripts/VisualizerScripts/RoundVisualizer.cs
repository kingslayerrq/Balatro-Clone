using System;
using TMPro;
using UnityEngine;

public class RoundVisualizer : MonoBehaviour
{
    public static RoundVisualizer Instance;

    private RoundManager _roundManager;
    private RunManager _runManager;
    
    [Header("UI to Visualize")] 
    [SerializeField] private TextMeshProUGUI blindName;
    [SerializeField] private TextMeshProUGUI blindDescription;
    
    [SerializeField] private TextMeshProUGUI blindChipGoal;
    [SerializeField] private TextMeshProUGUI blindReward;

    [SerializeField] private TextMeshProUGUI handCount;
    [SerializeField] private TextMeshProUGUI discardCount;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private TextMeshProUGUI anteLvl;
    [SerializeField] private TextMeshProUGUI anteLvlReqToWin;
    [SerializeField] private TextMeshProUGUI roundLvl;
    
    
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _roundManager = RoundManager.Instance;
        if (_roundManager)
        {
            _roundManager.roundSetupVisualEvent.AddListener(SetupVisual);
        }
        _runManager = RunManager.Instance;
    }

    private void SetupVisual(Round round)
    {
        blindName.text = round.blind.blindName;
        blindDescription.text = round.blind.description;
        blindChipGoal.text = round.chipGoal.FormatFloat();
        blindReward.text = " " + round.blind.reward.FormatRewardMoney();
        handCount.text = round.hands.FormatInt();
        discardCount.text = round.discards.FormatInt();
        money.text = _runManager.Money.FormatInt();
        anteLvl.text = _runManager.CurAnteLvl.FormatInt();
        anteLvlReqToWin.text = " / " + _runManager.AnteLvlReqToWin.FormatInt();
        roundLvl.text = _runManager.CurRoundLvl.FormatInt();
        
    }
}
