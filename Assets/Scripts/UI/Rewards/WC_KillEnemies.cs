using UnityEngine;

[CreateAssetMenu(fileName = "WC_KillEnemies", menuName = "Scriptable Objects/WinCons/WC_KillEnemies")]
public class WC_KillEnemies : WheelReward
{
    public override void Execute()
    {
        GameManager.Instance.SetCurrentWinCondition(WinConType.KillEnemies, this);
    }
}
