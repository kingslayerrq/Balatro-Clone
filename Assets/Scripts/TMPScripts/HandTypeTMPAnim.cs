using UnityEngine;

public class HandTypeTMPAnim : BaseTMPAnim
{
    protected override void Start()
    {
        base.Start();
        this.amplitude = 3f;
        this.crestwait = 0.3f;
        this.uniformity = 0.5f;
        
    }

    
}
