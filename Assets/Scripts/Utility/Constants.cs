using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    // Tolerance when comparing floats
    public const float TOLERANCE = 1e-6f;

    // Max $ icon count
    public const int MAX_REWARD_COUNT = 10;

    // Base chip goal per ante level; a blind multiplies it by its baseChipGoalMultiplier
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

    public const int MAX_ANTE_WITH_CHIPGOAL = 16;

    /// <summary>
    /// Base chip goal of an ante. Antes past the table reuse the last entry.
    /// </summary>
    public static double GetAnteChipGoal(int anteLvl)
    {
        return BASE_ANTE_CHIPGOAL[Mathf.Clamp(anteLvl, 1, MAX_ANTE_WITH_CHIPGOAL)];
    }

    public static readonly string[] CARD_EDITIONS = new string[]
    {
        "REGULAR",
        "POLYCHROME",
        "FOIL",
        "NEGATIVE"
    };
}
