using UnityEngine;

[CreateAssetMenu(fileName = "WC_LavaFloor", menuName = "Scriptable Objects/WinCons/WC_LavaFloor")]
public class WC_LavaFloor : WheelReward
{
    public override void Execute()
    {
        GameManager.Instance.SetCurrentWinCondition(WinConType.FloorIsLava, this);
    }
}
