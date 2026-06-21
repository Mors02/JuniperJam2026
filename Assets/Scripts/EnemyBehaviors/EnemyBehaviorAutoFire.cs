using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBehaviorAutoFire : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _bulletPoolTransform;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private int _poolSize = 10;

    private List<EnemyBullet> _bulletPool = new List<EnemyBullet>();
    
    [Header("Firing Parameters")]
    public bool CanFire = true;
    public bool FacingRight = true;
    [SerializeField] private float _fireInterval = 1f;
    [Tooltip("The time it takes for the enemy to wind up before firing (after the fire interval)")]
    [SerializeField] private float _fireWindupTime = 0.5f;
    [SerializeField] private float _fireWinddownTime = 0.5f;

    [Header("Bullet Parameters")]
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private int _bulletDamage = 10;
    [SerializeField] private float _bulletLifetime = 5f;
    private AutoFireState _currentState = AutoFireState.Idle;
    private float _fireTimer = 0f;
    private enum AutoFireState
    {
        Idle,
        Windup,
        Firing
    }

    void Awake()
    {
        // Initialize the bullet pool
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject bullet = Instantiate(_bulletPrefab, _bulletPoolTransform);
            bullet.SetActive(false);
            _bulletPool.Add(bullet.GetComponent<EnemyBullet>());
        }
    }

    void FixedUpdate()
    {
        switch (_currentState)
        {
            case AutoFireState.Idle:
                HandleIdle();
                break;
            case AutoFireState.Windup:
                HandleWindup();
                break;
            case AutoFireState.Firing:
                HandleFiring();
                break;
        }   
    } 

    private void HandleIdle()
    {
        _fireTimer += Time.fixedDeltaTime;
        if (_fireTimer >= _fireInterval)
        {            
            _fireTimer = 0f;
            StartWindup();
        }
    }

    private void StartWindup()
    {
        print("Starting windup");
        _currentState = AutoFireState.Windup;
    }

    private void HandleWindup()
    {
        _fireTimer += Time.fixedDeltaTime;
        if (_fireTimer >= _fireWindupTime)
        {
            _fireTimer = 0f;
            StartFiring();
        }
    }

    private void StartFiring()
    {
        print("Starting firing");
        _currentState = AutoFireState.Firing;
        EnemyBullet bullet = FindInactiveBullet();
        if (bullet != null)
        {
            print("Firing bullet from pool");
            Vector2 fireDirection = FacingRight ? Vector2.right : Vector2.left;
            bullet.Initialize(_bulletSpeed, _bulletDamage, _bulletLifetime, _bulletPoolTransform);
            bullet.Fire(fireDirection);
        }
        else
        {
            Debug.LogError("No inactive bullets available in the pool! Lower the fire rate or increase the pool size.");
        }
    }

    private void HandleFiring()
    {
        _fireTimer += Time.fixedDeltaTime;
        if (_fireTimer >= _fireWinddownTime)
        {
            _fireTimer = 0f;
            StartIdle();
        }
    }

    private void StartIdle()
    {
        _currentState = AutoFireState.Idle;
    }

    private EnemyBullet FindInactiveBullet()
    {
        for(int i = 0; i < _poolSize; i++)
        {
            if (!_bulletPool[i].gameObject.activeInHierarchy && !_bulletPool[i].IsActive())
            {
                return _bulletPool[i];
            }
        }

        // If no inactive bullet is found, return null
        return AddPoolBullet();
    }

    private EnemyBullet AddPoolBullet()
    {
        GameObject bullet = Instantiate(_bulletPrefab, _bulletPoolTransform);
        bullet.SetActive(false);
        EnemyBullet bulletComponent = bullet.GetComponent<EnemyBullet>();
        _bulletPool.Add(bulletComponent);
        bulletComponent.Initialize(_bulletSpeed, _bulletDamage, _bulletLifetime, _bulletPoolTransform);
        return bulletComponent;
    }
}
