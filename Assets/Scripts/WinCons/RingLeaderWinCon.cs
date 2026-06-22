using UnityEngine;

public class RingLeaderWinCon : WinCon
{
    public RingLeaderWinCon(string description) : base(WinConType.RingLeader, description)
    {
    }

    public override bool CheckWinCon()
    {
        return true;
    }

    public override void UpdateUI()
    {
        string progressText = "";
        OnWinConUpdate.Invoke(progressText, CheckWinCon());
    }

    public override void UpdateWinCon()
    {
        
    }
}
