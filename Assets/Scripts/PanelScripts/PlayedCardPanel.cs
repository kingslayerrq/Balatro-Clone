using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ScoreCalculator))]
public class PlayedCardPanel : Panel
{
    public static PlayedCardPanel Instance;
    private ScoreCalculator _scoreCalculator;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
    }

    protected override void Start()
    {
        base.Start();
        
        _scoreCalculator = GetComponent<ScoreCalculator>();
        
        HandPanel.Instance.handPlayedEvent.AddListener(PlayedHandHandler);
    }


    private void PlayedHandHandler(Panel panel)
    {
        if (panel != this) return;

        foreach (Card card in cardsInPanel)
        {
            card.cardVisuals.UpdateIndex(cardsInPanel.Count);
        }
        
        // Calculate Score
        StartCoroutine(_scoreCalculator.CalculateScore(cardsInPanel));
    }
}
