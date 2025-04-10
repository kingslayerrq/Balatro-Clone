using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class RoundVisualizer : MonoBehaviour
{
    public static RoundVisualizer Instance;

    private RoundManager _roundManager;
    private RunManager _runManager;

    [Header("Settings")]
    [SerializeField] private float colorTransition = 0.15f;
    
    [Header("UI to Visualize")] 
    [SerializeField] private TextMeshProUGUI blindName;
    [SerializeField] private TextMeshProUGUI blindDescription;
    [SerializeField] private UIImageAnimation blindImageAnimation;
    
    
    [SerializeField] private TextMeshProUGUI blindChipGoal;
    [SerializeField] private TextMeshProUGUI blindReward;

    [SerializeField] private TextMeshProUGUI handCount;
    [SerializeField] private TextMeshProUGUI discardCount;
    [SerializeField] private TextMeshProUGUI money;
    [SerializeField] private TextMeshProUGUI anteLvl;
    [SerializeField] private TextMeshProUGUI anteLvlReqToWin;
    [SerializeField] private TextMeshProUGUI roundLvl;
    [SerializeField] private TextMeshProUGUI roundScore;

    [SerializeField] private Image bgColor;
    [SerializeField] private Image blindNamePanelColor;
    [SerializeField] private Image blindDetailPanelColor;
    [SerializeField] private Image sidebarLeftOutlineColor;
    [SerializeField] private Image sidebarRightOutlineColor;
    [SerializeField] private Color sidebarOrigColor;
    [SerializeField] private Camera camera;
    [SerializeField] private Color cameraOrigColor;
    
    private int _prevHandCount;
    private int _prevDiscardCount;
    private int _prevMoney;
    private int _prevAnteLvl;
    private int _prevRoundLvl;
    private float _prevChipGoal;
    private float _prevScore;
    
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

        if (camera)
        {
            cameraOrigColor = camera.backgroundColor;
        }

        if (sidebarLeftOutlineColor && sidebarRightOutlineColor)
        {
            sidebarOrigColor = sidebarLeftOutlineColor.color;
        }
    }

    private void Update()
    {
        // Only update UI when values change
        if (_roundManager.curRound != null)
        {
            // Check for hand count changes
            if (_prevHandCount != _roundManager.curRound.hands)
            {
                _prevHandCount = _roundManager.curRound.hands;
                UpdateHandCount(_prevHandCount);
            }
        
            // Check for discard count changes
            if (_prevDiscardCount != _roundManager.curRound.discards)
            {
                _prevDiscardCount = _roundManager.curRound.discards;
                UpdateDiscardCount(_prevDiscardCount);
            }
        
            // Check for chip goal changes
            if (_prevChipGoal != _roundManager.curRound.chipGoal)
            {
                _prevChipGoal = _roundManager.curRound.chipGoal;
                UpdateBlindChipGoal(_prevChipGoal);
            }
            
            // Check for score changes
            if (_prevScore != _roundManager.curRound.roundScore)
            {
                _prevScore = _roundManager.curRound.roundScore;
                UpdateRoundScore(_prevScore);
            }
        }
    
        // Check for money changes
        if (_prevMoney != _runManager.Money)
        {
            _prevMoney = _runManager.Money;
            UpdateMoney(_prevMoney);
        }
    
        // Check for ante level changes
        if (_prevAnteLvl != _runManager.CurAnteLvl)
        {
            _prevAnteLvl = _runManager.CurAnteLvl;
            UpdateAnteLvl(_prevAnteLvl);
        }
    
        // Check for round level changes
        if (_prevRoundLvl != _runManager.CurRoundLvl)
        {
            _prevRoundLvl = _runManager.CurRoundLvl;
            UpdateRoundLvl(_prevRoundLvl);
        }
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

        blindImageAnimation.sprites = round.blind.blindSprites;
        blindImageAnimation.isSet = true;

        blindNamePanelColor.DOColor(round.blind.blindColor, colorTransition);
        sidebarLeftOutlineColor.DOColor(round.blind.blindColor, colorTransition);
        sidebarRightOutlineColor.DOColor(round.blind.blindColor, colorTransition);
        
        // Create a new color with the same RGB values but different alpha
        Color newColor = round.blind.blindColor;
        newColor.a = 50/255f;
        blindDetailPanelColor.DOColor(newColor, colorTransition);

        // Turn off camera color during boss
        // Change Background Color during boss
        if (round.blind.type == Enums.BlindType.BossBlind)
        {
            bgColor.DOColor(round.blind.blindColor, colorTransition);
            camera.DOColor(new Color(0, 0, 0, 0), colorTransition);
        }
    }

    
    // Update methods for each field
    public void UpdateBlindName(string name)
    {
        if (blindName != null)
            blindName.text = name;
    }
    
    public void UpdateBlindDescription(string description)
    {
        if (blindDescription != null)
            blindDescription.text = description;
    }
    
    public void UpdateBlindChipGoal(float goal)
    {
        if (blindChipGoal != null)
            blindChipGoal.text = goal.FormatFloat();
    }
    
    public void UpdateRoundScore(float score)
    {
        if (roundScore != null)
            roundScore.text = score.FormatFloat();
    }
    
    public void UpdateBlindReward(int reward)
    {
        if (blindReward != null)
            blindReward.text = " " + reward.FormatRewardMoney();
    }
    
    public void UpdateHandCount(int count)
    {
        if (handCount != null)
            handCount.text = count.FormatInt();
    }
    
    public void UpdateDiscardCount(int count)
    {
        if (discardCount != null)
            discardCount.text = count.FormatInt();
    }
    
    public void UpdateMoney(int amount)
    {
        if (money != null)
            money.text = amount.FormatInt();
    }
    
    public void UpdateAnteLvl(int level)
    {
        if (anteLvl != null)
            anteLvl.text = level.FormatInt();
    }
    
    public void UpdateAnteLvlReqToWin(int req)
    {
        if (anteLvlReqToWin != null)
            anteLvlReqToWin.text = " / " + req.FormatInt();
    }
    
    public void UpdateRoundLvl(int level)
    {
        if (roundLvl != null)
            roundLvl.text = level.FormatInt();
    }
    
    // Method to update all round-specific values
    public void UpdateRoundInfo(Round round)
    {
        if (round == null) return;
        
        UpdateHandCount(round.hands);
        UpdateDiscardCount(round.discards);
        UpdateBlindChipGoal(round.chipGoal);
        
        if (round.blind != null)
        {
            UpdateBlindName(round.blind.blindName);
            UpdateBlindDescription(round.blind.description);
            UpdateBlindReward(round.blind.reward);
        }
    }
}
