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
    [SerializeField] private CinemachineImpulseSource _damagedImpulseSource;

    [Header("Windpush effect")]
    public float windTimeStopDuration = 0.05f;
    public float windImpulseForce = 0.25f;
    [SerializeField] private CinemachineImpulseSource _windImpulseSource;

    [Header("Sword effect")]
    public float swordTimeStopDuration = 0.125f;
    public float swordTimeResumeDuration = 0.05f;
    public float swordImpulseForce = 0.25f;
    [SerializeField] private CinemachineImpulseSource _swordImpulseSource;

    private Coroutine _timeStopRoutine;

    private const string TIMESTOPNAME = "OnPlayerStopTime";

    private void Awake()
    {
        _animationSubscriber.SubscribeAction("WindLaunchEffect", PlayWindEffect);
        _animationSubscriber.SubscribeAction("SwordHitEffect", PlaySwordHitEffect);
    }

    void PlayWindEffect()
    {
        Vector3 velocity = System.MathF.Sign(transform.position.x) * Vector3.right;
        _windImpulseSource.GenerateImpulseWithVelocity(windImpulseForce * velocity);

        MakeTimeStop(windTimeStopDuration, 0);
    }

    void PlaySwordHitEffect()
    {
        _swordImpulseSource.GenerateImpulseWithForce(swordImpulseForce);

        MakeTimeStop(swordTimeStopDuration, swordTimeResumeDuration);
    }

    public void PlayDamagedEffect()
    {
        _damagedImpulseSource.GenerateImpulseWithForce(damagedImpulseForce);

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
