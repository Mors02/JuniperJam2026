using System.Collections;
using TMPro;
using UnityEngine;

public class Spawn : MonoBehaviour
{

    //[SerializeField]
    private GameObject _player;

    [SerializeField]
    private Transform _spawnPos;

    [SerializeField]
    private float _timeToSpawn;

    private Animator _animator;

    [SerializeField]
    private Collider2D _closeCollider;

    [SerializeField]
    private float _spawnForce;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _player.transform.position = _spawnPos.position;
        
        StartCoroutine("SpawnRoutine");
    }


    public IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(_timeToSpawn);
        Debug.Log("Started");
        _player.GetComponent<Rigidbody2D>().AddForce(new Vector2(_spawnForce, _spawnForce));        
        yield return new WaitForSeconds(0.1f);
        _closeCollider.enabled = true;
        _animator.SetTrigger("Activate");
    }
}
