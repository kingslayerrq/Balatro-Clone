using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Play / Skip buttons for one blind. Whoever creates the button listens to its events.
/// </summary>
public class RoundButton : MonoBehaviour
{
    public Button playButton;
    public Button skipButton;

    private Round _round;

    public event Action<Round> PlayClicked;
    public event Action<Round> SkipClicked;

    /// <summary>
    /// Bind the button to a round
    /// </summary>
    public void Setup(Round round)
    {
        _round = round;

        // Set up button listeners
        playButton.onClick.RemoveListener(PlayRound);
        playButton.onClick.AddListener(PlayRound);

        if (skipButton == null) return;
        skipButton.onClick.RemoveListener(SkipRound);
        skipButton.onClick.AddListener(SkipRound);
    }

    private void PlayRound()
    {
        if (_round == null) return;

        playButton.interactable = false;
        if (skipButton) skipButton.interactable = false;                // Also removes the ability to skip this round
        PlayClicked?.Invoke(_round);
    }

    private void SkipRound()
    {
        if (_round == null) return;

        skipButton.interactable = false;
        SkipClicked?.Invoke(_round);
    }
}
