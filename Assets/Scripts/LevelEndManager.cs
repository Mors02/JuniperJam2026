using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndManager : MonoBehaviour
{
    public static LevelEndManager instance;

    private Animator _curtainAnimator;

    private Coroutine _coroutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject.FindGameObjectWithTag("Curtains").TryGetComponent(out _curtainAnimator);
    }

    public void CloseAndLoadScene(string sceneName)
    {
        _coroutine ??= StartCoroutine(CloseAndLoadEnumerator(sceneName));
    }

    IEnumerator CloseAndLoadEnumerator(string sceneName)
    {
        _curtainAnimator.Play("Curtain_close", 0, 0);

        float elapsed = 0;
        while (elapsed < 1)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _coroutine = null;

        SceneManager.LoadScene(sceneName);
    }
}
