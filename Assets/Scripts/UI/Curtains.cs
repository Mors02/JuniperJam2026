using AbyssWorks.FMODAudioManager;
using UnityEngine;

public class Curtains : MonoBehaviour
{
    [SerializeField] private FMODAudioScriptable _openAudio;
    [SerializeField] private FMODAudioScriptable _closeAudio;

    /// <summary>
    /// Called at the end of the close curtains animation
    /// </summary>
    public void ActivateWinConAnimation()
    {
        GameManager.Instance.ActivateWinConAnimation();
        if (PauseManager.instance) PauseManager.instance.SetPause("curtains", 1f);
    }

    public void StopGame()
    {
        if (PauseManager.instance) PauseManager.instance.SetPause("curtains", 0f);
    }

    public void OpenAudio()
    {
        if (FMODAudioManager.Instance && _openAudio)
            FMODAudioManager.Instance.PlayOnce(_openAudio);
    }

    public void CloseAudio()
    {
        if (FMODAudioManager.Instance && _closeAudio)
            FMODAudioManager.Instance.PlayOnce(_closeAudio);
    }
}
