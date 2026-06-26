using UnityEngine;

public class Curtains : MonoBehaviour
{
    /// <summary>
    /// Called at the end of the close curtains animation
    /// </summary>
    public void ActivateWinConAnimation()
    {
        GameManager.Instance.ActivateWinConAnimation();
        PauseManager.instance.SetPause("curtains", 1f);
    }

    public void StopGame()
    {
        PauseManager.instance.SetPause("curtains", 0f);
    }
}
