using UnityEngine;

public class JokerPanel : Panel
{
    public static JokerPanel Instance;
    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
