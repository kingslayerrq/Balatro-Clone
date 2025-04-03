using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int CurrentSeed { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        SetRandomSeed();
    }
    
    public void SetSeed(int seed)
    {
        CurrentSeed = seed;
        Random.InitState(seed);
    }
    
    public void SetRandomSeed()
    {
        int seed = System.Environment.TickCount;
        SetSeed(seed);
    }
}