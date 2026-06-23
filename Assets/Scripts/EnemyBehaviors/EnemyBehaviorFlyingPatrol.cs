using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


/// <summary>
/// This class defines a flying enemy that patrols between two points in the air.
/// </summary>
public class EnemyBehaviorFlyingPatrol : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D _rb;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private Animator _animator;
    [SerializeField] private Transform _patrolMarkerA;
    [SerializeField] private Transform _patrolMarkerB;
    private Vector3 _patrolMarkerAPosition;
    private Vector3 _patrolMarkerBPosition;

    [Tooltip("What is considered ground for grounded checks")]
    [SerializeField]
    private LayerMask _floorMask;
    private ContactFilter2D _contactFilter;

    [Header("Movement")]
    [Tooltip("The speed at which the enemy moves")]
    [SerializeField]
    private float _moveSpeed = 1f;
    [SerializeField]
    private float _turnDelay = 0.5f;
    private float _turnTimer = 0f;
    private MovementState _movementState = MovementState.Moving;
    private enum MovementState {
        Moving,
        Turning,
        Stopped
    };
    private bool _headingToA = true;

    void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _contactFilter.SetLayerMask(_floorMask);
        _contactFilter.useTriggers = false;
        _patrolMarkerAPosition = _patrolMarkerA.position;
        _patrolMarkerBPosition = _patrolMarkerB.position;
        _patrolMarkerA.gameObject.SetActive(false);
        _patrolMarkerB.gameObject.SetActive(false);
        this.transform.position = new Vector2((_patrolMarkerAPosition.x + _patrolMarkerBPosition.x) / 2f, ( _patrolMarkerAPosition.y + _patrolMarkerBPosition.y) / 2f); // Start at marker A
    }

    private void FixedUpdate() {
        print("Current state: " + _movementState);
        switch (_movementState) {
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
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f) {
            StartTurning();
        }
        else {
            _rb.linearVelocity = direction * _moveSpeed;
        }        
    }

    private void StartTurning() {
        _movementState = MovementState.Turning;
        _turnTimer = 0f;
        _rb.linearVelocity = Vector2.zero;
        _headingToA = !_headingToA; // Switch the target marker
    }
    
    private void HandleTurning() {
        _turnTimer += Time.fixedDeltaTime;
        if (_turnTimer >= _turnDelay) {
            FinishTurning();
        }
    }

    private void FinishTurning() {
        // Flip facing by inverting localScale.x
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
        _movementState = MovementState.Moving;
    }

    private void StartStopping() {
        _movementState = MovementState.Stopped;
        _rb.linearVelocity = Vector2.zero;
    }

    private void HandleStopping() {
        _rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// lol
    /// </summary>
    private void StopStopping() {
        _movementState = MovementState.Moving;
    }
}
