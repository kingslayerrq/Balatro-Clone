using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundButton : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button skipButton;
    
    private Round _round;
    private AnteManager _anteManager;

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
            _anteManager.SelectRound(_round);
        }
    }
    
    private void SkipRound()
    {
        if (_round != null && _anteManager != null)
        {
            _anteManager.SkipRound(_round);
        }
    }
}