using UnityEngine;

public class ChipTMPAnim : BaseTMPAnim
{
    protected override void Start()
    {
        base.Start();
        this.amplitude = 2f;
        this.upperiod = 1.5f;
        this.downperiod = 1.5f;
    }
}
