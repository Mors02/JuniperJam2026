using AbyssWorks.FMODAudioManager;
using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyBehaviorPatrol : MonoBehaviour, ITakeDamage
{
    [Header("References")]
    private Rigidbody2D _rb;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private Animator _animator;
    private DamageReceiver _damageReceiver;


    [Tooltip("What is considered ground for grounded checks")]
    [SerializeField]
    private LayerMask _floorMask;
    private ContactFilter2D _contactFilter;
    private Transform _transform;

    [Header("Damage")]
    [SerializeField] private System.Collections.Generic.List<Hitbox> _hitboxes;
    [SerializeField] private int _contactDamage;
    [SerializeField] private Vector2 _knockback;
    private DamageInfo _contactDamageInfo;

    [Header("Movement")]
    [Tooltip("The speed at which the enemy moves")]
    [SerializeField]
    private float _moveSpeed = 1f;
    private bool _facingRight = false;
    [Tooltip("The distance to check for ledges (raycast down from this distance in front of the enemy)")]
    [SerializeField]
    private float _ledgeCheckDistance = 0.5f;
    [SerializeField]
    private float _ledgeCheckRange = 1f;

    [SerializeField]
    private float _wallCheckDistance = 0.5f;

    [SerializeField]
    private float _turnDelay = 0.5f;
    private float _turnTimer = 0f;
    private MovementState _movementState = MovementState.Moving;

    [Header("HurtAnimation")]
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _flashColor;
    [SerializeField] private float _flashInterval;
    [SerializeField] private float _flashDuration;

    [Header("Stasis Parameters")]
    [SerializeField] private Color _stasisColor;

    [Header("Audio")]
    [SerializeField] private FMODAudioScriptable _walkAudioScr;
    [SerializeField] private FMODAudioScriptable _turnAudioScr;
    [SerializeField] private FMODAudioScriptable _swingAudioScr;

    private enum MovementState
    {
        Moving,
        Turning,
        Stopped
    };
    bool _dead = false;
    bool _isStasis = false;
    bool _isKnockedBack = false;
    float _knockbackTimer = 0f;
    float _knockbackStillDuration = 0.5f;

    FMODAudioScriptable _walkAudio;
    FMODAudioScriptable _turnAudio;
    FMODAudioScriptable _swingAudio;

    FMODAudioManager _audioManager;

    void Awake()
    {
        _damageReceiver = GetComponent<DamageReceiver>();
        _damageReceiver.Initialize();
        _damageReceiver.OnDeath += OnDeathStart;
        _rb = GetComponent<Rigidbody2D>();
        _contactFilter.SetLayerMask(_floorMask);
        _contactFilter.useTriggers = false;
        _transform = transform;

        if (_walkAudioScr) _walkAudio = Instantiate(_walkAudioScr);
        if (_turnAudioScr) _turnAudio = Instantiate(_turnAudioScr);
        if (_swingAudioScr) _swingAudio = Instantiate(_swingAudioScr);
        _audioManager = FMODAudioManager.Instance;
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

    private void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            //OnDeathStart();
        }
    }

    private void FixedUpdate()
    {
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
                //HandleTurning();
                break;
            case MovementState.Stopped:
                HandleStopping();
                break;
        }
    }

    private void HandleMovement()
    {
        if (_audioManager && _walkAudio)
        {
            if (!_audioManager.IsPlaying(_walkAudio)) _audioManager.PlayAudio(_walkAudio);
            _audioManager.SetPosition(_walkAudio, transform.position);
        }

        int direction = _facingRight ? 1 : -1;
        // Check for ledge
        Vector2 ledgeCheckOrigin = (Vector2)_transform.position + Vector2.right * direction * _ledgeCheckDistance;
        Debug.DrawRay(ledgeCheckOrigin, Vector2.down * _ledgeCheckRange, Color.red);
        RaycastHit2D ledgeHit = Physics2D.Raycast(ledgeCheckOrigin, Vector2.down, _ledgeCheckRange, _floorMask);
        if (!ledgeHit)
        {
            StartTurning();
            return;
        }

        // Check for wall
        Vector2 wallCheckOrigin = (Vector2)_transform.position + Vector2.right * 0.5f * direction;
        Debug.DrawRay(wallCheckOrigin, Vector2.right * direction * _wallCheckDistance, Color.blue);
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheckOrigin, Vector2.right * direction, _wallCheckDistance, _floorMask);
        if (wallHit)
        {
            StartTurning();
            return;
        }


        // Move in the current direction
        _rb.linearVelocity = new Vector2(transform.localScale.x * _moveSpeed * direction, _rb.linearVelocity.y);
    }

    private void StartTurning()
    {
        if (_audioManager)
        {
            if (_turnAudio) _audioManager.PlayOnce(_turnAudio, transform.position);
            if (_walkAudio) _audioManager.StopAudio(_walkAudio);
        }

        _animator.SetTrigger("Turn");
        _movementState = MovementState.Turning;
        _turnTimer = 0f;
        _rb.linearVelocity = Vector2.zero;
    }

    private void HandleTurning()
    {
        _turnTimer += Time.fixedDeltaTime;
        if (_turnTimer >= _turnDelay)
        {
            FinishTurning();
        }
    }

    public void FinishTurning()
    {
        _facingRight = !_facingRight;
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

    public void PlaySwingAudio()
    {
        if (_audioManager && _swingAudio) _audioManager.PlayOnce(_swingAudio, transform.position);
    }

    Coroutine damageFlashRoutine;
    public void TakeDamage(DamageInfo damageInfo)
    {
        Debug.Log("TakeDamage " + damageInfo.damageType + " " + damageInfo.damage);

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
        if (_audioManager)
        {
            if (_walkAudio)
            {
                _audioManager.StopAudio(_walkAudio);
                Destroy(_walkAudio);
            }
            if (_swingAudio) Destroy(_swingAudio);
            if (_turnAudio) Destroy(_turnAudio);
        }

    }

    private void OnDeathStart()
    {
        _dead = true;
        _animator.SetTrigger("Death");
    }

    public void OnDeathEnd()
    {
        this.gameObject.SetActive(false);
    }
}