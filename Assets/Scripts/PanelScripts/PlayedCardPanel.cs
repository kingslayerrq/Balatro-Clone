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
    private RoundManager _roundManager;

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
        _roundManager = RoundManager.Instance;

        

        if (_roundManager != null)
        {
            _roundManager.updateRoundStateEvent.AddListener(OnPlayedStateHandler);
        }
    }

    private void OnPlayedStateHandler(RoundManager.State state)
    {
        if (state != RoundManager.State.OnPlayed) return;
        // Displaying Scoring Cards
        StartCoroutine(DisplayScoringCards(cardsInPanel));
    }

   
    /// <summary>
    /// Display the cards that can be scored and update state at the end
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    private IEnumerator DisplayScoringCards(List<Card> cards)
    {
        // TODO: this is waiting for HandAnalyzer to finalize the scoring hand
        yield return new WaitForEndOfFrame();
        // First reset all cards to ensure consistent starting state
        foreach (var card in cards.Where(card => !card.cardData.isScoring))
        {
            card.cardVisuals.transform.localScale = card.cardVisuals.originalScale;
        }
        yield return new WaitForSecondsRealtime(0.2f);
        foreach (var cd in cards.Where(cd => cd.cardData.canScore))
        {
            yield return new WaitForSecondsRealtime(checkCardGap);
            
            cd.cardData.onScoreCheckEvent?.Invoke(cd, cd.cardData.canScore);
        }

        yield return new WaitForSecondsRealtime(waitForScoring);
        // update state
        _roundManager.updateRoundStateEvent?.Invoke(RoundManager.State.Score);
    }
}
