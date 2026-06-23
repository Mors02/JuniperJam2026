using System;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D _rb;
    private Collider2D _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private LayerMask _collisionMask;
    private Transform _poolParentTransform;
    
    

    [Header("Bullet Parameters")]    
    private float _bulletSpeed = 10f;
    private int _bulletDamage = 10;
    private float _bulletLifetime = 5f;
    private float _lifetimeTimer = 0f;
    private bool _initialized = false;
    private bool _active = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true; // Set collider to trigger for collision detection without physics response
        _collider.enabled = false; // Start with collider disabled until the bullet is fired
        _collider.includeLayers = _collisionMask;
    }

    void FixedUpdate()
    {
        if (_active)
            HandleLifetime();
    }

    public void Initialize(float bulletSpeed, int bulletDamage, float bulletLifetime, Transform poolParent)
    {
        _bulletSpeed = bulletSpeed;
        _bulletDamage = bulletDamage;
        _bulletLifetime = bulletLifetime;
        transform.position = poolParent.position; // Start at the pool's position
        _poolParentTransform = poolParent;
    }

    public void Fire(Vector2 direction)
    {
        gameObject.SetActive(true);
        _collider.enabled = true; // Enable collider when fired
        _active = true;
        _rb.linearVelocity = direction.normalized * _bulletSpeed;
    }

    public bool IsActive()
    {
        return _active;
    }

    private void Deactivate()
    {
        print("Deactivating bullet");
        _active = false;
        gameObject.SetActive(false);
        _rb.linearVelocity = Vector2.zero;
        transform.position = _poolParentTransform.position; // Move back to pool position            
    }

    private void HandleLifetime()
    {
       float deltaTime = Time.fixedDeltaTime;
        _lifetimeTimer += deltaTime;
        if (_lifetimeTimer >= _bulletLifetime)
        {
            Deactivate();
            _lifetimeTimer = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_active)
            return;

        if (collision.gameObject.TryGetComponent<DamageReceiver>(out DamageReceiver receiver))
        {
            receiver.ReceiveDamage(); 
        }
        Deactivate();
    }
}



