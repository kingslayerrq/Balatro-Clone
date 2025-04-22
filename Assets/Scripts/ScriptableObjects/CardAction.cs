using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardAction", menuName = "Scriptable Objects/CardAction")]
public class CardActionConfig : BaseActionConfig
{
    public override void Execute(Card card, List<Card> allCards, ScoreCalculator calculator)
    {
        calculator.curChips += card.cardData.chips;
        Debug.LogWarning("triggering" + card);
    }

    public override BaseAction Create()
    {
        return new CardAction
        {
            actionName = actionName,
            description = description,
            executionOrder = executionOrder,
            config = this
        };
    }

    public class CardAction : BaseAction
    {
        public override void Execute(Card card, List<Card> allCards, ScoreCalculator calculator)
        {
            base.Execute(card, allCards, calculator);
        }
    }
}
