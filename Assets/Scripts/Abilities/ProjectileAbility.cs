using AbyssWorks.AnimatorSignal;
using AbyssWorks.FMODAudioManager;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class ProjectileAbility : Ability
{
    public Transform cloudSpot;
    public Transform launchSpot;
    public string launchAnim;
    public FMODAudioScriptable projectileAudio;
    public GameObject projectilePrefab;
    
    private Transform transform;
    private MonoBehaviour _monoBehaviour;
    private Animator _animator;
    private AnimationSubscriber _animationSubscriber;
    private PlayerSMController _playerSMController;

    private bool _hasAnimEnded = false;
    private Coroutine _waitRoutine;

    private Cloud _cloud;

    public override void Initialize(GameObject go = null)
    {
        base.Initialize(go);

        if (gameObject)
        {
            transform = gameObject.transform;
            _animator = gameObject.GetComponent<Animator>();
            _animationSubscriber = gameObject.GetComponent<AnimationSubscriber>();
            _monoBehaviour = gameObject.GetComponent<MonoBehaviour>();
            _playerSMController = gameObject.GetComponent<PlayerSMController>();

            if (_animationSubscriber)
            {
                _animationSubscriber.SubscribeEndAction(() => {
                    if (!IsExecuting()) return;

                    _hasAnimEnded = true;
                });
                _animationSubscriber.SubscribeAction("Launch", SpawnProjectile);
            }

            if (!launchSpot) launchSpot = transform;

            var cloudObject = GameObject.FindWithTag("Cloud");

            if (cloudObject)
            {
                _cloud = cloudObject.GetComponent<Cloud>();
            }
        }

    }

    protected override void OnDeepCopy()
    {
        base.OnDeepCopy();

        launchSpot = null;
        projectileAudio = null;
        projectilePrefab = null;
        transform = null;
        _monoBehaviour = null;
        _animator = null;
        _animationSubscriber = null;
        _playerSMController = null;
        _cloud = null;
    }

    public override bool IsExecuting()
    {
        return _waitRoutine != null;
    }

    public override bool CanTrigger()
    {
        return base.CanTrigger() && _waitRoutine == null && projectilePrefab && 
            _animator && _animationSubscriber && _playerSMController;
    }

    void SpawnProjectile()
    {
        if (!IsExecuting()) return;

        //Debug.Log(projectilePrefab);

        var projectileObject = UnityEngine.Object.Instantiate(projectilePrefab, launchSpot.position, launchSpot.rotation);

        if (projectileObject.TryGetComponent<Projectile>(out var projectile))
        {
            projectile.SetOwner(gameObject);
        }
    }

    public override void Trigger()
    {
        base.Trigger();

        _waitRoutine ??= _monoBehaviour.StartCoroutine(WaitEnumerator());
    }

    public override void CancelExecution(bool forceCancel = false)
    {
        base.CancelExecution(forceCancel);

        if (_waitRoutine != null)
        {
            _monoBehaviour.StopCoroutine(_waitRoutine);

            _playerSMController.FreezeConstraints(_playerSMController.BaseConstraints);

            if (_cloud && cloudSpot) _cloud.Disappear();

            onExecutionCancel?.Invoke();

            _waitRoutine = null;
        }
    }

    IEnumerator WaitEnumerator()
    {
        if (!_animator) yield break;

        if (FMODAudioManager.Instance && projectileAudio)
            FMODAudioManager.Instance.PlayOnce(projectileAudio, null, true);

        _animator.Play(launchAnim, 0, 0);
        _hasAnimEnded = false;

        if (_cloud && cloudSpot && !_playerSMController.Grounded()) 
            _cloud.Appear(cloudSpot.position, cloudSpot.rotation);

        _playerSMController.FreezeConstraints(RigidbodyConstraints2D.FreezePosition);

        while (!_hasAnimEnded) yield return null;

        _playerSMController.FreezeConstraints(_playerSMController.BaseConstraints);

        if (_cloud && cloudSpot) _cloud.Disappear();

        onExecutionComplete?.Invoke();

        _waitRoutine = null;
    }
}
