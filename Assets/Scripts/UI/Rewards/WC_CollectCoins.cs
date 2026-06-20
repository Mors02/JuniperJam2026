using UnityEngine;

[CreateAssetMenu(fileName = "WC_CollectCoins", menuName = "Scriptable Objects/WinCons/WC_CollectCoins")]
public class WC_CollectCoins : WheelReward
{   
    public override void Execute()
    {
        Debug.Log("Changes the gameObject boolean to check the correct win con");
    }
}
