using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ScoreCalculator))]
public class PlayedCardPanel : Panel
{
    public static PlayedCardPanel Instance;

    private HandPanel _handPanel;
    private ScoreCalculator _scoreCalculator;

    [SerializeField] private float checkCardGap = 0.1f;
    [Tooltip("Seconds in real time before calculating score")]
    [SerializeField] private float waitForScoring = 1f;
    
    [HideInInspector] public UnityEvent<List<Card>> calculateScoringCardsEvent = new UnityEvent<List<Card>>();
    

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
        
        _handPanel = HandPanel.Instance;

        if (_handPanel != null)
        {
            _handPanel.handPlayedEvent.AddListener(PlayedHandHandler);
        }
    }


    private void PlayedHandHandler(Panel panel)
    {
        if (panel != this) return;
        
        // Displaying Scoring Cards
        StartCoroutine(DisplayScoringCards(cardsInPanel));
    }
    
    private IEnumerator DisplayScoringCards(List<Card> cards)
    {
        // TODO: this is waiting for HandAnalyzer to finalize the scoring hand
        yield return new WaitForEndOfFrame();
        // First reset all cards to ensure consistent starting state
        foreach (Card card in cards)
        {
            // Reset card visual state
            if (!card.cardData.isScoring)
            {
                card.cardVisuals.transform.localScale = card.cardVisuals.originalScale;
            }
  
        }
        yield return new WaitForSecondsRealtime(checkCardGap);
        foreach (Card cd in cards)
        {
            if (!cd.cardData.canScore) continue;
            
            yield return new WaitForSecondsRealtime(checkCardGap);
            
            cd.cardData.onScoreCheckEvent.Invoke(cd, cd.cardData.canScore);
        }

        yield return new WaitForSecondsRealtime(waitForScoring);
        // trigger to calculate score
        calculateScoringCardsEvent.Invoke(cardsInSelection);
    }
}
