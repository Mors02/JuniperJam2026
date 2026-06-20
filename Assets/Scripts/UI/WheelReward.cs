using UnityEngine;

[CreateAssetMenu(fileName = "WheelReward")]
public abstract class WheelReward : ScriptableObject
{
    /// <summary>
    /// Name of the prize
    /// </summary>
    public string Name;

    /// <summary>
    /// Description of the prize
    /// </summary>
    [TextArea]
    public string Description;

    /// <summary>
    /// Executes the relative reward from the wheel
    /// </summary>
    public abstract void Execute();
}
