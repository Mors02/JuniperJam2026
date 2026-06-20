using UnityEngine;

public class UIManager : MonoBehaviour
{
    PlayerController _playerController;
    DamageReceiver _playerDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        _playerController = player.GetComponent<PlayerController>();
        _playerDamage = player.GetComponent<DamageReceiver>();
        
        _playerDamage.OnHitReceived.AddListener(UpdateLifeUI);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLifeUI(int _currentLives)
    {
        Debug.Log("Now has " + _currentLives);
    }
}
