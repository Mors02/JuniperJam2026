using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    
    private Animator _curtainAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        SceneManager.LoadScene("Tutorial level");
    }

    private IEnumerator WaitForOpening()
    {
        yield return new WaitForSecondsRealtime(0.20f);
        _curtainAnimator.SetTrigger("Open");
    }
}
