using AbyssWorks.FMODAudioManager;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyBehaviorAutoFire : MonoBehaviour, ITakeDamage
{
    [Header("References")]
    [SerializeField] private FMODAudioScriptable _throwAudioScr;
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rb;
    private DamageReceiver _damageReceiver;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _bulletPoolTransform;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private int _poolSize = 10;
    private Vector2 _firePointLocalPosition;

    private List<EnemyBullet> _bulletPool = new List<EnemyBullet>();

    [Header("Damage")]
    [SerializeField] private List<Hitbox> _hitboxes;
    [SerializeField] private int _contactDamage;
    [SerializeField] private Vector2 _knockback;
    private DamageInfo _contactDamageInfo;

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

    [Header("HurtAnimation")]
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _flashColor;
    [SerializeField] private float _flashInterval;
    [SerializeField] private float _flashDuration;

    [Header("Stasis Parameters")]
    [SerializeField] private Color _stasisColor;
    private enum AutoFireState
    {
        Idle,
        Windup,
        Firing,
    }
    bool _dead = true;
    bool _isStasis = false;
    bool _isKnockedBack = false;
    float _knockbackTimer = 0f;
    float _knockbackStillDuration = 0.5f;

    FMODAudioScriptable _audioSFX;

    void Awake()
    {

        _damageReceiver = GetComponent<DamageReceiver>();
        _damageReceiver.Initialize();
        _damageReceiver.OnDeath += OnDeathStart;
        // Initialize the bullet pool
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject bullet = Instantiate(_bulletPrefab, _bulletPoolTransform);
            bullet.SetActive(false);
            _bulletPool.Add(bullet.GetComponent<EnemyBullet>());
        }
        _firePointLocalPosition = _firePoint.localPosition;

        if (_throwAudioScr) _audioSFX = Instantiate(_throwAudioScr);
        InitHitBoxes();
    }


    private void InitHitBoxes()
    {
        _contactDamageInfo = new DamageInfo(_contactDamage, DamageType.Knockback, 0, _knockback);
        foreach (Hitbox hitbox in _hitboxes)
        {
            hitbox.onEnter2D += DealDamageToPlayer;
            hitbox.onStay2D += DealDamageToPlayer;
        }
    }

    private void DealDamageToPlayer(Collider2D collider2D)
    {
        if (collider2D.CompareTag("Player"))
        {
            DamageInfo newDamageInfo = new DamageInfo
            (
                _contactDamageInfo.damage,
                _contactDamageInfo.damageType,
                _contactDamageInfo.duration,
                new Vector2 (
                    collider2D.transform.position.x > this.transform.position.x ? Mathf.Abs(_contactDamageInfo.force.x) : Mathf.Abs(_contactDamageInfo.force.x) * -1,
                    collider2D.transform.position.y > this.transform.position.y ? Mathf.Abs(_contactDamageInfo.force.y) : Mathf.Abs(_contactDamageInfo.force.y) * -1
                )
            );

            collider2D.GetComponent<ITakeDamage>().TakeDamage(newDamageInfo);
        }
    }

    void Update()
    {

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            //OnDeathStart();
        }

        if (_dead)
            return;

        if (_isKnockedBack)
        {
            // Possible damping of the knockback force
            if (_rb.linearVelocity.magnitude <= 0.01f)
            {
                _knockbackTimer += Time.deltaTime;
                if (_knockbackTimer >= _knockbackStillDuration)
                {
                    _isKnockedBack = false;
                    _rb.constraints |= RigidbodyConstraints2D.FreezePositionX;
                    _knockbackTimer = 0f;
                }
            }
            else
            {
                _knockbackTimer = 0f;
            }
            return;
        }

        if (_isStasis)
        {
            return;
        }

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
        if (FMODAudioManager.Instance && _audioSFX)
            FMODAudioManager.Instance.PlayOnce(_audioSFX, transform.position);

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
            _firePoint.localPosition = new Vector2(_firePointLocalPosition.x * directionMultiplier, _firePointLocalPosition.y);
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
        for (int i = 0; i < _poolSize; i++)
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


    Coroutine damageFlashRoutine;
    public void TakeDamage(DamageInfo damageInfo)
    {

        if (damageInfo.damageType == DamageType.Stasis)
        {
            StartCoroutine(StasisCoroutine(damageInfo.duration));
        }
        else if (damageInfo.damageType == DamageType.Knockback)
        {
            if (damageInfo.damage > 0)
            {
                if (damageFlashRoutine != null)
                    StopCoroutine(damageFlashRoutine);
                damageFlashRoutine = StartCoroutine(DamageFlashCoroutine());
            }

            _isKnockedBack = true;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.AddForce(damageInfo.force, ForceMode2D.Impulse);
        }
        else
        {
            if (damageFlashRoutine != null)
                StopCoroutine(damageFlashRoutine);
            damageFlashRoutine = StartCoroutine(DamageFlashCoroutine());
        }

        _damageReceiver.ReceiveDamage(damageInfo.damage);
    }

    private IEnumerator StasisCoroutine(float duration)
    {
        _isStasis = true;
        _spriteRenderer.color = _stasisColor;
        _animator.speed = 0;
        yield return new WaitForSeconds(duration);
        _isStasis = false;
        _spriteRenderer.color = _defaultColor;
        _animator.speed = 1;
    }

    private IEnumerator DamageFlashCoroutine()
    {
        float intervalTimer = 0;
        float durationTimer = 0;
        bool glowIn = true;
        while (durationTimer < _flashDuration)
        {
            intervalTimer += Time.deltaTime;
            durationTimer += Time.deltaTime;
            Color a = glowIn ? _defaultColor : _flashColor;
            Color b = glowIn ? _flashColor : _defaultColor;
            float t = Mathf.Clamp(intervalTimer / _flashInterval, 0, 1);
            _spriteRenderer.color = Color.Lerp(a, b, t);

            if (intervalTimer >= _flashInterval)
            {
                intervalTimer = 0;
                glowIn = !glowIn;
            }

            yield return new WaitForEndOfFrame();
        }
        _spriteRenderer.color = _defaultColor;
    }

    private void OnDestroy()
    {
        if (_audioSFX) Destroy(_audioSFX);
    }

    private void OnDeathStart()
    {
        _dead = true;
        _animator.SetTrigger("Death");
    }

    public void OnDeathEnd()
    {
        print("PPPP");
        this.gameObject.SetActive(false);
    }
}
