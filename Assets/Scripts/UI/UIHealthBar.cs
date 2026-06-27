using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    
    private DamageReceiver _playerDamage;

    [SerializeField]
    private Image _sliderImage;

    private float _targetPercentage;
    [SerializeField]
    private float _sliderSpeed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         GameObject player = GameObject.FindGameObjectWithTag("Player");
         _playerDamage = player.GetComponent<DamageReceiver>();
         _playerDamage.OnHealthChanged.AddListener(UpdateHealthUI);
         _targetPercentage = 1;
    }

    public void Update()
    {
        
        float nextValue = Mathf.Lerp(_sliderImage.fillAmount, _targetPercentage, _sliderSpeed);

        _sliderImage.fillAmount = nextValue;
    }

    private void UpdateHealthUI(int health)
    {
        
        _targetPercentage = (float)health / (float)_playerDamage.MaxHealth;
        Debug.Log(health + " " + _playerDamage.MaxHealth + " " + _targetPercentage);
    }
}
