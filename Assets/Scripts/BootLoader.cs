using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gates game startup until all FMOD banks have finished loading.
/// On WebGL, FMOD streams banks asynchronously, so audio events are not
/// available immediately. Entering gameplay before banks finish causes
/// "Event not found" errors. Place this on a GameObject in a lightweight
/// boot scene that is first in the build order.
/// </summary>
public class BootLoader : MonoBehaviour
{
    [SerializeField]
    private string _nextScene = "MainMenu";

    private IEnumerator Start()
    {
        while (!RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        while (RuntimeManager.AnySampleDataLoading())
        {
            yield return null;
        }

        SceneManager.LoadScene(_nextScene);
    }
}
