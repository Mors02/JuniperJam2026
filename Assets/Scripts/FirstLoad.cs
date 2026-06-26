using AbyssWorks.FMODAudioManager;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FirstLoad : MonoBehaviour
{
    public string startSceneName;
    public string startGameString = "Click to start";

    public FMODAudioPreloader audioPreloader;
    public UILoadingBar loadingSlider;
    public TextMeshProUGUI loadingTextGUI;
    public Button button;

    bool isDoneLoading = false;
    bool isSceneLoading = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadEnumerator());
        button.interactable = false;
    }

    IEnumerator LoadEnumerator()
    {
        if (loadingSlider) loadingSlider.UpdateBar(0);

        if (audioPreloader)
        {
            audioPreloader.PreloadAllAudio();

            while (!audioPreloader.HasLoaded)
            {
                if (loadingSlider) 
                    loadingSlider.UpdateBar(0.50f);

                yield return null;
            }
        }

        if (loadingSlider) loadingSlider.UpdateBar(1);

        if (loadingTextGUI) loadingTextGUI.text = startGameString;

        isDoneLoading = true;
        button.interactable = true;
    }

    public void StartScene()
    {
        print("Itworks");
        SceneManager.LoadSceneAsync(startSceneName);
    }
}