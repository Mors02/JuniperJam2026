using AbyssWorks.FMODAudioManager;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    
    private Animator _curtainAnimator;

    [SerializeField]
    private GameObject _mainMenu;

    [SerializeField]
    private GameObject _credits;

    [SerializeField] private FMODAudioScriptable volumeAudioScriptable;

    FMODAudioManager _audioManager;


    const string MUSICKEY = "MusicVolume";
    const string SFXKEY = "SfxVolume";

    const string MUSICPARAM = "MX Volume";
    const string SFXPARAM = "SFX Volume";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _audioManager = FMODAudioManager.Instance;

        if (!_audioManager.IsPlaying(volumeAudioScriptable))
            _audioManager.PlayAudio(volumeAudioScriptable);

        if (PlayerPrefs.HasKey(MUSICKEY))
        {
            _audioManager.SetGlobalParameter(MUSICPARAM, PlayerPrefs.GetFloat(MUSICKEY));
        }
        if (PlayerPrefs.HasKey(SFXKEY))
        {
            _audioManager.SetGlobalParameter(SFXPARAM, PlayerPrefs.GetFloat(SFXKEY));
        }

        GameObject.FindGameObjectWithTag("Curtains").TryGetComponent(out _curtainAnimator);
        Debug.Log("Entered");
        StartCoroutine(WaitForOpening());
    }


    public void StartGame()
    {
        StartCoroutine(ChangeSceneRoutine());
        _curtainAnimator.SetTrigger("Exit");
    }

    private IEnumerator ChangeSceneRoutine()
    {
        yield return new WaitForSeconds(0.90f);
        SceneManager.LoadScene("UITutorial");
    }

    private IEnumerator WaitForOpening()
    {
        yield return new WaitForSecondsRealtime(0.20f);
        _curtainAnimator.SetTrigger("Open");
    }

    public void ChangeSection()
    {
        _curtainAnimator.SetTrigger("Exit");
        StartCoroutine(WaitForCloseCurtains());

    }

    public IEnumerator WaitForCloseCurtains()
    {
        yield return new WaitForSeconds(1f);
        
        _mainMenu.SetActive(!_mainMenu.activeSelf);
        _credits.SetActive(!_credits.activeSelf);
    
        _curtainAnimator.SetTrigger("Open");
    }
}
