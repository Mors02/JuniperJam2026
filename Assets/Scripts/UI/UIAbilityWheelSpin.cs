using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityWheelSpin : MonoBehaviour
{
    [SerializeField] private Image _abilityImage;
    [SerializeField, Min(0)] private float _spinSpeed;
    [SerializeField, Min(0)] private float _minSpinDuration;
    [SerializeField, Min(0)] private float _maxSpinDuration;
    [SerializeField, Min(0)] private float _lerpToSpeed = 0.5f;
    [SerializeField, Range(0, 1)] private float _speedUpRange = 0.5f;
    [SerializeField, Min(1)] private float _speedUpScale = 1.5f;

    [SerializeField] private List<AbilityReward> _rewards = new();

    public Action<string> onSpinEnd;

    private Coroutine _spinRoutine;

    private Animator _animator;

    int _curIndex = -1;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

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

        if(_curIndex >= 0) _animator.Play("WheelAbilityHide", 0, 0);

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
        float perc = 0;
        float speedPercRange = _speedUpRange;
        float minLerpPercRange = (_speedUpRange + 1) / 2f;
        while (elapsed < duration)
        {
            perc = elapsed / duration;

            if (perc < _speedUpRange) transform.Rotate(maxSpinSpeed * Time.deltaTime * Vector3.forward);
            else
            {
                float lerpPercScale = Mathf.InverseLerp(speedPercRange, minLerpPercRange, perc);
                float addSpinSpeed = maxSpinSpeed + maxSpinSpeed * _speedUpScale * lerpPercScale;
                transform.Rotate(addSpinSpeed * Time.deltaTime * Vector3.forward);
            }

            elapsed += Time.deltaTime;

            yield return null;
        }

        float rotation = transform.rotation.eulerAngles.z;

        int rewardIndex = (int)(rotation % 360 / ((float)360 / _rewards.Count));

        _curIndex = rewardIndex;

        _abilityImage.transform.rotation = Quaternion.identity;
        _abilityImage.sprite = _rewards[rewardIndex].sprite;

        _animator.Play("WheelAbilityShow", 0, 0);

        onSpinEnd?.Invoke(_rewards[rewardIndex].Name);

        _spinRoutine = null;
    }
}
