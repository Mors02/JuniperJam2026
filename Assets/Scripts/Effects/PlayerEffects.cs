using AbyssWorks.AnimatorSignal;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

using ImpulseShape = Unity.Cinemachine.CinemachineImpulseDefinition.ImpulseShapes;

public class PlayerEffects : MonoBehaviour
{
    [SerializeField] private AnimationSubscriber _animationSubscriber;

    [Header("Damaged effect")]
    public float damagedTimeStopDuration = 0.1f;
    public float damagedTimeResumeDuration = 0.1f;
    public float damagedImpulseForce = 0.2f;
    public ImpulseShape damagedImpulseShape = ImpulseShape.Bump;
    public float damagedImpulseDuration = 0.2f;
    public Vector3 damagedImpulseVelocity = Vector3.up;

    [Header("Windpush effect")]
    public float windTimeStopDuration = 0.05f;
    public float windImpulseForce = 0.25f;
    public ImpulseShape windImpulseShape = ImpulseShape.Explosion;
    public float windImpulseDuration = 0.1f;

    private CinemachineImpulseSource _impulseSource;
    
    private Coroutine _timeStopRoutine;

    private const string TIMESTOPNAME = "OnPlayerStopTime";

    private void Awake()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();

        _animationSubscriber.SubscribeAction("WindLaunchEffect", PlayWindEffect);
    }

    void PlayWindEffect()
    {
        _impulseSource.ImpulseDefinition.ImpulseShape = windImpulseShape;
        _impulseSource.ImpulseDefinition.ImpulseDuration = windImpulseDuration;
        _impulseSource.DefaultVelocity = System.MathF.Sign(transform.position.x) * Vector3.right;
        _impulseSource.GenerateImpulseWithForce(windImpulseForce);

        MakeTimeStop(windTimeStopDuration, 0);
    }

    public void PlayDamagedEffect()
    {
        _impulseSource.ImpulseDefinition.ImpulseShape = damagedImpulseShape;
        _impulseSource.ImpulseDefinition.ImpulseDuration = damagedImpulseDuration;
        _impulseSource.DefaultVelocity = damagedImpulseVelocity;
        _impulseSource.GenerateImpulseWithForce(damagedImpulseForce);

        MakeTimeStop(damagedTimeStopDuration, damagedTimeResumeDuration);
    }

    void MakeTimeStop(float stopTime, float resumeTime)
    {
        _timeStopRoutine ??= StartCoroutine(TimeStopEnumerator(stopTime, resumeTime));
    }

    IEnumerator TimeStopEnumerator(float stopTime, float resumeTime)
    {
        float elapsed = 0;

        if (PauseManager.instance)
            PauseManager.instance.SetPause(TIMESTOPNAME, 0);

        while (elapsed < stopTime)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0;

        while (elapsed < resumeTime)
        {
            float perc = elapsed / resumeTime;

            if (PauseManager.instance)
                PauseManager.instance.SetPause(TIMESTOPNAME, perc);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (PauseManager.instance)
            PauseManager.instance.SetPause(TIMESTOPNAME, 1);

        _timeStopRoutine = null;
    }
}
