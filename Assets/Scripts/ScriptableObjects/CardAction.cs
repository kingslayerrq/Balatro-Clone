using UnityEngine;

/// <summary>
/// Basic card effect: add the card's chips to the hand
/// </summary>
[CreateAssetMenu(fileName = "CardAction", menuName = "Scriptable Objects/CardAction")]
public class CardActionConfig : BaseActionConfig
{
    public override void Execute(ScoreContext context)
    {
        context.Chips += context.Card.cardData.chips;
    }
}
