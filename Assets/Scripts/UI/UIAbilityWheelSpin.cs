using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAbilityWheelSpin : MonoBehaviour
{
    [SerializeField, Min(0)] private float _spinSpeed;
    [SerializeField, Min(0)] private float _minSpinDuration;
    [SerializeField, Min(0)] private float _maxSpinDuration;
    [SerializeField, Min(0)] private float _lerpToSpeed = 0.5f;

    [SerializeField] private List<AbilityReward> _rewards = new();

    public Action<string> onSpinEnd;

    private Coroutine _spinRoutine;

    private void Start()
    {
        Spin();
    }

    public void Spin()
    {
        _spinRoutine ??= StartCoroutine(SpinEnumerator());
    }

    IEnumerator SpinEnumerator()
    {
        if (_rewards.Count == 0) yield break;

        float maxSpinSpeed = _spinSpeed;

        float spinSpeed = 0;

        float duration = _lerpToSpeed;
        float elapsed = 0;
        while (elapsed < duration)
        {
            spinSpeed = Mathf.Lerp(spinSpeed, _spinSpeed, elapsed / duration);

            transform.Rotate(spinSpeed * Time.deltaTime * Vector3.forward);

            elapsed += Time.deltaTime;

            yield return null;
        }

        duration = UnityEngine.Random.Range(_minSpinDuration, _maxSpinDuration);
        elapsed = 0;
        while (elapsed < duration)
        {
            transform.Rotate(maxSpinSpeed * Time.deltaTime * Vector3.forward);

            elapsed += Time.deltaTime;

            yield return null;
        }

        float rotation = transform.rotation.eulerAngles.z;

        int rewardIndex = (int)(rotation % 360 / ((float)360 / _rewards.Count));

        onSpinEnd?.Invoke(_rewards[rewardIndex].Name);

        _spinRoutine = null;
    }
}
