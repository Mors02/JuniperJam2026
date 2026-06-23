using UnityEngine;

public class Curtains : MonoBehaviour
{
    /// <summary>
    /// Called at the end of the close curtains animation
    /// </summary>
    public void ActivateWinConAnimation()
    {
        GameManager.Instance.ActivateWinConAnimation();
        Time.timeScale = 1f;
    }

    public void StopGame()
    {
        Time.timeScale = 0f;
    }
}
