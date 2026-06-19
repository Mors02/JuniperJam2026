using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    
    private Rigidbody2D _rb;
    [SerializeField]
    private LayerMask _floorMask;

    #region Input actions
    [SerializeField]
    private InputActionAsset _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    #endregion

    [Header("Rider")]
    [SerializeField]
    float _rideHeight;
    [SerializeField]
    float _rideSpringForceStrength, _rideSpringDamper;
    [SerializeField]
    float _rayLength = 0.5f;

    [Header("Movement")]
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _acceleration;
    
    private Vector2 _currentMovement;

    [SerializeField]
    private float _maxAcceleration;

    [SerializeField]
    private AnimationCurve _accelerationFactorFromDot;
    
    float _move;

    [Header("Jump")]
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private float _downwardForce;
    [SerializeField]
    private float _coyoteTime;

    [Header("Debug")]
    [SerializeField]
    bool _isWalking;
    bool _isJumping;



    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _moveAction = _playerInput.FindActionMap("Player").FindAction("Move");
        _moveAction.performed += OnMove;
        _moveAction.canceled += StopMovement;
        _jumpAction = _playerInput.FindActionMap("Player").FindAction("Jump");
        _jumpAction.performed += OnJump;
        _jumpAction.canceled += OnJumpRelease;
    }

    private void HandleRideHeight()
    {
        RaycastHit2D rayHit = Physics2D.Raycast(this.transform.position, -this.transform.up, _rayLength, _floorMask);

        if (rayHit)
        {
            Vector2 vel = _rb.linearVelocity;
            Vector2 rayDir = transform.TransformDirection(new Vector2(0, -1));

            Vector2 otherVel = Vector3.zero;
            Rigidbody2D hitBody = rayHit.rigidbody;

            if (hitBody != null)
            {
                otherVel = hitBody.linearVelocity;
            }

            float rayDirVel = Vector2.Dot(rayDir, vel);
            float otherDirVel = Vector2.Dot(rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;

            float x = rayHit.distance - _rideHeight;
            float springForce = (x * _rideSpringForceStrength) - (relVel * _rideSpringDamper);

            _rb.AddForce(rayDir * springForce);

            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(rayDir * -springForce, rayHit.point);
            }
        }
    }

    private void HandleMovement()
    {
        
//        Debug.Log("Movement " + _rb.linearVelocity);
       // _rb.AddForce(movement * _speed * Time.fixedDeltaTime, ForceMode2D.Force);
       // Debug.Log(_rb.linearVelocity);

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
        //if we reached the peak of the jump
        if (_isJumping && _rb.linearVelocityY < 0)
        {
            //start descending
            ApplyDownForce();
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>().x;
        _isWalking = true;
    }

    void StopMovement(InputAction.CallbackContext context)
    {
        _move = 0;
        _isWalking = false;
    }

    void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jumped");
        _rb.AddForceY(_jumpForce, ForceMode2D.Impulse);
        _isJumping = true;

    }

    void OnJumpRelease(InputAction.CallbackContext context)
    {
        ApplyDownForce();
    }

    private void ApplyDownForce()
    {
        Debug.Log("Released");
        _rb.AddForceY(-_downwardForce, ForceMode2D.Impulse);
        _isJumping = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        this._rb.linearVelocityX = 0;
    }

    private void FixedUpdate()
    {
        if (!_isJumping)
            HandleRideHeight();
        HandleMovement();
        HandleVerticalMovement();
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _jumpAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.up * _rayLength);
    }


}
