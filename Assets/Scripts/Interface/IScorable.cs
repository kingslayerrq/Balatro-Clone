using System.Collections;
using System.Collections.Generic;

interface IScorable
{
    IEnumerator ScoreCoroutine(List<Card> scoringCards, ScoreCalculator calculator);
}
