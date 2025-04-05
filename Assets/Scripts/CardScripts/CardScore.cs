using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(Card))]
[RequireComponent(typeof(CardData))]
public class CardScore : MonoBehaviour, IScorable
{
    private Card _card;
    private CardData _cardData;
    private ScoreCalculator _scoreCalculator;

    [SerializeField] private float scoreTriggerGap = 0.2f;
    
    
    private void Start()
    {
        _card = GetComponent<Card>();
        _cardData = GetComponent<CardData>();
        _scoreCalculator = ScoreCalculator.Instance;
        
    }

    public IEnumerator ScoreCoroutine()
    {
        var cardActions = _cardData.cardActions;
        cardActions.Sort(((action, cardAction) => action.executionOrder.CompareTo(cardAction.executionOrder)));
        for (int i = 0; i < _cardData.triggerCounts; i++)
        {
            foreach (var action in cardActions)
            {
                yield return new WaitForSecondsRealtime(cardActions.Count > 1 ? scoreTriggerGap : 0);
                Card.scoreEvent?.Invoke(_card);
                action.Execute(_card, _card.curPanel.cardsInSelection, _scoreCalculator);
            }

            yield return new WaitForSecondsRealtime(scoreTriggerGap);
        }
        
    }
    
}
