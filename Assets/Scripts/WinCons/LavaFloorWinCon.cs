using UnityEngine;

public class LavaFloorWinCon : WinCon
{
    public LavaFloorWinCon(string description) : base(WinConType.FloorIsLava, description)
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
