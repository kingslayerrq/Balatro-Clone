using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BaseActionConfig : ScriptableObject
{
    [Tooltip("Name of this scoring action")]
    public string actionName;
    
    [TextArea(3, 5)]
    [Tooltip("Description of what this action does")]
    public string description;
    
    [Tooltip("Order in which this action is executed (lower numbers go first)")]
    public int executionOrder = 0;

    [Tooltip("Config")]
    public BaseActionConfig config;
    
    // Abstract method that all scoring actions must implement
    public abstract void Execute(Card card, List<Card> allCards, ScoreCalculator calculator);
    
    // Optional method for actions that need to run over time
    public virtual IEnumerator ExecuteCoroutine(Card card, List<Card> allCards, ScoreCalculator calculator)
    {
        Execute(card, allCards, calculator);
        yield return null;
    }

    public abstract BaseAction Create();

    [Serializable]
    public abstract class BaseAction
    {
        public string actionName;
        public string description;
        public int executionOrder;
        public BaseActionConfig config;

        public virtual void Execute(Card card, List<Card> allCards, ScoreCalculator calculator)
        {
            if (config != null)
            {
                config.Execute(card, allCards, calculator);
            }
        }
        public virtual IEnumerator ExecuteCoroutine(Card card, List<Card> allCards, ScoreCalculator calculator)
        {
            if (config != null)
            {
                yield return config.ExecuteCoroutine(card, allCards, calculator);
            }
            else
            {
                Execute(card, allCards, calculator);
                yield return null;
            }
        }
    }
}
