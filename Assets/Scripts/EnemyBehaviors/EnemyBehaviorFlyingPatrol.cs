using AbyssWorks.FMODAudioManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// This class defines a flying enemy that patrols between two points in the air.
/// </summary>
public class EnemyBehaviorFlyingPatrol : MonoBehaviour, ITakeDamage
{
    [Header("References")]
    private Rigidbody2D _rb;
    private Collider2D _collider;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private Animator _animator;
    [SerializeField] private Transform _patrolMarkerA;
    [SerializeField] private Transform _patrolMarkerB;
    private Vector3 _patrolMarkerAPosition;
    private Vector3 _patrolMarkerBPosition;
    private DamageReceiver _damageReceiver;

    [SerializeField] private FMODAudioScriptable _popAudio;

    [Header("Damage")]
    [SerializeField] private List<Hitbox> _hitboxes;
    [SerializeField] private int _contactDamage;
    [SerializeField] private Vector2 _knockback;
    private DamageInfo _contactDamageInfo;

    [Tooltip("What is considered ground for grounded checks")]
    [SerializeField]
    private LayerMask _floorMask;
    private ContactFilter2D _contactFilter;

    [Header("Movement")]
    [SerializeField] private bool _flipStartingFacing;
    [Tooltip("The speed at which the enemy moves")]
    [SerializeField]
    private float _moveSpeed = 1f;
    [SerializeField]
    private float _turnDelay = 0.5f;
    private float _turnTimer = 0f;

    [Header("HurtAnimation")]
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _flashColor;
    [SerializeField] private float _flashInterval;
    [SerializeField] private float _flashDuration;

    [Header("Stasis Parameters")]
    [SerializeField] private Color _stasisColor;

    private MovementState _movementState = MovementState.Moving;
    private enum MovementState
    {
        Moving,
        Turning,
        Stopped
    };
    private bool _headingToA = true;

    bool _dead = false;
    bool _isStasis = false;
    bool _isKnockedBack = false;
    float _knockbackTimer = 0f;
    float _knockbackStillDuration = 0.5f;

    void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _damageReceiver = GetComponent<DamageReceiver>();
        _damageReceiver.Initialize();
        _damageReceiver.OnDeath += OnDeathStart;
        _rb = GetComponent<Rigidbody2D>();
        _contactFilter.SetLayerMask(_floorMask);
        _contactFilter.useTriggers = false;
        _patrolMarkerAPosition = _patrolMarkerA.position;
        _patrolMarkerBPosition = _patrolMarkerB.position;
        _patrolMarkerA.gameObject.SetActive(false);
        _patrolMarkerB.gameObject.SetActive(false);
        this.transform.position = new Vector2((_patrolMarkerAPosition.x + _patrolMarkerBPosition.x) / 2f, (_patrolMarkerAPosition.y + _patrolMarkerBPosition.y) / 2f); // Start at marker A
        if (_flipStartingFacing)
        {
            Vector3 s = transform.localScale;
            s.x *= -1f;
            transform.localScale = s;
        }
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

    public void PlayPopAudio()
    {
        if (FMODAudioManager.Instance && _popAudio)
            FMODAudioManager.Instance.PlayOnce(_popAudio, transform.position, true);
    }

    private void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            OnDeathStart();
        }
    }

    private void FixedUpdate()
    {
        if (_dead)
            return;

        if (_isKnockedBack)
        {
            _rb.linearVelocity *= 0.9f;
            // Possible damping of the knockback force
            if (_rb.linearVelocity.magnitude <= 0.01f)
            {
                _knockbackTimer += Time.deltaTime;
                if (_knockbackTimer >= _knockbackStillDuration)
                {
                    _isKnockedBack = false;
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
        switch (_movementState)
        {
            case MovementState.Moving:
                HandleMovement();
                break;
            case MovementState.Turning:
                HandleTurning();
                break;
            case MovementState.Stopped:
                HandleStopping();
                break;
        }
    }

    private void HandleMovement()
    {
        Vector2 targetPosition = _headingToA ? _patrolMarkerAPosition : _patrolMarkerBPosition;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            StartTurning();
        }
        else
        {
            _rb.linearVelocity = direction * _moveSpeed;
        }
    }

    private void StartTurning()
    {
        _movementState = MovementState.Turning;
        _turnTimer = 0f;
        _rb.linearVelocity = Vector2.zero;
        _headingToA = !_headingToA; // Switch the target marker
    }

    private void HandleTurning()
    {
        _turnTimer += Time.fixedDeltaTime;
        if (_turnTimer >= _turnDelay)
        {
            FinishTurning();
        }
    }

    private void FinishTurning()
    {
        // Flip facing by inverting localScale.x
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
        _movementState = MovementState.Moving;
    }

    private void StartStopping()
    {
        _movementState = MovementState.Stopped;
        _rb.linearVelocity = Vector2.zero;
    }

    private void HandleStopping()
    {
        _rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// lol
    /// </summary>
    private void StopStopping()
    {
        _movementState = MovementState.Moving;
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
            _rb.linearVelocity = Vector2.zero;
            _rb.linearVelocity = damageInfo.force;
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

    private void OnDeathStart()
    {
        foreach (var hitbox in _hitboxes)
            hitbox.gameObject.SetActive(false);
        _collider.enabled = false;
        _rb.constraints = RigidbodyConstraints2D.FreezePosition;

        if (GameManager.Instance.CurrentWinCon.Type == WinConType.KillEnemies)
            GameManager.Instance.CurrentWinCon.UpdateWinCon();
        
        GameManager.Instance.KilledEnemies++;

        _dead = true;
        //print("S");
        _animator.SetTrigger("Death");
    }

    public void OnDeathEnd()
    {
        this.gameObject.SetActive(false);
    }
}
