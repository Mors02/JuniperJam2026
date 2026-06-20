using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    
    private Rigidbody2D _rb;
    private Animator _animator;
    [SerializeField]
    private ParticleSystem _dustParticles;
    [Tooltip("What is considered ground for grounded checks")]
    [SerializeField]
    private LayerMask _floorMask;
    private ContactFilter2D _contactFilter;

    #region Input actions
    [SerializeField]
    private InputActionAsset _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;

    private InputAction _dashAction;

    #endregion

    [Header("Rider")]
    [Tooltip("The height of the character")]
    [SerializeField]
    float _rideHeight;
    [SerializeField]
    [Tooltip("The strength of the spring that keeps the character at the specified height")]
    float _rideSpringForceStrength;
    [SerializeField]
    [Tooltip("The damper of the spring strength")]
    float _rideSpringDamper;
    [Tooltip("Length of the ray to check the ride height")]
    [SerializeField]
    float _rayLength = 0.5f;

    [Header("Movement")]
    [Tooltip("Speed of the character")]
    [SerializeField]
    private float _speed;
    [Tooltip("Acceleration of the character to get to max speed")]
    [SerializeField]
    private float _acceleration;
    
    private Vector2 _currentMovement;

    [Tooltip("Max acceleration each frame to get to the desired speed")]
    [SerializeField]
    private float _maxAcceleration;
    [Tooltip("Curve for change of direction strength (Do not change unless you know what you're doing)")]
    [SerializeField]
    private AnimationCurve _accelerationFactorFromDot;
    
    float _move;

    [Header("Jump")]
    [Tooltip("The force of the jump")]
    [SerializeField]
    private float _jumpForce;
    [Tooltip("Custom gravity")]
    [SerializeField]
    private float _gravity;
    [Tooltip("How hard the force that brings down the player when stops pressing the jump button")]
    [SerializeField]
    private float _downwardForce;
    [Tooltip("How much time for the player to jump after leaving and hedge")]
    [SerializeField]
    private float _coyoteTime;
    private float _mayJump;
    [Tooltip("How much time for the player to buffer a jump before landing")]
    [SerializeField]
    private float _inputBufferTime;
    private float _jumpRequested;
    [Tooltip("How many jumps (also considering the first jump from the ground)")]
    [SerializeField]
    private float _totalJumps;
    private float _numberOfJumps;
    [Tooltip("Force multiplier for the second jump onward (to oppose the downward force)")]
    [SerializeField]
    private float _additionalJumpsMultiplier;

    [Header("Dash")]
    [Tooltip("The strength of the dash")]
    [SerializeField]
    private float _dashForce;
    [Tooltip("The cooldown of the dash")]
    [SerializeField]
    private float _dashCooldown;
    private float _dashCooldownTimer;
    [Tooltip("The duration of the dash (for how long the character is unaffected by gravity)")]
    [SerializeField]
    private float _dashDuration;
    private float _dashDurationTimer;
    [Tooltip("If true, the character reacts to the inputs of the player during the dash")]
    [SerializeField]
    private bool _canMoveDuringDash;


    [Header("Debug")]
    [SerializeField]
    bool _isWalking;
    [SerializeField]
    bool _isJumping;
    [SerializeField]
    bool _hasLanded;
    [SerializeField]
    bool _lookingRight;
    [SerializeField]
    bool _dashing;

    [SerializeField]
    bool _isStopped;
    Vector2 _velocityBeforeStopping;


    private bool _wasGrounded;
    private bool Grounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, _rideHeight + .05f, _floorMask);
        if (hit)
        {   
            return true;
        }            
        return false;   
    }



    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _moveAction = _playerInput.FindActionMap("Player").FindAction("Move");

        _jumpAction = _playerInput.FindActionMap("Player").FindAction("Jump");
        
        _dashAction = _playerInput.FindActionMap("Player").FindAction("Sprint");
        
        _numberOfJumps = 0;
        _jumpRequested = 0;
        _dashCooldownTimer = 0;
        _dashDurationTimer = 0;
        _dustParticles.Pause();
        _contactFilter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = true
        };
        _contactFilter.SetLayerMask(_floorMask);
    }

    private void HandleRideHeight()
    {
        RaycastHit2D[] hits = new RaycastHit2D[1]; 
        int howManyHits = Physics2D.Raycast(this.transform.position, -this.transform.up, _contactFilter, hits, _rayLength);

        if (howManyHits > 0)
        {
            Vector2 vel = _rb.linearVelocity;
            Vector2 rayDir = transform.TransformDirection(new Vector2(0, -1));

            Vector2 otherVel = Vector3.zero;
            Rigidbody2D hitBody = hits[0].rigidbody;

            if (hitBody != null)
            {
                otherVel = hitBody.linearVelocity;
            }

            float rayDirVel = Vector2.Dot(rayDir, vel);
            float otherDirVel = Vector2.Dot(rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;

            float x = hits[0].distance - _rideHeight;
            float springForce = (x * _rideSpringForceStrength) - (relVel * _rideSpringDamper);

            _rb.AddForce(rayDir * springForce);

            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(rayDir * -springForce, hits[0].point);
            }
        }
    }

    private void HandleMovement()
    {
        
        //add to the dash cooldown
        _dashCooldownTimer += Time.fixedDeltaTime;
        //if dashing add to the duration timer
        if (_dashing)
        {
            _dashDurationTimer += Time.fixedDeltaTime;
            //if we are reaching the dash duration
            if (_dashDurationTimer > _dashDuration)
            {
                //no more dashing
                _dashing = false;
            }
        }

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

    private void HandleVerticalMovement()
    {   
        bool grounded = Grounded();
        //if we reached the peak of the jump
        if (_isJumping && _rb.linearVelocityY < 0)
        {
            //start descending
            ApplyDownForce();
            //_isJumping = false;
        }

        _hasLanded = !_wasGrounded && grounded;

        _wasGrounded = grounded;
        if (_hasLanded && !_isJumping)
        {
            _animator.SetTrigger("Landed");
            _numberOfJumps = 0;
            _mayJump = 0;
        }

        if (grounded)
        {
            //if we inputted before the timer
            if (_jumpRequested < _inputBufferTime)
            {
                Jump();
                _jumpRequested = 10f;
            }
                
        } 
        else
        {
            if (!_dashing)
                _rb.linearVelocityY -= _gravity * Time.fixedDeltaTime;
            
            _jumpRequested += Time.fixedDeltaTime;
            _mayJump += Time.fixedDeltaTime;
        }
    }

    private void HandleAnimations()
    {
        _animator.SetBool("Walking", _isWalking);

        if (_lookingRight && _move < 0 || !_lookingRight && _move > 0)
        {
            FlipCharacter();
            if (Grounded())
                _dustParticles.Play();
        }
            
            
    }

    void FlipCharacter()
    {
        if (_lookingRight)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            _lookingRight = false;
        } else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            _lookingRight = true;
        }

    }

    void OnMove(InputAction.CallbackContext context)
    {
        if (_dashing)
        {
            _move = 0f;
           if (!_canMoveDuringDash) return;
        }

        _move = context.ReadValue<Vector2>().x;
        if (_move != 0)
            _isWalking = true;
    }

    void StopMovement(InputAction.CallbackContext context)
    {
        _move = 0;
        _isWalking = false;
    }

    void OnJump(InputAction.CallbackContext context)
    {
        if (Grounded() || (_mayJump < _coyoteTime && !_isJumping) || _numberOfJumps < _totalJumps)
        {
           Jump();
        } else
        {
            _jumpRequested = 0;
        }
    }

    void OnDash(InputAction.CallbackContext context)
    {
       
        if (_dashCooldownTimer > _dashCooldown)
        {   
            _animator.SetTrigger("Dashed");
            //add force in the direction we're going
            _rb.AddForceX(_dashForce * _move, ForceMode2D.Impulse);
            //reset vertical velocity
            _rb.linearVelocityY = 0;
            _dashing = true;
            _dashCooldownTimer = 0;
            _dashDurationTimer = 0;
        }
            
    }

    void Jump()
    {
        if(_numberOfJumps > 0)
            _rb.linearVelocityY = 0;
        _animator.SetTrigger("Jumped");
        _rb.AddForceY(_jumpForce * (_numberOfJumps > 0? _additionalJumpsMultiplier : 1), ForceMode2D.Impulse);
        _isJumping = true;
        Debug.Log("Jumps: " + _numberOfJumps);
        _numberOfJumps++;
    }

    void OnJumpRelease(InputAction.CallbackContext context)
    {
        ApplyDownForce();
    }

    private void ApplyDownForce()
    {
        _rb.AddForceY(-_downwardForce, ForceMode2D.Impulse);
        _isJumping = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        this._rb.linearVelocityX = 0;
    }

    private void FixedUpdate()
    {
        HandleAnimations();

        if (_isStopped)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }
            

        if (!_isJumping)
            HandleRideHeight();
        HandleMovement();
        HandleVerticalMovement();
    }

    private void OnEnable()
    {
        _moveAction.performed += OnMove;
        _moveAction.canceled += StopMovement;

        _jumpAction.performed += OnJump;
        _jumpAction.canceled += OnJumpRelease;

        _dashAction.performed += OnDash;

        _moveAction?.Enable();
        _jumpAction?.Enable();
        _dashAction?.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _dashAction.Disable();
    }

    private void OnDestroy()
    {
        _moveAction.performed -= OnMove;
        _moveAction.canceled -= StopMovement;

        _jumpAction.performed -= OnJump;
        _jumpAction.canceled -= OnJumpRelease;

        _dashAction.performed -= OnDash;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.up * _rideHeight);

        Gizmos.DrawSphere(transform.position - (transform.up * _rideHeight), 0.05f);
    }


    public void StopCharacter(float time = 0f)
    {
        _velocityBeforeStopping = _rb.linearVelocity;
        _isStopped = true;
        _rb.linearVelocity = Vector2.zero;

        if (time > 0f)
            Invoke("StartCharacter", time);
    }

    public void StartCharacter()
    {
        _rb.linearVelocity = _velocityBeforeStopping;
        _isStopped = false;
    }
}
