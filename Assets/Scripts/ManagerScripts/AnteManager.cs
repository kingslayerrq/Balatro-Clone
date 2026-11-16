using System;
using System.Collections.Generic;
using UnityEngine;

public class AnteManager : Singleton<AnteManager>
{    private RunManager _runManager;
    private RoundManager _roundManager;

    [SerializeField] private BaseBlindParameters smallBlindConfig;
    [SerializeField] private BaseBlindParameters bigBlindConfig;

    private void Start()
    {
        _runManager = RunManager.Instance;
        _roundManager = RoundManager.Instance;
        if (!_runManager || !_roundManager)
        {
            Debug.LogError("AnteManager: RunManager or RoundManager is missing from the scene", this);
        }
    }

    /// <summary>
    /// The player chose to skip this blind
    /// </summary>
    public void SkipRound(Round round)
    {
        if (round == null || _runManager == null) return;
        _runManager.SkipRound(round);
    }

    /// <summary>
    /// The player chose to play this blind
    /// </summary>
    public void SelectRound(Round round)
    {
        if (round == null || _roundManager == null) return;
        _roundManager.StartRound(round);
    }

    public Ante Create(int anteLvl, BaseBlindParameters bossBlindConfig)
    {
        if (smallBlindConfig == null || bigBlindConfig == null || bossBlindConfig == null)
        {
            Debug.LogError("AnteManager: a blind config is missing, cannot create an ante", this);
            return null;
        }
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

        /// <summary>
        /// Index of the blind to play next (-1 once every blind is complete or skipped)
        /// </summary>
        public int CurrentRoundIndex => rounds.FindIndex(r => !r.isComplete && !r.isSkipped);
    }
}
