using UnityEngine;

public class UsedPanel : Panel
{
    public static UsedPanel Instance;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null) Instance = this;
    }
}
