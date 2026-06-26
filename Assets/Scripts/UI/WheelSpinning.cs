using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using AbyssWorks.FMODAudioManager;
using Unity.VisualScripting;


public class WheelSpinning : MonoBehaviour
{

    [SerializeField]
    private UIManager _uiManager;
    private Animator _animator;
    private bool _isSpinning = false;
    [SerializeField]
    private float _spinSpeed;
    [Range(0f, 1f)]
    [SerializeField]
    private float _spinSpeedRandomness;
    private float _rotation = 0;
    [SerializeField]
    private Image _wheelImage;
    [Range(1f, 6f)]
    [SerializeField]
    private float _fastSpinningFriction;
    [Range(0.1f, 1f)]
    [SerializeField]
    private float _slowSpinningFriction;
    [Range(0.5f, 4f)]
    [SerializeField]
    private float _fastToSlowValue;

    [SerializeField]
    private TMP_Text _announcementText;

    [SerializeField]
    private List<WheelReward> _rewards;
    private int _rewardNumber = -1;

    [SerializeField]
    private FMODAudioScriptable _wheelAudio;

    [SerializeField]
    private ParticleSystem _confetti;

    [SerializeField]
    private List<Sprite> _confettiSprites;
    private const string WHEELSPINPARAMETER = "WheelSpinState";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rotation = 0;
        _isSpinning = false;
        _rewardNumber = -1;
        Spin();
        _animator = GetComponent<Animator>();

        //add some randomness
        _spinSpeed = Random.Range(_spinSpeed-_spinSpeedRandomness, _spinSpeed+_spinSpeedRandomness);
        _rotation = Random.Range(0f, 360f);
        
        _wheelImage.transform.localRotation = Quaternion.Euler(0, 0, -_rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isSpinning)
        {
            if (_spinSpeed > _fastToSlowValue)
            {
                if (FMODAudioManager.Instance && _wheelAudio)
                    FMODAudioManager.Instance.SetParameterByName(_wheelAudio, WHEELSPINPARAMETER, 0);

                _spinSpeed -= _fastSpinningFriction * Time.unscaledDeltaTime;
            } else
            {
                if (FMODAudioManager.Instance && _wheelAudio)
                    FMODAudioManager.Instance.SetParameterByName(_wheelAudio, WHEELSPINPARAMETER, 1);

                _spinSpeed -= _slowSpinningFriction * Time.unscaledDeltaTime;
            }  

            _spinSpeed = Mathf.Max(0, _spinSpeed);

            _rotation += 100 * Time.unscaledDeltaTime * _spinSpeed;
            _wheelImage.transform.localRotation = Quaternion.Euler(0, 0, -_rotation);

            if (_spinSpeed <= 0)
            {
                _spinSpeed = 0;
                _isSpinning = false;
                //get the current rotation and divide it by the reward angle (360 divided by the number of rewards)
                _rewardNumber = (int)_rotation % 360 / (360 / _rewards.Count);
               // this.ShootConfetti();

                if (FMODAudioManager.Instance && _wheelAudio)
                    FMODAudioManager.Instance.StopAudio(_wheelAudio);
            }
        } 
        
        if (_rewardNumber >= 0)
        {
            
            _animator.SetTrigger("Exit");
            _announcementText.text = _rewards[_rewardNumber].Name;
            _rewards[_rewardNumber].Execute();
            _rewardNumber = -1;
        }
    }

    public void Spin()
    {
        if (!_isSpinning)
        {
            _isSpinning = true;

            if (FMODAudioManager.Instance && _wheelAudio)
                FMODAudioManager.Instance.PlayAudio(_wheelAudio);
        }
    }

    public void OpenCurtains()
    {
        _uiManager.OpenCurtains();
    }

    public void ShootConfetti()
    {
        this._confetti.Play();
        Debug.Log(this._confetti.isPlaying);
    }
}
