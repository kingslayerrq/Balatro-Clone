using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    [Tooltip("Tolerance when comparing floats")]
    public const float TOLERANCE = 1e-6f;

    [Tooltip("Max $ icon count")] public const int MAX_REWARD_COUNT = 10;
    
    // public static readonly Dictionary<Enums.BlindType, float> BASE_BLIND_CHIPGOAL = new Dictionary<Enums.BlindType, float>
    // {
    //     { Enums.BlindType.SmallBlind, 300 },
    //     { Enums.BlindType.BigBlind, 450 },
    //     { Enums.BlindType.BossBlind, 600 },
    // };
    
    public static readonly Dictionary<int, double> BASE_ANTE_CHIPGOAL = new Dictionary<int, double>
    {
        {1, 300},
        {2, 800},
        {3, 2000},
        {4, 5000},
        {5, 11000},
        {6, 20000},
        {7, 35000},
        {8, 50000},
        {9, 110000},
        {10, 560000},
        {11, 7200000},
        {12, 300000000},
        {13, 47000000000},
        {14, 2.9e13},
        {15, 7.7e16},
        {16, 8.6e20}
    };

    public static readonly string[] CARD_EDITIONS = new string[]
    {
        "REGULAR",
        "POLYCHROME",
        "FOIL",
        "NEGATIVE"
    };
}
