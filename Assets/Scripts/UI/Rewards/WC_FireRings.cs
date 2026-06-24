using UnityEngine;

[CreateAssetMenu(fileName = "WC_FireRings", menuName = "Scriptable Objects/WinCons/WC_FireRings")]
public class WC_FireRings : WheelReward
{
    public override void Execute()
    {
        GameManager.Instance.SetCurrentWinCondition(WinConType.FireRings, this);
    }
}
