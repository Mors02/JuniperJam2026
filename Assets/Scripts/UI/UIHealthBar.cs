using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    
    private DamageReceiver _playerDamage;

    private Slider _slider;

    private int _targetPercentage;
    [SerializeField]
    private float _sliderSpeed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         GameObject player = GameObject.FindGameObjectWithTag("Player");
         _playerDamage = player.GetComponent<DamageReceiver>();
         _playerDamage.OnHealthChanged.AddListener(UpdateHealthUI);
    }

    public void Update()
    {
        
        float nextValue = Mathf.Lerp(_slider.value, _targetPercentage, _sliderSpeed);

        _slider.value = nextValue;
    }

    private void UpdateHealthUI(int health)
    {
        _targetPercentage = health / _playerDamage.MaxHealth;
    }
}
