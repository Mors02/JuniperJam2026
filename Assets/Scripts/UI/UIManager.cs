using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //PlayerSMController _playerController;
    [SerializeField]
    WinconUI _winconUI;
    [SerializeField]
    WheelSpinning _wheelSpinning;

    [SerializeField]
    Animator _curtainAnimator;

    DamageReceiver _playerDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        //_playerController = player.GetComponent<PlayerSMController>();
        _playerDamage = player.GetComponent<DamageReceiver>();
        
        _playerDamage.OnHitReceived.AddListener(UpdateLifeUI);
    }

    public void UpdateLifeUI(int _currentLives)
    {
        Debug.Log("Now has " + _currentLives);
    }

    public void UpdateWinConUI(string progressText, bool completed)
    {
        _winconUI.UpdateWinConUI(progressText, completed);
    }

    public void UpdateWinConDescriptionUI(WinCon winCon)
    {
        _winconUI.UpdateWinConDescriptionUI(winCon);
    }

    public void OpenCurtains()
    {
        _curtainAnimator.SetTrigger("Open");
        _winconUI.Enter();
        //_playerController.StartCharacter();
    }
}
