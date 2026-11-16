using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ScoreCalculator))]
public class PlayedCardPanel : SingletonPanel<PlayedCardPanel>
{
    [Tooltip("Gap between each scoring card popping up")]
    [SerializeField] private float checkCardGap = 0.1f;
    [Tooltip("Seconds in real time before calculating score")]
    [SerializeField] private float waitForScoring = 1f;
    [Tooltip("Gap between each card when lowering / removing played cards")]
    [SerializeField] private float recycleCardGap = 0.1f;
    [Tooltip("Pause between lowering the scoring cards and removing the played cards")]
    [SerializeField] private float recyclePause = 0.5f;

    /// <summary>
    /// Pop up the cards that will score, one by one, then pause before scoring starts
    /// </summary>
    /// <param name="scoringCards">Cards of this panel that score</param>
    public IEnumerator HighlightScoringCards(IReadOnlyList<Card> scoringCards)
    {
        yield return null;
        // reset all cards to a consistent starting scale
        foreach (var card in cardsInPanel)
        {
            card.cardVisuals.transform.localScale = card.cardVisuals.originalScale;
        }
        yield return new WaitForSecondsRealtime(0.2f);

        foreach (var card in scoringCards)
        {
            yield return new WaitForSecondsRealtime(checkCardGap);
            card.SetScoringHighlight(true);
        }

        yield return new WaitForSecondsRealtime(waitForScoring);
    }

    /// <summary>
    /// Push down the scoring cards, then move every played card to the used pile
    /// </summary>
    public IEnumerator RecycleCards(IReadOnlyList<Card> scoringCards, Panel usedPanel)
    {
        foreach (var card in scoringCards)
        {
            card.SetScoringHighlight(false);
            yield return new WaitForSecondsRealtime(recycleCardGap);
        }

        yield return new WaitForSecondsRealtime(recyclePause);

        // copy: MoveTo removes the card from cardsInPanel
        foreach (var card in cardsInPanel.ToArray())
        {
            yield return null;
            card.MoveTo(usedPanel, CardState.CardStatus.InUsed);
            yield return new WaitForSecondsRealtime(recycleCardGap);
        }
    }
}
