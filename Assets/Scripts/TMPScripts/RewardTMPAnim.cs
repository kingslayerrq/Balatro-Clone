using UnityEngine;

public class RewardTMPAnim : BaseTMPAnim
{
    private int length = 0;
    protected override void Start()
    {
        base.Start();
        
        this.amplitude = 5f;
        this.upperiod = 0.2f;
        this.downperiod = 0.2f;
        length = this.content.Length;
        this.troughwait = (length + 2) * this.upperiod;
    }

    protected override void UpdateTMPAnimParams()
    {
        base.UpdateTMPAnimParams();
        length = this.content.Length;
        this.troughwait = (length + 2) * this.upperiod;
    }
}
