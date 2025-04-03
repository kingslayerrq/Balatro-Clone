using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    [Tooltip("Tolerance when comparing floats")]
    public const float TOLERANCE = 1e-6f;

    [Tooltip("Max $ icon count")] public const int MAX_REWARD_COUNT = 10;
    
    public static readonly Dictionary<Enums.BlindType, float> BASE_BLIND_CHIPGOAL = new Dictionary<Enums.BlindType, float>
    {
        { Enums.BlindType.SmallBlind, 300 },
        { Enums.BlindType.BigBlind, 450 },
        { Enums.BlindType.BossBlind, 600 },
    };
}
