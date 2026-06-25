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
