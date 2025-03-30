using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ScoreVisualizer))]
public class ScoreCalculator : MonoBehaviour
{
    [SerializeField] private float checkCardGap = 0.2f;
    
    private JokerPanel _jokerPanel;
    
    public float curChips = 0;
    public float curMults = 0;

    private void Start()
    {
        _jokerPanel = JokerPanel.Instance;
    }

    public IEnumerator CalculateScore(List<Card> cards)
    {
        // get data reference from cards
        List<CardData> cardDataList = cards.Select(card => card.GetComponent<CardData>()).ToList();
        
        // check whether the cards can be scored (first round of screening)
        CheckForScoreAvailability(cardDataList);

        yield return new WaitForSecondsRealtime(checkCardGap);
        
        // display the can score result
        yield return StartCoroutine(DisplayCheckScoreResult(cardDataList));
        
        

    }

    /// <summary>
    /// Check for score availability and store the cards that can score into cardsInSelection List
    /// </summary>
    /// <param name="cards"></param>
    private void CheckForScoreAvailability(List<CardData> cards)
    {
        // check whether the cards can be scored (first round of screening)
        for (int i = 0; i < cards.Count; i++)
        {
            // test
            cards[i].canScore = i < 3 ;
        }
    }

    private IEnumerator DisplayCheckScoreResult(List<CardData> cards)
    {
        foreach (CardData cd in cards)
        {
            if (!cd.canScore) continue;
            
            yield return new WaitForSecondsRealtime(checkCardGap);
            
            cd.onScoreCheckEvent.Invoke(cd.card, cd.canScore);
        }
    }
}
