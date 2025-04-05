using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class RoundButton : MonoBehaviour
{
    public Button playButton;
    public Button skipButton;
    
    private Round _round;
    private AnteManager _anteManager;

    [HideInInspector] public static UnityEvent<Round> SelectRoundEvent = new UnityEvent<Round>();
    [HideInInspector] public static UnityEvent<Round> SkipRoundEvent = new UnityEvent<Round>();

    private void Start()
    {
        _anteManager = AnteManager.Instance;
    }

    public void Setup(Round round)
    {
        _round = round;
        
        // Set up button listeners
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(PlayRound);

        if (skipButton == null) return;
        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(SkipRound);
    }
    
    private void PlayRound()
    {
        if (_round != null && _anteManager != null)
        {
            playButton.interactable = false;
            skipButton.interactable = false;                // Also removes the ability to skip this round
            SelectRoundEvent?.Invoke(_round);
        }
    }
    
    private void SkipRound()
    {
        if (_round != null && _anteManager != null)
        {
            skipButton.interactable = false;
            SkipRoundEvent?.Invoke(_round);
        }
    }
}