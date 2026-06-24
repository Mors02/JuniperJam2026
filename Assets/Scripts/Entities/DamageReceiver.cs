using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class DamageReceiver : MonoBehaviour
{
    private ITakeDamage _damageable;
    private int _currentHealth;
    public int CurrentHealth => _currentHealth;
    [SerializeField]
    private int _maxHealth;
    [SerializeField]
    private bool _canTakeDamage;

    [Range(0f, 5f)]
    [SerializeField]
    private float _invicibilityTime;

    public UnityEvent<int> OnHealthChanged;
    public Action OnInvincibilityStart;
    public Action OnInvincibilityEnd;
    public Action OnDeath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (OnHealthChanged == null)
            OnHealthChanged = new UnityEvent<int>();
    }

    public void Initialize(ITakeDamage damageable, int currentHealth = -1)
    {
        _damageable = damageable;
        _currentHealth = currentHealth == -1 ? _maxHealth : currentHealth;  
        _canTakeDamage = true;
    }

    public void ReceiveDamage(DamageInfo damageInfo = new DamageInfo())
    {
        if (!_canTakeDamage)
            return;

        this._currentHealth -= damageInfo.damage;
        OnHealthChanged.Invoke(_currentHealth);
        if (_currentHealth <= 0)
        {
            OnDeath?.Invoke();
            return;
        }
        _damageable.TakeDamage(damageInfo);
        _canTakeDamage = false;
        OnInvincibilityStart?.Invoke();
        StartCoroutine(BecomeInvincible());
    }

    public void Heal(int amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        OnHealthChanged.Invoke(_currentHealth);
    }

    private IEnumerator BecomeInvincible()
    {
        yield return new WaitForSeconds(_invicibilityTime);
        _canTakeDamage = true;
        OnInvincibilityEnd?.Invoke();
    }
}
