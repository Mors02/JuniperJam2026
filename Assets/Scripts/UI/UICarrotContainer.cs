using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

public class UICarrotContainer : MonoBehaviour
{
    List<UICarrot> _carrots;

    [SerializeField]
    private GameObject _carrotPrefab;

    private DamageReceiver _playerDamage;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _carrots = new List<UICarrot>();
        //_playerController = player.GetComponent<PlayerSMController>();
        _playerDamage = player.GetComponent<DamageReceiver>();
        
        _playerDamage.OnHealthChanged.AddListener(UpdateHealthUI);
        for(int i = 0; i < 10; i++)
        {
            GameObject _carrot = Instantiate(_carrotPrefab, this.transform);
            _carrot.name = "Carrot" + i;
            _carrots.Add(_carrot.GetComponent<UICarrot>());
        }
    }

    private void UpdateHealthUI(int health)
    {
        for (int i = 0; i < 10; i++)
        {
            int gradient = 0;
            int high = (i+1)*10 -1;
            if (high < health)
                gradient = 10;
            else
                gradient = health - i * 10;

            _carrots[i].SetTargetGradiant(Math.Max(0, gradient));
        }
    }

}
