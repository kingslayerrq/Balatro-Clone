using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PanelParameters", menuName = "Scriptable Objects/PanelParameters")]
public class PanelParameters : ScriptableObject
{
    public string panelID;
    
    public Enums.PanelType panelType = Enums.PanelType.Basic;
    
    public int maxSize = 10;

    public int maxSelectionSize;

    public bool allowCurve = true;
    
    [Tooltip("Can player adjust cards indexes")]
    public bool allowSwap = true;

    public bool allowDrag = true;
    
    public float cardReturnTransition = 0.15f;
}
