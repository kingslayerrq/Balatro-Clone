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
    }

    public void SelectSmallBlind(bool chooseToSkip)
    {
        SelectRound(_runManager.antes[_runManager.curAnteLvl-1].rounds[0], chooseToSkip);
    }
    public void SelectRound(Round round, bool chooseToSkip)
    {
        if (round == null) return;
        if (chooseToSkip)
        {
            _roundManager.SkipRound(round);
        }
        else
        {
            Debug.LogWarning("Starting");
            // Start the round
            _roundManager.StartRound(round);
        }
    }

    public Ante Create(int anteLvl, BaseBlindParameters bossBlindConfig)
    {
        if (smallBlindConfig == null || bigBlindConfig == null) return null;
        var roundList = new List<Round>();
        roundList.Add(_roundManager.Create(smallBlindConfig));
        roundList.Add(_roundManager.Create(bigBlindConfig));
        roundList.Add(_roundManager.Create(bossBlindConfig));
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
