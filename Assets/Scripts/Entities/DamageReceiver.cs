using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    
    [SerializeField]
    private int _lives;
    public int Lives => _lives;
    private int _currentLives;

    [SerializeField]
    private bool _canTakeDamage;

    [Range(0f, 5f)]
    [SerializeField]
    private float _invicibilityTime;

    public UnityEvent<int> OnHitReceived;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentLives = GameManager.Instance.CurrentLives;
        if (OnHitReceived == null)
            OnHitReceived = new UnityEvent<int>();
    }

    public void ReceiveDamage()
    {
        if (!_canTakeDamage)
            return;

        _animator.SetTrigger("Hit");
        this._currentLives--;
        UpdateGameManager();
       
        OnHitReceived.Invoke(_currentLives);
        _canTakeDamage = false;
        _animator.SetBool("Invincible", true);
        StartCoroutine("BecomeInvincible");
        
        if (_currentLives <= 0)
            Debug.Log("Death");
    }

    public void Heal()
    {
        this._currentLives++;
        UpdateGameManager();

        OnHitReceived.Invoke(_currentLives);

        if (_currentLives >= _lives)
            _currentLives = _lives;
    }

    private IEnumerator BecomeInvincible()
    {
        yield return new WaitForSeconds(_invicibilityTime);
        _canTakeDamage = true;
        _animator.SetBool("Invincible", false);
    }


    private void UpdateGameManager()
    {
        GameManager.Instance.CurrentLives = _currentLives;
    }

}
