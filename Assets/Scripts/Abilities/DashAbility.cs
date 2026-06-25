using AbyssWorks.AnimatorSignal;
using AbyssWorks.FMODAudioManager;
using System.Collections;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[System.Serializable]
public class DashAbility : Ability
{
    [Header("Damage settings")]
    [SerializeField] private Hitbox _hitbox;
    [SerializeField] private int _damage = 1;

    [Header("Dash settings")]
    [SerializeField] private FMODAudioScriptable _dashAudio;
    [SerializeField] private string _dashAnim;
    [SerializeField] private float _dashForce = 10f;
    [SerializeField] private float _dashCooldown = 2f;
    [SerializeField] private bool _allowFirst = true;

    private MonoBehaviour _monoBehaviour;
    private Animator _animator;
    private AnimationSubscriber _animationSubscriber;
    private Rigidbody2D _rb;
    private PlayerSMController _playerSMController;

    private Coroutine _dashCoroutine;

    private float _curDashTime = 0;

    private bool _hasAnimEnded = false;
    public override void Initialize(GameObject go = null)
    {
        base.Initialize(go);

        _monoBehaviour = gameObject.GetComponent<MonoBehaviour>();
        _animator = gameObject.GetComponent<Animator>();
        _animationSubscriber = gameObject.GetComponent<AnimationSubscriber>();
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _playerSMController = gameObject.GetComponent<PlayerSMController>();

        if (_hitbox)
        {
            _hitbox.gameObject.SetActive(false);
        }

        if (_animationSubscriber)
        {
            _animationSubscriber.SubscribeEndAction(() => {
                if (!IsExecuting()) return;

                _hasAnimEnded = true;
            });
            _animationSubscriber.SubscribeAction("Dash", () =>
            {
                if (!IsExecuting()) return;

                _rb.AddForce(_dashForce * gameObject.transform.right, ForceMode2D.Impulse);
            });
        }
    }

    protected override void OnDeepCopy()
    {
        base.OnDeepCopy();

        _monoBehaviour = null;
        _animator = null;
        _animationSubscriber = null;
        _rb = null;
        _playerSMController = null;
        _dashAudio = null;

        _hitbox = null;
    }

    bool CanDash()
    {
        return _allowFirst || Time.time - _curDashTime > _dashCooldown;
    }

    public override bool IsExecuting()
    {
        return _dashCoroutine != null;
    }

    public override bool CanTrigger()
    {
        return base.CanTrigger() && _dashCoroutine == null && _hitbox
            && CanDash() && _monoBehaviour && _animator && _animationSubscriber;
    }

    public override void Trigger()
    {
        base.Trigger();

        _hitbox.gameObject.SetActive(true);
        _hitbox.onEnter2D += HitboxEnter2D;

        if (FMODAudioManager.Instance)
            FMODAudioManager.Instance.PlayOnce(_dashAudio, null, true);

        _allowFirst = false;

        _dashCoroutine ??= _monoBehaviour.StartCoroutine(DashEnumerator());
    }

    public override void CancelExecution(bool forceCancel = false)
    {
        base.CancelExecution(forceCancel);

        if (_dashCoroutine != null)
        {
            _monoBehaviour.StopCoroutine(_dashCoroutine);

            _playerSMController.FreezeConstraints(_playerSMController.BaseConstraints);

            _curDashTime = Time.time;

            _hitbox.onEnter2D -= HitboxEnter2D;
            _hitbox.gameObject.SetActive(false);


            onExecutionCancel?.Invoke();

            _dashCoroutine = null;
        }
    }

    void HitboxEnter2D(Collider2D collision)
    {
        if (collision.gameObject == gameObject) return;

        //to do
        //Stop enemies by stasis amount
        //effects

        if (collision.TryGetComponent<ITakeDamage>(out var iTakeDamage))
        {
            iTakeDamage.TakeDamage(new DamageInfo(_damage));
        }
    }

    IEnumerator DashEnumerator()
    {
        if (!_animator) yield break;

        _animator.Play(_dashAnim, 0, 0);
        _hasAnimEnded = false;

        _rb.linearVelocityX = 0;
        _playerSMController.FreezeConstraints(RigidbodyConstraints2D.FreezePositionY);
        while (!_hasAnimEnded) yield return null;

        _playerSMController.FreezeConstraints(_playerSMController.BaseConstraints);

        _curDashTime = Time.time;

        _hitbox.onEnter2D -= HitboxEnter2D;
        _hitbox.gameObject.SetActive(false);

        onExecutionComplete?.Invoke();
        _dashCoroutine = null;
    }
}
