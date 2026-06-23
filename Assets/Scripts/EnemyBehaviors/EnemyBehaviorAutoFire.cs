using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBehaviorAutoFire : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _bulletPoolTransform;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private int _poolSize = 10;
    private Vector2 _firePointLocalPosition;

    private List<EnemyBullet> _bulletPool = new List<EnemyBullet>();
    
    [Header("Firing Parameters")]
    public bool CanFire = true;
    // Facing is determined from transform.localScale.x sign (positive = right, negative = left)
    public bool FacingRight = true;
    [SerializeField] private float _fireInterval = 1f;

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
        _firePointLocalPosition = _firePoint.localPosition;
    }

    void Update()
    {
        _spriteRenderer.flipX = FacingRight;

        switch (_currentState)
        {
            case AutoFireState.Idle:
                HandleIdle();
                break;
        }   
    } 

    private void HandleIdle()
    {
        _fireTimer += Time.deltaTime;
        if (_fireTimer >= _fireInterval)
        {            
            _fireTimer = 0f;
            StartWindup();
        }
    }

    private void StartWindup()
    {
        _animator.SetTrigger("Fire");
        _currentState = AutoFireState.Windup;
    }

    /// <summary>
    /// Starts the firing state, which will fire a bullet from the pool.
    /// This is called by the animation controller when the firing animation reaches the point where the bullet should be fired
    /// </summary>
    public void StartFiring()
    {
        _currentState = AutoFireState.Firing;
        EnemyBullet bullet = FindInactiveBullet();
        if (bullet != null)
        {
            Vector2 fireDirection = FacingRight ? Vector2.right : Vector2.left;
            bullet.Initialize(_bulletSpeed, _bulletDamage, _bulletLifetime, _bulletPoolTransform);
            float directionMultiplier = FacingRight ? -1f : 1f;
            _firePoint.localPosition = new Vector2( _firePointLocalPosition.x * directionMultiplier, _firePointLocalPosition.y);
            bullet.transform.position = _firePoint.position;
            bullet.Fire(fireDirection, FacingRight);
        }
        else
        {
            Debug.LogError("No inactive bullets available in the pool! Lower the fire rate or increase the pool size.");
        }
    }

    /// <summary>
    /// Starts the idle state, which will wait for the fire interval before transitioning to the windup state and firing a bullet. 
    /// This is called by the animation controller when the firing animation completes.
    /// </summary>
    public void StartIdle()
    {
        _currentState = AutoFireState.Idle;
    }

    /// <summary>
    /// Returns an inactive bullet from the pool if available, otherwise adds a new bullet to the pool and returns it. This ensures that we always have a bullet to fire, but also allows for dynamic pool expansion if necessary. In practice, you may want to set an upper limit on pool size to prevent excessive memory usage.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Adds a new bullet to the pool and returns it. This is called when all bullets in the pool are active and we need to fire another one. In practice, you may want to set an upper limit on pool size to prevent excessive memory usage.
    /// </summary>
    /// <returns></returns>
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
