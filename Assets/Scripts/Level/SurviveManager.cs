using System.Collections.Generic;
using UnityEngine;

public class SurviveManager : MonoBehaviour
{


    private bool _active;
    [SerializeField]
    private float _attackCooldown;
    private float _attackTimer;
    [SerializeField]
    private int _maxProjectilesOnScreen;
    [SerializeField]
    private GameObject _projectilePrefab;
    private List<Balloon> _projectiles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _projectiles = new List<Balloon>();
        _attackTimer = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_active)
        {
            GameManager.Instance.CurrentWinCon.UpdateWinCon();
            _active = !GameManager.Instance.CurrentWinCon.CheckWinCon();
            _attackTimer += Time.fixedDeltaTime;

            if (_attackTimer >= _attackCooldown)
            {
                _attackTimer = 0f;

                float randomX = Random.Range(0f, 1f);
                float randomY = Random.Range(0f, 1f);

                //coordinates that go from -0.5 to 0 and 1 to 1.5 in both direction
                randomX = randomX > 0.5f? randomX + 0.5f : randomX - 0.5f;
                randomY = randomY > 0.5f? randomY + 0.5f : randomY - 0.5f;
                
                Vector2 position = Camera.main.ViewportToWorldPoint(new Vector2(randomX, randomY));
                Balloon balloon = Instantiate(_projectilePrefab, position, Quaternion.identity).GetComponent<Balloon>();
                
                if (_projectiles.Count >= _maxProjectilesOnScreen)
                {
                    Balloon _firstProj = _projectiles[0];
                    Debug.Log(_firstProj);
                    if (_firstProj != null)
                    {
                        _firstProj.Pop();
                        _projectiles.Remove(_firstProj);    
                    }
                    
                }
                _projectiles.Add(balloon);
            }
        }
    }

    public void Activate()
    {
        _active = true;
    }
}
