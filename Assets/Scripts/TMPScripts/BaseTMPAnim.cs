using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using TMPEffects.Components;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMPAnimator))]
[RequireComponent(typeof(TextMeshProUGUI))]
public abstract class BaseTMPAnim : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] protected float amplitude;
    [SerializeField] protected float upperiod;
    [SerializeField] protected float downperiod;
    [SerializeField] protected float troughwait;
    [SerializeField] protected float crestwait;
    [SerializeField] protected string content;
    [SerializeField] protected float uniformity;
    
    protected float _prevAmplitude;
    protected float _prevUpperiod;
    protected float _prevDownperiod;
    protected float _prevTroughwait;
    protected float _prevCrestwait;
    protected string _prevContent;
    protected float _prevUniformity;
    
    [SerializeField] protected string prevText;
    [SerializeField] protected string currText;

    protected TextMeshProUGUI _tmp;
    
    private readonly StringBuilder sb = new StringBuilder(128);
    
    

    protected virtual void Start()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        if (_tmp == null)
        {
            Debug.Log("No TMP component attached");
        }
    }

    private void Update()
    {
        currText = _tmp.text;
        if (currText == "" || currText == prevText) return;
        UpdateText();
    }

    /// <summary>
    /// Update the text with wave animation
    /// </summary>
    protected virtual void UpdateText()
    {
        UpdateContent();
        
        UpdateTMPAnimParams();

        // if content and animation param didn't change, return early with previous text
        if (Mathf.Approximately(amplitude, _prevAmplitude) && Mathf.Approximately(upperiod, _prevUpperiod)
                                                           && Mathf.Approximately(downperiod, _prevDownperiod)
                                                           && Mathf.Approximately(troughwait, _prevTroughwait) &&
                                                           Mathf.Approximately(crestwait, _prevCrestwait)
                                                           && Mathf.Approximately(uniformity, _prevUniformity) &&
                                                           (_prevContent == content))
        {
            _tmp.SetText(prevText);
            currText = _tmp.text;
            return;
        }
        
        // build new string 
        sb.Clear();
        sb.Append("<wave");

        if (amplitude  != 0f) sb.AppendFormat(" amplitude=\"{0:F2}\"", amplitude);
        if (upperiod   != 0f) sb.AppendFormat(" upperiod=\"{0:F2}\"", upperiod);
        if (downperiod != 0f) sb.AppendFormat(" downperiod=\"{0:F2}\"", downperiod);
        if (troughwait != 0f) sb.AppendFormat(" troughwait=\"{0:F2}\"", troughwait);
        if (uniformity != 0f) sb.AppendFormat(" uniformity=\"{0:F2}\"", uniformity);
        
        
        sb.Append('>').Append(content).Append("</wave>");
        var cur = sb.ToString();
        _tmp.SetText(cur);
        // Debug.Log($"Updating to {cur}");
        currText = cur;
        
        // update 
        _prevContent = content;
        prevText = cur;
        _prevAmplitude = amplitude;
        _prevUpperiod = upperiod;
        _prevDownperiod = downperiod;
        _prevTroughwait = troughwait;
        _prevCrestwait = crestwait;
        _prevUniformity = uniformity;
    }

    /// <summary>
    /// optionally update the parameters of tmp animation based on content change
    /// </summary>
    protected virtual void UpdateTMPAnimParams()
    {
        if (_prevContent == content) return;
    }

    /// <summary>
    /// update the content of the text
    /// </summary>
    protected virtual void UpdateContent()
    {
        const string pattern = @"<wave[^>]*>(.*?)</wave>";
        var match = Regex.Match(currText, pattern);
        content = match.Success ? match.Groups[1].Value : currText;
    }
}
