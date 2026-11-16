using UnityEngine;

/// <summary>
/// A blind effect authored as an asset. RoundManager executes it when the blind is
/// entered, after a hand is played, or when the blind is beaten (see BossBlindConfig).
/// </summary>
public abstract class BaseBlindActionParameters : ScriptableObject
{
    public string actionName;
    public string description;
    [Tooltip("Order in which this action is executed (lower numbers go first)")]
    public int executionOrder;

    public abstract void Execute(Round round, RoundManager.State state);
}
