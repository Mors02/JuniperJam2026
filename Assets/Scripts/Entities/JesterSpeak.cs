using AbyssWorks.FMODAudioManager;
using UnityEngine;

public class JesterSpeak : MonoBehaviour
{
    [SerializeField] private FMODAudioScriptable dialogueAudio;

    public void PlayDialogueAudio()
    {
        if (FMODAudioManager.Instance && dialogueAudio)
            FMODAudioManager.Instance.PlayOnce(dialogueAudio);
    }
}
