using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UITutorial : MonoBehaviour
{
    [SerializeField]
    private List<TutorialStep> _steps;

    [SerializeField]
    private TMP_Text _dialogueText;

    [SerializeField]
    private Animator _jesterAnimator;

    [SerializeField]
    private float _characterCooldown;

    [SerializeField]
    private UIImageContainer _imageContainer;

    [SerializeField]
    private Button _backButton, _nextButton;

    private int _activeStep;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PauseManager.instance.SetPause("tutorial", 1);
        GameObject.FindGameObjectWithTag("Curtains").GetComponent<Animator>().SetTrigger("Open");
        _activeStep = -1;
        if (_steps[0] == null)
            return;

        Next();
        //StartCoroutine(TypingText(_steps[_activeStep].Text));      
    }

    public void Update()
    {
        //PauseManager.instance.SetPause("tutorial", 1);
        
        //Debug.Log(PauseManager.instance.GetPause("curtains"));
    }

    public IEnumerator TypingText(string text)
    {
        _jesterAnimator.SetBool("Talking", true);
        _dialogueText.text = "";
        foreach (char c in text)
        {
            yield return new WaitForSeconds(_characterCooldown);
            _dialogueText.text += c;
        }
        _jesterAnimator.SetBool("Talking", false);
    }

    public void Next()
    { 
        _activeStep++;
        if (_activeStep < _steps.Count)
        {
            _backButton.interactable = true;
            ChangeActiveStep();
        }
        
        
        if (_activeStep == _steps.Count)
        {
            StartCoroutine(ChangeSceneRoutine());
            GameObject.FindGameObjectWithTag("Curtains").GetComponent<Animator>().SetTrigger("Exit");    
        }
    }

    private IEnumerator ChangeSceneRoutine()
    {
        yield return new WaitForSeconds(0.90f);
        SceneManager.LoadScene("Tutorial level");
    }

    public void Before()
    {
        if (_activeStep > 0)
        {
            _nextButton.interactable = true;
            _activeStep--;
            ChangeActiveStep();
            
            
        }
        
        if (_activeStep == 0)
        {
            _backButton.interactable = false;    
        }
    }

    private void ChangeActiveStep()
    {
        StopAllCoroutines();
            
        if (_steps[_activeStep].ShouldChangeImage)
            _imageContainer.Change(_steps[_activeStep].TutorialImage);
        
        StartCoroutine(TypingText(_steps[_activeStep].Text));
    }


}


[Serializable]
public class TutorialStep
{   
    [TextArea]
    [SerializeField]
    private string _text;

    public string Text => _text;
    [SerializeField]
    public Sprite TutorialImage;

//    public Sprite TutorialImage => TutorialImage;

    [SerializeField]
    private bool _shouldChangeImage;

    public bool ShouldChangeImage => _shouldChangeImage;
}