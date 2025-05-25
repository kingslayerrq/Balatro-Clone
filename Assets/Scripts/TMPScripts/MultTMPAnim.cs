using UnityEngine;

public class MultTMPAnim : BaseTMPAnim
{
    protected override void Start()
    {
        base.Start();
        this.amplitude = 2f;
        this.upperiod = 1.5f;
        this.downperiod = 1.5f;
    }
}
