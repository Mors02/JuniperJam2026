using UnityEngine;

[CreateAssetMenu(fileName = "WC_Ringleader", menuName = "Scriptable Objects/WinCons/WC_Ringleader")]
public class WC_Ringleader : WheelReward
{

    public override void Execute()
    {
        GameManager.Instance.SetCurrentWinCondition(WinConType.RingLeader, this);
    }
}