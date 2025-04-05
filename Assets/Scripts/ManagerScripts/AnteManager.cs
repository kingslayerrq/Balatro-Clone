using System;
using System.Collections.Generic;
using UnityEngine;

public class AnteManager : MonoBehaviour
{
    public static AnteManager Instance;
    private RunManager _runManager;
    private RoundManager _roundManager;

    [SerializeField] private BaseBlindParameters smallBlindConfig;
    [SerializeField] private BaseBlindParameters bigBlindConfig;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _runManager = RunManager.Instance;
        _roundManager = RoundManager.Instance;
        RoundButton.SelectRoundEvent.AddListener(SelectRound);
        RoundButton.SkipRoundEvent.AddListener(SkipRound);
    }
    

    private void SkipRound(Round round)
    {
        if (round == null || _roundManager == null) return;
        
        Debug.LogWarning("Skipping");
        _roundManager.SkipRound(round);
    }
    private void SelectRound(Round round)
    {
        if (round == null || _roundManager == null) return;

        Debug.LogWarning("Starting");
        _roundManager.StartRound(round);
    }

    public Ante Create(int anteLvl, BaseBlindParameters bossBlindConfig)
    {
        if (smallBlindConfig == null || bigBlindConfig == null) return null;
        var roundList = new List<Round>
        {
            RoundManager.Create(smallBlindConfig),
            RoundManager.Create(bigBlindConfig),
            RoundManager.Create(bossBlindConfig)
        };
        return new Ante
        {
            lvl = anteLvl,
            rounds = roundList
        };
    }
    
    [Serializable]
    public class Ante
    {
        public int lvl;
        public List<Round> rounds;
        
    }
    
    
}
