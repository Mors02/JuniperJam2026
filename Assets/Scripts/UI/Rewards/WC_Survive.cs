using UnityEngine;

[CreateAssetMenu(fileName = "WC_Survive", menuName = "Scriptable Objects/WinCons/WC_Survive")]
public class WC_Survive : WheelReward
{
    public override void Execute()
    {
        GameManager.Instance.SetCurrentWinCondition(WinConType.Survive, this);
    }
}
