using AbyssWorks.FMODAudioManager;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public FMODAudioScriptable backgroundAudio;

    FMODAudioManager _audioManager;

    private void Awake()
    {
        _audioManager = FMODAudioManager.Instance;

        if (_audioManager && backgroundAudio)
            FMODAudioManager.Instance.RegisterAudio(backgroundAudio);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_audioManager && backgroundAudio && !_audioManager.IsPlaying(backgroundAudio))
            FMODAudioManager.Instance.PlayAudio(backgroundAudio);
    }
}
