using System.Collections.Generic;
using UnityEngine;

public class PlayedCardPanel : Panel
{
    public static PlayedCardPanel Instance;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
    }

    protected override void Start()
    {
        base.Start();
        
        
        

    }

    protected override void EndDrag(Card card, Panel panel)
    {
        base.EndDrag(card, panel);
    }

    
    
}
