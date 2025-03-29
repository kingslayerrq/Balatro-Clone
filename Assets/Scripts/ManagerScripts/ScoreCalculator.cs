using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ScoreVisualizer))]
public class ScoreCalculator : MonoBehaviour
{
    [SerializeField] private float checkCardGap = 0.2f;
    public float curChips = 0;
    public float curMults = 0;

    //public UnityEvent<>
    
    public IEnumerator CalculateScore(List<Card> cards)
    {
        // get data reference from cards
        List<CardData> cardDataList = cards.Select(card => card.GetComponent<CardData>()).ToList();
        
        // check whether the cards can be scored (first round of screening)
        CheckForScoreAvailability(cardDataList);
        
        // display the can score result
        foreach (CardData cd in cardDataList)
        {
            if (!cd.canScore) continue;
            
            cd.onScoreCheckEvent.Invoke(cd.card, cd.canScore);
            yield return new WaitForSecondsRealtime(checkCardGap);
            
        }
        
        // just sum up
        

        yield return null;

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
}
