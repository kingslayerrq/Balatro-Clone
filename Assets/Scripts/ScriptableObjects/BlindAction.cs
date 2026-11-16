using UnityEngine;

/// <summary>
/// Blind action with no effect. Base class for the real boss actions.
/// </summary>
[CreateAssetMenu(fileName = "BasicBlindAction", menuName = "Scriptable Objects/BasicBlindAction")]
public class BlindActionConfig : BaseBlindActionParameters
{
    public override void Execute(Round round, RoundManager.State state)
    {
    }
}
