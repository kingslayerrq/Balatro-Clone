using System;
using UnityEngine;
using DG.Tweening;
using TMPEffects.Components;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(TMPAnimator))]

public class WaveText : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void AddWave()
    {
        if (_text.text == "") return;
        _text.text = "<wave amplitude=6 upperiod=0.4 downperiod=0.4 troughwait=2 uniformity=0.1>" + _text.text + "</wave>";
    }
}
