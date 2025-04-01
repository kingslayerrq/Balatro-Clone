using System;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;
    private RunManager _runManager;
    public RoundState state = RoundState.Init;

    [HideInInspector] public UnityEvent<RoundState> UpdateRoundStateEvent = new UnityEvent<RoundState>();
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
    }

    private void Start()
    {
        _runManager = RunManager.Instance;
    }

    private void Init()
    {
        // Set up round
        
    }
}

public enum RoundState
{
    Init,
    Play,
    Score,
    Evaluate,
    Draw,
    Result
}