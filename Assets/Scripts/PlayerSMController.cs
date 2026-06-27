using AbyssWorks.AnimatorSignal;
using AbyssWorks.FMODAudioManager;
using AbyssWorks.ParasiteBehaviour;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerSMController : MonoBehaviour, ITakeDamage
{
    public enum StateExecutionType
    {
        Enter,
        Update,
        FixedUpdate,
        Exit,
        CollisionEnter,
        CollisionStay,
        CollisionExit,
        TriggerEnter,
        TriggerStay,
        TriggerExit,
    }

    public enum PlayerState
    {
        Idle,
        Run,
        Jump,
        Glide,
        Fall,
        Land,
        Special,
        Death,
        Damaged
    }

    [SerializeField] private ParticleSystem _dustParticles;

    [Header("Movement")]
    [Tooltip("Speed of the character")]
    [SerializeField] private float _speed;
    [Tooltip("Acceleration of the character to get to max speed")]
    [SerializeField] private float _acceleration;
    [Tooltip("Max acceleration each frame to get to the desired speed")]
    [SerializeField] private float _maxAcceleration;
    [Tooltip("Curve for change of direction strength (Do not change unless you know what you're doing)")]
    [SerializeField] private AnimationCurve _accelerationFactorFromDot;

    [Header("Jump")]
    [Tooltip("The force of the jump")]
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _downwardForce;
    [Tooltip("How many jumps (also considering the first jump from the ground)")]
    [SerializeField] private float _totalJumps;
    [Tooltip("How much time for the player to jump after leaving and hedge")]
    [SerializeField] private float _coyoteTime;

    [Header("Glide")]
    [Tooltip("How slowly the player falls during glide")]
    [SerializeField, Min(0)] private float _glideGravityScale = 1f;

    [Header("Animation")]
    [SerializeField] private string idleAnim;
    [SerializeField] private string runAnim;
    [SerializeField] private string jumpAnim;
    [SerializeField] private string glideAnim;
    [SerializeField] private string fallAnim;
    [SerializeField] private string landAnim;
    [SerializeField] private string damagedAnim;
    [SerializeField] private string deathAnim;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Head Check")]
    [SerializeField] private Transform _headCheck;
    [SerializeField] private float _headCheckRadius = 0.1f;
    [SerializeField] private LayerMask headMask;

    [Header("Floor")]
    [SerializeField] public Transform _groundCheck;
    [SerializeField] private Vector2 _groundedSize = Vector2.one;
    [SerializeField] private LayerMask _floorMask;

    [Header("Ability")]
    public ParasiteBehaviourLibrary abilityLibrary;
    public string dashAbName;
    public string testAbility;

    [Header("Audio")]
    [SerializeField] private FMODAudioScriptable _jumpAudio;
    [SerializeField] private FMODAudioScriptable _landAudio;
    [SerializeField] private FMODAudioScriptable _damagedAudio;
    [SerializeField] private FMODAudioScriptable _glideAudio;
    [SerializeField] private FMODAudioScriptable _deathAudio;

    [Header("Misc")]
    [SerializeField] private InputActionAsset _playerInput;
    [SerializeField] private UIAbilityWheelSpin _wheelSpin;
    [SerializeField] private PlayerEffects _playerEffect;

    [Header("Debug")]
    public PlayerState debugState;

    private PlayerState _currentState = PlayerState.Idle;

    #region Private Components
    private Rigidbody2D _rb;
    private Animator _animator;
    private AnimationSubscriber _animationSubscriber;
    private DamageReceiver _damageReceiver;
    #endregion

    private Vector2 _currentMovement;
    private float _move;
    private bool _isGrounded;
    private float _numberOfJumps;
    private float _mayJump;
    private bool _isInstantJump = false;
    private bool _hasJumpForce = false;
    private bool _canGlide = false;
    private float _baseGravityScale;

    #region Input actions
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _attackAction;
    #endregion

    private Ability _dashAbility;
    private Ability _ability;

    [NonSerialized] public string wheelAbilityName;

    private RigidbodyConstraints2D _baseConstraints;
    public RigidbodyConstraints2D BaseConstraints => _baseConstraints;

    private void Awake()
    {
        if (!_wheelSpin)
        {
            var playerCanvas = GameObject.FindWithTag("PlayerCanvas").GetComponent<UIPlayerCanvas>();
            _wheelSpin = playerCanvas.abilityWheelSpin;
        }

        _animator = GetComponent<Animator>();
        _damageReceiver = GetComponent<DamageReceiver>();
        _animationSubscriber = GetComponent<AnimationSubscriber>();
        _rb = GetComponent<Rigidbody2D>();
        _baseConstraints = _rb.constraints;
        _baseGravityScale = _rb.gravityScale;

        _moveAction = _playerInput.FindActionMap("Player").FindAction("Move");
        _jumpAction = _playerInput.FindActionMap("Player").FindAction("Jump");
        _dashAction = _playerInput.FindActionMap("Player").FindAction("Sprint");
        _attackAction = _playerInput.FindActionMap("Player").FindAction("Attack");

        _animationSubscriber.SubscribeAction("PlayerLand", () =>
        {
            SwitchState(PlayerState.Idle);
        });
        _animationSubscriber.SubscribeAction("GlideEnd", () =>
        {
            SwitchState(PlayerState.Fall);
        });
        _animationSubscriber.SubscribeAction("DamagedEnd", () =>
        {
            SwitchState(PlayerState.Idle);
        });
        _animationSubscriber.SubscribeAction("Jump", Jump);

        _animationSubscriber.SubscribeAction("PlayerDead", () =>
        {
            if (LevelEndManager.instance)
                LevelEndManager.instance.CloseAndLoadScene(SceneManager.GetActiveScene().name);
            else
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        });


        _wheelSpin.onSpinEnd += (string abilityName) =>
        {
            wheelAbilityName = abilityName;
        };

        _numberOfJumps = 0;
        _mayJump = 0;

        _damageReceiver.Initialize();
        _damageReceiver.OnHealthChanged.Invoke(_damageReceiver.MaxHealth);
        /*_damageReceiver.OnDeath += () =>
        {
            SwitchState(PlayerState.Death);
        };*/
        _damageReceiver.OnInvincibilityStart += StartInvincibilityAnimation;
        _damageReceiver.OnInvincibilityEnd += StopInvincibilityAnimation;
    }

    private void OnEnable()
    {
        _jumpAction.performed += OnJump;
        _jumpAction.canceled += OnJumpRelease;

        _dashAction.performed += OnDash;

        _attackAction.performed += OnAttack;
    }

    private void OnDisable()
    {
        _jumpAction.performed -= OnJump;
        _jumpAction.canceled -= OnJumpRelease;

        _dashAction.performed -= OnDash;

        _attackAction.performed -= OnAttack;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _dashAbility = abilityLibrary.GetAnyParasiteB(dashAbName) as Ability;

        ExecuteState(StateExecutionType.Enter);
    }

    private bool onMovingPlatform = false;
    private GondolaScript currentPlatform;
    private Vector2 platformVelocity = Vector2.zero;
    private void FixedUpdate()
    {
        GondolaScript platform = GetPlatformBelow();
        if (platform != null)
        {

            // Carry the player along with the platform's motion this step.
            //Vector3 delta = platform.Velocity * Time.fixedDeltaTime;
            //_rb.MovePosition((Vector2)_rb.position + (Vector2)delta);
            print("SHSDFA " + platform.Velocity);
            //_currentMovement += platform.Velocity;
            platformVelocity = platform.Velocity;
            onMovingPlatform = true;
            currentPlatform = platform;
            currentPlatform.usedByCharacter = true;
        }
        else
        {
            if (currentPlatform != null)
                currentPlatform.usedByCharacter = false;
            currentPlatform = null;
            onMovingPlatform = false;
        }

        ExecuteState(StateExecutionType.FixedUpdate);

        _isGrounded = Grounded();
    }

    // Update is called once per frame
    void Update()
    {
        

        /*if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var parasiteBehaviour = abilityLibrary.GetAnyParasiteB(testAbility);
            if (parasiteBehaviour is Ability ability)
            {
                ability.TryTrigger();
            }
        }*/

        if (_moveAction != null) _move = _moveAction.ReadValue<Vector2>().x;

        ExecuteState(StateExecutionType.Update);

        debugState = _currentState;
    }

    bool KeyBlockedByState() => _currentState == PlayerState.Special
            || _currentState == PlayerState.Damaged
            || _currentState == PlayerState.Death;

    void OnJump(InputAction.CallbackContext context)
    {
        if (KeyBlockedByState()) return;

        bool coyoteCheck = _mayJump < _coyoteTime || _numberOfJumps != 0;

        if (_numberOfJumps <= _totalJumps && coyoteCheck)
        {
            SwitchState(PlayerState.Jump, true);
        }
        else if (_canGlide)
        {
            SwitchState(PlayerState.Glide);
        }

        //if (Grounded())
        //{

        //}
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        if (!KeyBlockedByState())
        {
            if (abilityLibrary && !string.IsNullOrWhiteSpace(wheelAbilityName))
            {
                var parasiteBehaviour = abilityLibrary.GetAnyParasiteB(wheelAbilityName);
                if (parasiteBehaviour is Ability ability)
                {
                    wheelAbilityName = null;
                    _wheelSpin.Spin();

                    _ability = ability;
                    SwitchState(PlayerState.Special);
                }
            }
        }
    }

    private GondolaScript GetPlatformBelow()
    {
        
        float castDistance = 5;
        
        Debug.DrawRay(transform.position, Vector2.down * castDistance, Color.blue);
        RaycastHit2D ledgeHit = Physics2D.Raycast(transform.position, Vector2.down * castDistance,
                            castDistance, _floorMask);
        if (ledgeHit)
        {

            if (ledgeHit.transform.CompareTag("Moving"))
                return ledgeHit.collider.GetComponentInParent<GondolaScript>();
        }
        return null;
    }
    private void OnJumpRelease(InputAction.CallbackContext context)
    {
        if (!_hasJumpForce) return;

        if (!_isGrounded && !_isInstantJump && _currentState == PlayerState.Jump)
            _rb.AddForceY(-_downwardForce, ForceMode2D.Impulse);
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (KeyBlockedByState()) return;

        _ability = _dashAbility;
        SwitchState(PlayerState.Special);
    }

    private void HandleMovement()
    {
        Vector2 movement = new Vector2(_move, 0f) * _speed;

        if (!_isGrounded)
        {

            _currentMovement = movement;
            _rb.linearVelocityX = 0;
            _rb.position += movement * Time.fixedDeltaTime;
        }
        else
        {

            //calculate the new goal velocity
            Vector3 unitVel = _currentMovement.normalized;
            //calculate dot velocity based on current movement
            float velDot = Vector3.Dot(new Vector2(_move, 0), unitVel);
            //and evaluate current acceleration
            //check what is the ideal movement and rotate to match the camera


            float accel = _acceleration * _accelerationFactorFromDot.Evaluate(velDot);
            //checks the ideal direciton and speed and moves it towards it
            _currentMovement = Vector3.MoveTowards(_currentMovement, movement, _acceleration * Time.fixedDeltaTime);

            //retrieve the acceleration needed to reach the desidered movement
            Vector2 neededAccel = (_currentMovement - _rb.linearVelocity) / Time.fixedDeltaTime;

            float maxAccel = _maxAcceleration; // * _maxAccelerationForceFactorFromDot.Evaluate(velDot);

            neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
            _rb.AddForce(Vector3.Scale(neededAccel * _rb.mass, new Vector2(1f, 0)));
        }

    }

    void FlipCharacter(float direction)
    {
        if (direction < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (direction > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    void Jump()
    {
        if (FMODAudioManager.Instance)
            FMODAudioManager.Instance.PlayOnce(_jumpAudio, null, true);

        _hasJumpForce = true;

        _rb.linearVelocityY = 0;

        _isInstantJump = !_isGrounded;

        _rb.AddForceY(_jumpForce, ForceMode2D.Impulse);

        _numberOfJumps++;
    }

    public void FreezeConstraints(RigidbodyConstraints2D freezeConstraints)
    {
        _rb.constraints = freezeConstraints;
    }

    void ExecuteState(StateExecutionType stateExecutionType)
    {
        switch (_currentState)
        {
            case PlayerState.Idle:
                IdleState(stateExecutionType);
                break;
            case PlayerState.Run:
                RunState(stateExecutionType);
                break;
            case PlayerState.Jump:
                JumpState(stateExecutionType);
                break;
            case PlayerState.Glide:
                GlideState(stateExecutionType);
                break;
            case PlayerState.Fall:
                FallState(stateExecutionType);
                break;
            case PlayerState.Land:
                LandState(stateExecutionType);
                break;
            case PlayerState.Special:
                SpecialState(stateExecutionType);
                break;
            case PlayerState.Damaged:
                DamagedState(stateExecutionType);
                break;
            case PlayerState.Death:
                DeathState(stateExecutionType);
                break;
            default:
                break;
        }
    }

    void SwitchState(PlayerState nextState, bool loop = false)
    {
        if (!loop && _currentState == nextState) return;

        ExecuteState(StateExecutionType.Exit);
        _currentState = nextState;
        ExecuteState(StateExecutionType.Enter);
    }

    void IdleState(StateExecutionType stateExecutionType)
    {
        switch (stateExecutionType)
        {
            case StateExecutionType.Enter:
                {
                    _numberOfJumps = 0;
                    _mayJump = 0;
                    _canGlide = true;

                    if (_animator) _animator.Play(idleAnim, 0, 0);
                    break;
                }
            case StateExecutionType.Update:
                {
                    if (_move != 0)
                    {
                        SwitchState(PlayerState.Run);
                        break;
                    }

                    if (!_isGrounded)
                    {
                        SwitchState(PlayerState.Fall);
                        break;
                    }
                    break;
                }
            case StateExecutionType.FixedUpdate:
                {
                    if (!onMovingPlatform)
                        _rb.linearVelocityX = 0;
                    else
                    {
                        print("SDFSD");
                        _rb.MovePosition(_rb.position + (platformVelocity * Time.fixedDeltaTime));
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void RunState(StateExecutionType stateExecutionType)
    {
        switch (stateExecutionType)
        {
            case StateExecutionType.Enter:
                {
                    _numberOfJumps = 0;
                    _mayJump = 0;
                    _canGlide = true;

                    FlipCharacter(_move);

                    if (Grounded())
                        _dustParticles.Play();

                    if (_animator) _animator.Play(runAnim, 0, 0);

                    HandleMovement();
                    FlipCharacter(_move);

                    break;
                }
            case StateExecutionType.FixedUpdate:
                {
                    if (_move == 0)
                    {
                        SwitchState(PlayerState.Idle);
                        break;
                    }

                    if (!_isGrounded)
                    {
                        SwitchState(PlayerState.Fall);
                        break;
                    }

                    HandleMovement();
                    FlipCharacter(_move);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void JumpState(StateExecutionType stateExecutionType)
    {
        switch (stateExecutionType)
        {
            case StateExecutionType.Enter:
                {
                    _hasJumpForce = false;
                    if (_animator) _animator.Play(jumpAnim, 0, 0);

                    //Jump();
                    break;
                }
            case StateExecutionType.FixedUpdate:
                {
                    if (HeadHit())
                    {
                        _rb.linearVelocityY = 0;
                        SwitchState(PlayerState.Fall);
                        break;
                    }

                    if (!_hasJumpForce)
                    {
                        _rb.linearVelocityY = 0;
                    }
                    else if (_hasJumpForce && _rb.linearVelocityY < 0)
                    {
                        SwitchState(PlayerState.Fall);
                        break;
                    }

                    HandleMovement();
                    FlipCharacter(_move);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void GlideState(StateExecutionType stateExecutionType)
    {
        switch (stateExecutionType)
        {
            case StateExecutionType.Enter:
                {
                    _canGlide = false;

                    _rb.linearVelocity = Vector2.zero;
                    _rb.gravityScale = _glideGravityScale;

                    if (_animator) _animator.Play(glideAnim, 0, 0);
                    if (_glideAudio && FMODAudioManager.Instance)
                        FMODAudioManager.Instance.PlayOnce(_glideAudio, null, true);
                    break;
                }
            case StateExecutionType.FixedUpdate:
                {
                    if (_isGrounded)
                    {
                        if (_move != 0) SwitchState(PlayerState.Run);
                        else SwitchState(PlayerState.Idle);
                        break;
                    }

                    HandleMovement();
                    FlipCharacter(_move);
                    break;
                }
            case StateExecutionType.Exit:
                {
                    _rb.gravityScale = _baseGravityScale;
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void FallState(StateExecutionType stateExecutionType)
    {
        switch (stateExecutionType)
        {
            case StateExecutionType.Enter:
                {
                    if (_animator) _animator.Play(fallAnim, 0, 0);
                    break;
                }
            case StateExecutionType.FixedUpdate:
                {
                    _mayJump += Time.fixedDeltaTime;

                    if (_isGrounded)
                    {
                        SwitchState(PlayerState.Land);
                        break;
                    }

                    HandleMovement();
                    FlipCharacter(_move);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void LandState(StateExecutionType stateExecutionType)
    {
        switch (stateExecutionType)
        {
            case StateExecutionType.Enter:
                {
                    if (_move != 0)
                    {
                        SwitchState(PlayerState.Run);
                        break;
                    }

                    if (FMODAudioManager.Instance)
                        FMODAudioManager.Instance.PlayOnce(_landAudio, null, true);

                    _numberOfJumps = 0;
                    _mayJump = 0;
                    _canGlide = true;

                    if (_animator) _animator.Play(landAnim, 0, 0);
                    break;
                }
            case StateExecutionType.FixedUpdate:
                {
                    if (_move != 0)
                    {
                        SwitchState(PlayerState.Run);
                        break;
                    }

                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void SpecialState(StateExecutionType stateExecutionType)
    {
        switch (stateExecutionType)
        {
            case StateExecutionType.Enter:
                {
                    //_rb.linearVelocity = Vector2.zero;
                    _ability.TryTrigger();
                    break;
                }
            case StateExecutionType.FixedUpdate:
                {
                    if (!_ability.IsExecuting())
                    {
                        if (_isGrounded) SwitchState(PlayerState.Idle);
                        else SwitchState(PlayerState.Fall);
                        break;
                    }

                    break;
                }
            case StateExecutionType.Exit:
                {
                    if (_ability.IsExecuting())
                    {
                        _ability.CancelExecution();
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void DamagedState(StateExecutionType stateExecutionType)
    {
        switch (stateExecutionType)
        {
            case StateExecutionType.Enter:
                {
                    if (_animator) _animator.Play(damagedAnim, 0, 0);
                    if (_damagedAudio && FMODAudioManager.Instance)
                        FMODAudioManager.Instance.PlayOnce(_damagedAudio, null, true);
                    if (_playerEffect) _playerEffect.PlayDamagedEffect();
                    break;
                }
            case StateExecutionType.Exit:
                {
                    _damageReceiver.BecomeInvisible();
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void DeathState(StateExecutionType stateExecutionType)
    {
        switch (stateExecutionType)
        {
            case StateExecutionType.Enter:
                {
                    FreezeConstraints(RigidbodyConstraints2D.FreezePositionX);

                    if (_animator) _animator.Play(deathAnim, 0, 0);
                    if (_deathAudio && FMODAudioManager.Instance)
                        FMODAudioManager.Instance.PlayOnce(_deathAudio, null, true);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    private bool HeadHit()
    {
        if (!_headCheck) return false;

        return Physics2D.OverlapCircle(_headCheck.position, _headCheckRadius, 0, headMask);
    }

    public bool Grounded()
    {
        if (!_groundCheck) return false;

        return Physics2D.OverlapBox(_groundCheck.position, _groundedSize, 0, _floorMask);
    }

    private void OnDrawGizmos()
    {
        if (_groundCheck)
        {
            if (_isGrounded) Gizmos.color = Color.green;
            else Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_groundCheck.position, _groundedSize);
        }

        if (_headCheck)
        {
            Gizmos.DrawWireSphere(_headCheck.position, _headCheckRadius);
        }
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        bool checkState = _currentState == PlayerState.Damaged ||
            _currentState == PlayerState.Death;

        if (_dashAbility.IsExecuting() || !_damageReceiver.CanTakeDamage
            || checkState) return;

        _damageReceiver.ReceiveDamage(damageInfo.damage);

        GameManager.Instance.CurrentLives = _damageReceiver.CurrentHealth;

        if (_damageReceiver.CurrentHealth > 0)
        {
            if (damageInfo.damageType == DamageType.Knockback)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(damageInfo.force, ForceMode2D.Impulse);
                FlipCharacter(-damageInfo.force.x);
            }

            SwitchState(PlayerState.Damaged);
        }
        else SwitchState(PlayerState.Death);
        // Knockback on the player would go here.
    }

    private void StartInvincibilityAnimation()
    {
        _animator.SetBool("Invincible", true);
    }

    private void StopInvincibilityAnimation()
    {
        _animator.SetBool("Invincible", false);
    }
}
