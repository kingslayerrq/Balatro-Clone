using UnityEngine;

/// <summary>
/// Scene singleton: one instance per scene, reachable through Instance.
/// Warns about duplicates and clears Instance when the object is destroyed.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate {typeof(T).Name} on '{name}' ignored, '{Instance.name}' is already the instance", this);
            return;
        }
        Instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
