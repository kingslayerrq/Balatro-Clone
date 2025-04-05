using UnityEngine;

public class DrawPanel : Panel
{
    public static DrawPanel Instance;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null) Instance = this;
    }
}
