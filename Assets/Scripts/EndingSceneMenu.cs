using AbyssWorks.FMODAudioManager;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingSceneMenu : MonoBehaviour
{
    [SerializeField] private Button menuButton;
    [SerializeField] private FMODAudioScriptable _endAudio;
    [SerializeField] private FMODAudioScriptable menuClickAudio;

    private Animator _curtainAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.CurrentWinCon = null;

        GameObject.FindGameObjectWithTag("Curtains").TryGetComponent(out _curtainAnimator);

        StartCoroutine(WaitForOpening());

        if (menuButton)
        {
            menuButton.onClick.AddListener(MainMenu);
        }
    }

    void MainMenu()
    {
        gameObject.SetActive(false);

        if (FMODAudioManager.Instance && menuClickAudio)
            FMODAudioManager.Instance.PlayOnce(menuClickAudio, null, true);

        GameManager.Instance.CurrentWinCon = null;

        if (LevelEndManager.instance)
            LevelEndManager.instance.CloseAndLoadScene("MainMenu");
        else
            SceneManager.LoadSceneAsync("MainMenu");
    }

    private IEnumerator WaitForOpening()
    {
        yield return new WaitForSecondsRealtime(0.20f);
        _curtainAnimator.SetTrigger("Open");

        if (FMODAudioManager.Instance && _endAudio)
            FMODAudioManager.Instance.PlayOnce(_endAudio);
    }
}
