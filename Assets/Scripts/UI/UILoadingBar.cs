using UnityEngine;
using UnityEngine.UI;

public class UILoadingBar : MonoBehaviour
{

    [SerializeField]
    private Slider _slider;

    private float _targetPercentage;
    [SerializeField]
    private float _sliderSpeed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
         _targetPercentage = 0;
    }

    public void Update()
    {
        
        float nextValue = Mathf.Lerp(_slider.value, _targetPercentage, _sliderSpeed);

        _slider.value = nextValue;
    }

    public void UpdateBar(float newPercent)
    {
        
        _targetPercentage = newPercent;
    }
}
