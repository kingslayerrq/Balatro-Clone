using UnityEngine;

public class MoneyTMPAnim : BaseTMPAnim
{
    private int length = 0;
    protected override void Start()
    {
        base.Start();
        
        this.amplitude = 2f;
        this.upperiod = 0.3f;
        this.downperiod = 0.3f;
        this.uniformity = 0.1f;
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
