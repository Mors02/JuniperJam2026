using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyBehaviorPatrol : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D _rb;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;


    [Tooltip("What is considered ground for grounded checks")]
    [SerializeField]
    private LayerMask _floorMask;
    private ContactFilter2D _contactFilter;
    private Transform _transform;

    [Header("Movement")]
    [Tooltip("The speed at which the enemy moves")]
    [SerializeField]
    private float _moveSpeed = 1f;
    private bool _facingRight = true;
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
    private enum MovementState {
        Moving,
        Turning,
        Stopped
    };

    void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _contactFilter.SetLayerMask(_floorMask);
        _contactFilter.useTriggers = false;
        _transform = transform;
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

    private void HandleMovement() {
        int direction = _facingRight ? 1 : -1;
        // Check for ledge
        Vector2 ledgeCheckOrigin = (Vector2)_transform.position + Vector2.right * direction * _ledgeCheckDistance;
        Debug.DrawRay(ledgeCheckOrigin, Vector2.down * _ledgeCheckRange, Color.red);
        RaycastHit2D ledgeHit = Physics2D.Raycast(ledgeCheckOrigin, Vector2.down, _ledgeCheckRange, _floorMask);
        print("Ledge check: " + (ledgeHit ? "Ground detected" : "No ground detected"));
        if (ledgeHit)
        {
            Debug.Log("Ledge check hit: " + ledgeHit.collider.name);
        }
        if (!ledgeHit) {
            StartTurning();
            return;
        }

        // Check for wall
        Vector2 wallCheckOrigin = (Vector2)_transform.position + Vector2.right * 0.5f;
        Debug.DrawRay(wallCheckOrigin, Vector2.right * direction * _wallCheckDistance, Color.blue);
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheckOrigin, Vector2.right * transform.localScale.x, _wallCheckDistance, _floorMask);
        if (wallHit) {
            StartTurning();
            return;
        }

        
        // Move in the current direction
        _rb.linearVelocity = new Vector2(transform.localScale.x * _moveSpeed * direction, _rb.linearVelocity.y);
        print("Moving " + (direction == 1 ? "right" : "left"));
    }

    private void StartTurning() {
        _movementState = MovementState.Turning;
        _turnTimer = 0f;
        _rb.linearVelocity = Vector2.zero;
    }
    
    private void HandleTurning() {
        _turnTimer += Time.fixedDeltaTime;
        if (_turnTimer >= _turnDelay) {
            FinishTurning();
        }
    }

    private void FinishTurning() {
        _facingRight = !_facingRight;
        _movementState = MovementState.Moving;
        _spriteRenderer.flipX = !_spriteRenderer.flipX;
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
