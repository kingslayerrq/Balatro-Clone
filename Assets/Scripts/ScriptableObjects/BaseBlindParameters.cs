using System;
using System.Collections.Generic;
using UnityEngine;


public abstract class BaseBlindParameters : ScriptableObject
{
    public string blindName;

    public string description;
    
    public float baseChipGoal;
    
    public int reward;

    public abstract BaseBlind Create();
    
    [Serializable]
    public abstract class BaseBlind
    {
        public string blindName;
        public string description;
        public float baseChipGoal;
        public int reward;
        public BaseBlindParameters config;
    }
}
