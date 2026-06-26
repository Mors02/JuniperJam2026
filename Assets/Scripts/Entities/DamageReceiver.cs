using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField] private int _currentHealth;
    public int CurrentHealth => _currentHealth;
    [SerializeField]
    private int _maxHealth;
    public int MaxHealth => _maxHealth;
    [SerializeField]
    private bool _canTakeDamage;

    [Range(0f, 5f)]
    [SerializeField]
    private float _invicibilityTime;

    public UnityEvent<int> OnHealthChanged;
    public Action OnInvincibilityStart;
    public Action OnInvincibilityEnd;
    public Action OnDeath;

    public bool CanTakeDamage => _canTakeDamage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (OnHealthChanged == null)
            OnHealthChanged = new UnityEvent<int>();
    }

    public void Initialize(int currentHealth = -1)
    {
        _currentHealth = currentHealth == -1 ? _maxHealth : currentHealth;  
        _canTakeDamage = true;
    }

    public void ReceiveDamage(int amount)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - amount);

        OnHealthChanged.Invoke(_currentHealth);
        if (_currentHealth <= 0)
        {
            OnDeath?.Invoke();
            return;
        }
    }

    public void Heal(int amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        OnHealthChanged.Invoke(_currentHealth);
    }

    public void BecomeInvisible()
    {
        if (!_canTakeDamage)
            return;

        _canTakeDamage = false;
        OnInvincibilityStart?.Invoke();
        StartCoroutine(BecomeInvincible());
    }

    private IEnumerator BecomeInvincible()
    {
        yield return new WaitForSeconds(_invicibilityTime);
        _canTakeDamage = true;
        OnInvincibilityEnd?.Invoke();
    }
}
