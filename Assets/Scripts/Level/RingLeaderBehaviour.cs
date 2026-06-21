using System.Threading;
using UnityEngine;

public class RingLeaderBehaviour : MonoBehaviour
{
    [SerializeField]
    private Transform _transform;

    [SerializeField]
    private Collider2D _damageCollider;

    private Transform _player;

    [SerializeField]
    private float _movementSpeed;

    [SerializeField]
    private float _attackCooldown;

    private float _attackTimer;

    [SerializeField]
    private float _X;

    private bool _active;
    private bool _attacking;

    [SerializeField]
    private Animator _animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _active = false;
        _damageCollider.enabled = false;
        _player = GameObject.FindGameObjectWithTag("Player").transform;        
    }

    // Update is called once per frame
    void Update()
    {
        if (_active)
        {
            _attackTimer += Time.deltaTime;
           
            if (!_attacking)
            {
                float targetY = Mathf.Lerp(transform.position.y, _player.position.y, _movementSpeed * Time.deltaTime);
                transform.position = new Vector2(transform.parent.position.x + _X, targetY);    
            }
            

            if (_attackTimer >= _attackCooldown)
            {
                _attackTimer = 0f;
                _animator.SetTrigger("Attack");
            }
        }
        
        


        
    }

    public void Activate()
    {
        _active = true;
        _animator.SetTrigger("Activate");
        _attackTimer = 0f;
    }

    public void ActivateDamageArea()
    {
        _damageCollider.enabled = true;
    }

    public void DeleteDamageArea()
    {
        _attacking = false;
        _damageCollider.enabled = false;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out DamageReceiver receiver))
        {
            receiver.ReceiveDamage();
        }
    }

    public void BlockMovement()
    {
        _attacking = true;
    }
}
