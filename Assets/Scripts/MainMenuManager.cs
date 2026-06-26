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
        SceneManager.LoadScene("CharacterScene");
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
