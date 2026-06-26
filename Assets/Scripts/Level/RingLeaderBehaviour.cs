using AbyssWorks.FMODAudioManager;
using System.Collections.Generic;
using UnityEngine;

public class RingLeaderBehaviour : MonoBehaviour
{
    [SerializeField]
    private Transform _transform;

    [SerializeField]
    private Collider2D _damageCollider;

    private Transform _player;

    [SerializeField]
    private float _movementYSpeed;
    [SerializeField]
    private float _movementXSpeed;

    [SerializeField]
    private float _attackCooldown;

    private float _attackTimer;

    [SerializeField]
    private float _X;

    private bool _active;
    private bool _attacking;

    [SerializeField]
    private int _damage;

    [SerializeField]
    private Animator _animator;

    [Header("Audio")]
    [SerializeField] private FMODAudioScriptable _attackAudio;
    [SerializeField] private FMODAudioScriptable _laughAudio;
    [SerializeField] private FMODAudioScriptable _shortLaughAudio;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _active = false;
        _damageCollider.enabled = false;
        _player = GameObject.FindGameObjectWithTag("Player").transform;        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_active)
        {
            _attackTimer += Time.deltaTime;
           
            if (!_attacking)
            {
                float targetY = Mathf.Lerp(transform.position.y, _player.position.y, _movementYSpeed * Time.fixedDeltaTime);
                float targetX = Mathf.Lerp(transform.position.x, Camera.main.transform.position.x + _X, _movementXSpeed  * Time.fixedDeltaTime);
                transform.position = new Vector2(targetX, targetY);    
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

    public void PlayAttackSFX()
    {
        if (_shortLaughAudio && FMODAudioManager.Instance)
            FMODAudioManager.Instance.PlayOnce(_shortLaughAudio);

        if (_attackAudio && FMODAudioManager.Instance)
            FMODAudioManager.Instance.PlayOnce(_attackAudio);
    }

    public void PlayLaughSFX()
    {
        if (_laughAudio && FMODAudioManager.Instance)
            FMODAudioManager.Instance.PlayOnce(_laughAudio);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<ITakeDamage>(out ITakeDamage damageReceiver))
        {
            if (collision.CompareTag("Player"))
            {
                DamageInfo info = new DamageInfo(_damage, DamageType.None);
                damageReceiver.TakeDamage(info);
            }
        }
    }

    public void BlockMovement()
    {
        _attacking = true;
    }
}
