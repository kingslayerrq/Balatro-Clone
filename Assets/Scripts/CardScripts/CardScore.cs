using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Card))]
[RequireComponent(typeof(CardData))]
public class CardScore : MonoBehaviour, IScorable
{
    private Card _card;
    private CardData _cardData;

    [SerializeField] private float scoreTriggerGap = 0.2f;


    private void Awake()
    {
        _card = GetComponent<Card>();
        _cardData = GetComponent<CardData>();
    }

    /// <summary>
    /// Run this card's actions against the calculator, once per trigger
    /// </summary>
    /// <param name="scoringCards">Every card scoring in the played hand</param>
    /// <param name="calculator">Calculator holding the running chips / mults</param>
    public IEnumerator ScoreCoroutine(List<Card> scoringCards, ScoreCalculator calculator)
    {
        // already sorted by execution order when the card was created
        var cardActions = _cardData.cardActions;
        for (int i = 0; i < _cardData.triggerCounts; i++)
        {
            foreach (var action in cardActions)
            {
                yield return new WaitForSecondsRealtime(cardActions.Count > 1 ? scoreTriggerGap : 0);
                _card.cardVisuals.OnScore();
                action.Execute(_card, scoringCards, calculator);
            }

            yield return new WaitForSecondsRealtime(scoreTriggerGap);
        }

    }

}
