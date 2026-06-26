using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class QuickHitEffectHandler : MonoBehaviour
{
    [Min(0)] public float timeStopDuration = 0.05f;
    [Min(0)] public float timeResumeDuration = 0f;
    [Min(0)] public float impulseForce = 0.1f;
    [Min(1)] public float maxDistance = 20f;

    public static QuickHitEffectHandler instance;

    private const string TIMESTOPNAME = "HitTimeStop";

    private CinemachineImpulseSource _impulseSource;

    private Coroutine _timeStopRoutine;

    private Transform _playerTransform;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _playerTransform = GameObject.FindWithTag("Player").transform;
        
    }

    public void PlayHitEffect(Vector3 position)
    {
        float percDist = 1f;
        if (_playerTransform)
        {
            float distance = Mathf.Min(Vector3.Distance(_playerTransform.position, position), maxDistance);
            percDist = 1 - distance / maxDistance;
        }

        _impulseSource.GenerateImpulseWithForce(impulseForce * percDist);

        _timeStopRoutine ??= StartCoroutine(TimeStopEnumerator());
    }

    IEnumerator TimeStopEnumerator()
    {
        float stopTime = timeStopDuration;
        float resumeTime = timeResumeDuration;

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
