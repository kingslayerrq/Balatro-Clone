using UnityEngine;

public class ConsumablePanel : Panel
{
    public static ConsumablePanel Instance;
    
    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
        
    }
}
