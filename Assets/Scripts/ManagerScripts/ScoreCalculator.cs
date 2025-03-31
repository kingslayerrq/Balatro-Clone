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
        List<CardData> cardDataList = cards.Select(card => card.cardData).ToList();

        yield return new WaitForSecondsRealtime(checkCardGap);
        
        // display the can score result
        yield return StartCoroutine(DisplayCheckScoreResult(cardDataList));
        
        

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
