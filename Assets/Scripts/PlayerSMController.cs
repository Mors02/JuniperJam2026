using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSMController : MonoBehaviour
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
        Fall,
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

    [Header("Animation")]
    [SerializeField] private string idleAnim;
    [SerializeField] private string runAnim;
    [SerializeField] private string jumpAnim;
    [SerializeField] private string fallAnim;

    [Header("Head Check")]
    [SerializeField] private Transform _headCheck;
    [SerializeField] private float _headCheckRadius = 0.1f;
    [SerializeField] private LayerMask headMask;

    [Header("Floor")]
    [SerializeField] public Transform _groundCheck;
    [SerializeField] private Vector2 _groundedSize = Vector2.one;
    [SerializeField] private LayerMask _floorMask;

    [Header("Misc")]
    [SerializeField] private InputActionAsset _playerInput;

    [Header("Debug")]
    public PlayerState debugState;

    private PlayerState _currentState = PlayerState.Idle;
    private Rigidbody2D _rb;
    private Animator _animator;

    private Vector2 _currentMovement;
    private float _move;
    private bool _isGrounded;

    #region Input actions
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    #endregion

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();

        _moveAction = _playerInput.FindActionMap("Player").FindAction("Move");
        _jumpAction = _playerInput.FindActionMap("Player").FindAction("Jump");
        _jumpAction.performed += OnJump;
        _jumpAction.canceled += OnJumpRelease;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ExecuteState(StateExecutionType.Enter);
    }

    private void FixedUpdate()
    {
        ExecuteState(StateExecutionType.FixedUpdate);

        _isGrounded = Grounded();
    }

    // Update is called once per frame
    void Update()
    {
        if (_moveAction != null) _move = _moveAction.ReadValue<Vector2>().x;

        ExecuteState(StateExecutionType.Update);

        debugState = _currentState;
    }

    void OnJump(InputAction.CallbackContext context)
    {
        //if (Grounded())
        //{
        if (_isGrounded) Jump();
        //}
    }

    private void OnJumpRelease(InputAction.CallbackContext context)
    {
        if (!_isGrounded) _rb.AddForceY(-_downwardForce, ForceMode2D.Impulse);
    }

    private void HandleMovement()
    {
        //calculate the new goal velocity
        Vector3 unitVel = _currentMovement.normalized;
        //calculate dot velocity based on current movement
        float velDot = Vector3.Dot(new Vector2(_move, 0), unitVel);
        //and evaluate current acceleration
        //check what is the ideal movement and rotate to match the camera
        Vector2 movement = new Vector2(_move, 0f) * _speed;

        float accel = _acceleration * _accelerationFactorFromDot.Evaluate(velDot);
        //checks the ideal direciton and speed and moves it towards it
        _currentMovement = Vector3.MoveTowards(_currentMovement, movement, _acceleration * Time.fixedDeltaTime);

        //retrieve the acceleration needed to reach the desidered movement
        Vector2 neededAccel = (_currentMovement - _rb.linearVelocity) / Time.fixedDeltaTime;

        float maxAccel = _maxAcceleration; // * _maxAccelerationForceFactorFromDot.Evaluate(velDot);

        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
        _rb.AddForce(Vector3.Scale(neededAccel * _rb.mass, new Vector2(1f, 0)));
    }

    void FlipCharacter()
    {
        if (_move <= 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    void Jump()
    {
        _rb.AddForceY(_jumpForce, ForceMode2D.Impulse);
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
            case PlayerState.Fall:
                FallState(stateExecutionType);
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
                    if (_animator) _animator.Play(idleAnim);
                    break;
                }
            case StateExecutionType.Update:
                {
                    if (_move != 0)
                    {
                        SwitchState(PlayerState.Run);
                        break;
                    }

                    HandleMovement();
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
                    FlipCharacter();

                    if (Grounded())
                        _dustParticles.Play();

                    if (_animator) _animator.Play(runAnim);
                    break;
                }
            case StateExecutionType.Update:
                {
                    if (_move == 0)
                    {
                        SwitchState(PlayerState.Idle);
                        break;
                    }

                    HandleMovement();
                    FlipCharacter();
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
    }

    void FallState(StateExecutionType stateExecutionType)
    {
    }

    void SpecialState(StateExecutionType stateExecutionType)
    {
    }

    void DamagedState(StateExecutionType stateExecutionType)
    {
    }

    void DeathState(StateExecutionType stateExecutionType)
    {

    }

    private bool Grounded()
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
    }
}
