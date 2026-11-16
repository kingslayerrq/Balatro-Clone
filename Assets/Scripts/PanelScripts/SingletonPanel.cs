using UnityEngine;

/// <summary>
/// A panel that exists once in the scene (hand, draw pile, ...), reachable through Instance.
/// </summary>
public abstract class SingletonPanel<T> : Panel where T : SingletonPanel<T>
{
    public static T Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate {typeof(T).Name} on '{name}' ignored, '{Instance.name}' is already the instance", this);
            return;
        }
        Instance = (T)this;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Instance == this) Instance = null;
    }
}
