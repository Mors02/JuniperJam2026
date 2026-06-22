using AbyssWorks.AnimatorSignal;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class Int_Dash : Ability
{
    [SerializeField] private string _dashAnim;
    [SerializeField] private float _dashForce = 10f;
    [SerializeField] private float _dashCooldown = 2f;
    [SerializeField] private bool _allowFirst = true;

    private MonoBehaviour _monoBehaviour;
    private Animator _animator;
    private AnimationSubscriber _animationSubscriber;
    private Rigidbody2D _rb;
    
    private Coroutine _dashCoroutine;

    private float _curDashTime = 0;

    private bool _hasDashEnded = false;
    public override void Initialize(GameObject go = null)
    {
        base.Initialize(go);

        _monoBehaviour = gameObject.GetComponent<MonoBehaviour>();
        _animator = gameObject.GetComponent<Animator>();
        _animationSubscriber = gameObject.GetComponent<AnimationSubscriber>();
        _rb = gameObject.GetComponent<Rigidbody2D>();

        if (_animationSubscriber)
        {
            _animationSubscriber.SubscribeAction("DashEnd", () => {
                _hasDashEnded = true;
            });
        }
    }

    protected override void OnDeepCopy()
    {
        base.OnDeepCopy();

        _monoBehaviour = null;
        _animator = null;
        _animationSubscriber = null;
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
        return base.CanTrigger() && CanDash() && _monoBehaviour && _animator && _animationSubscriber;
    }

    public override void Trigger()
    {
        base.Trigger();

        _allowFirst = false;

        _dashCoroutine ??= _monoBehaviour.StartCoroutine(DashEnumerator());
    }

    public override void CancelExecution(bool forceCancel = false)
    {
        base.CancelExecution(forceCancel);

        if (_dashCoroutine != null)
        {
            _monoBehaviour.StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;

            onExecutionCancel?.Invoke();
        }
    }

    IEnumerator DashEnumerator()
    {
        _hasDashEnded = false;

        if (_animator)
        {
            _animator.Play(_dashAnim);

            _rb.linearVelocityX = 0;

            _rb.AddForce(_dashForce * gameObject.transform.right, ForceMode2D.Impulse);
            while (!_hasDashEnded)
            {
                _rb.linearVelocityY = 0;
                yield return null;
            }
        }

        _curDashTime = Time.time;

        _dashCoroutine = null;
    }
}
