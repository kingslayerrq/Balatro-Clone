using System;
using UnityEngine;


public abstract class BaseBlindActionParameters : ScriptableObject
{
    public string actionName;
    public string description;
    public int executionOrder;
    public BaseBlindActionParameters config;
    
    public abstract void Execute(Round round, RoundManager.State state);


    public abstract BaseBlindAction Create();
    

    [Serializable]
    public abstract class BaseBlindAction
    {
        public string actionName;
        public string description;
        public int executionOrder;
        public BaseBlindActionParameters config;

        public virtual void Execute(Round round, RoundManager.State state)
        {
            if (config != null)
            {
                config.Execute(round, state);
            }
        }
        
    }
}
