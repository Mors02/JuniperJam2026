using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    PlayerController _playerController;
    [SerializeField]
    WinconUI _winconUI;
    DamageReceiver _playerDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        _playerController = player.GetComponent<PlayerController>();
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
}
