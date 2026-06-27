using AbyssWorks.FMODAudioManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIIngameMenu : MonoBehaviour
{
    [SerializeField] private InputActionAsset _playerInput;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainmenuButton;
    //[SerializeField] private Button quitButton;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Audio Settings")]
    [SerializeField] private FMODAudioScriptable pauseAudioScriptable;
    [SerializeField] private FMODAudioScriptable resumeAudioScriptable;
    [SerializeField] private FMODAudioScriptable volumeAudioScriptable; 

    bool isBusy = false;

    private InputAction _toggleAction;

    //private Animator _curtainAnimator;

    const int MAXVOLUME = 100;
    const string MUSICKEY = "MusicVolume";
    const string SFXKEY = "SfxVolume";

    const string MUSICPARAM = "MX Volume";
    const string SFXPARAM = "SFX Volume";

    FMODAudioManager _audioManager;

    private void Awake()
    {
        _toggleAction = _playerInput.FindActionMap("Player").FindAction("ToggleMenu");

        _toggleAction.performed += OnToggleMenu;

        if (continueButton)
            continueButton.onClick.AddListener(Continue);
        if (restartButton)
            restartButton.onClick.AddListener(Restart);
        if (mainmenuButton)
            mainmenuButton.onClick.AddListener(MainMenu);
        /*if (quitButton)
            quitButton.onClick.AddListener(Quit);*/

        _audioManager = FMODAudioManager.Instance;

        if (!_audioManager.IsPlaying(volumeAudioScriptable))
            _audioManager.PlayAudio(volumeAudioScriptable);

        if (musicSlider)
        {
            musicSlider.maxValue = MAXVOLUME;

            musicSlider.value = PlayerPrefs.HasKey(MUSICKEY) ? PlayerPrefs.GetFloat(MUSICKEY) : MAXVOLUME;
            _audioManager.SetGlobalParameter(MUSICPARAM, musicSlider.value);

            musicSlider.onValueChanged.AddListener((float value) =>
            {
                PlayerPrefs.SetFloat(MUSICKEY, value);
                _audioManager.SetGlobalParameter(MUSICPARAM, value);
            });
        }

        if (sfxSlider)
        {
            sfxSlider.maxValue = MAXVOLUME;

            sfxSlider.value = PlayerPrefs.HasKey(SFXKEY) ? PlayerPrefs.GetFloat(SFXKEY) : MAXVOLUME;
            _audioManager.SetGlobalParameter(SFXPARAM, sfxSlider.value);

            sfxSlider.onValueChanged.AddListener((float value) =>
            {
                PlayerPrefs.SetFloat(SFXKEY, value);
                _audioManager.SetGlobalParameter(SFXPARAM, value);
            });
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnToggleMenu(InputAction.CallbackContext context)
    {
        if (PauseManager.instance)
        {
            if (gameObject.activeSelf)
            {
                if (resumeAudioScriptable && _audioManager)
                    _audioManager.PlayOnce(resumeAudioScriptable);

                PauseManager.instance.SetPause("InGameMenu", 1);
                gameObject.SetActive(false);
            }
            else
            {
                if (PauseManager.instance.GetPause("curtains") == 0) return;

                if (pauseAudioScriptable && _audioManager)
                    _audioManager.PlayOnce(pauseAudioScriptable);

                PauseManager.instance.SetPause("InGameMenu", 0);

                gameObject.SetActive(true);
            }
        }
    }

    void Continue()
    {
        if (isBusy) return;

        if (resumeAudioScriptable && _audioManager)
            _audioManager.PlayOnce(resumeAudioScriptable);

        if (PauseManager.instance) PauseManager.instance.SetPause("InGameMenu", 1);
        gameObject.SetActive(false);
    }

    void Restart()
    {
        if (isBusy) return;

        if (resumeAudioScriptable && _audioManager)
            _audioManager.PlayOnce(resumeAudioScriptable);

        isBusy = true;

        if (LevelEndManager.instance) 
            LevelEndManager.instance.CloseAndLoadScene(SceneManager.GetActiveScene().name);
        else 
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    void MainMenu()
    {
        if (isBusy) return;

        if (resumeAudioScriptable && _audioManager)
            _audioManager.PlayOnce(resumeAudioScriptable);

        isBusy = true;

        GameManager.Instance.CurrentWinCon = null;

        if (LevelEndManager.instance)
            LevelEndManager.instance.CloseAndLoadScene("MainMenu");
        else
            SceneManager.LoadSceneAsync("MainMenu");
    }

    /*void Quit()
    {
        if (isBusy) return;

        if (Application.platform != RuntimePlatform.WebGLPlayer) Application.Quit();
        else
        {
            //isBusy = true;
            //GameManager.Instance.LoadSceneAsync("QuitScene");
        }
    }*/

    private void OnDestroy()
    {
        _toggleAction.performed -= OnToggleMenu;
    }
}
