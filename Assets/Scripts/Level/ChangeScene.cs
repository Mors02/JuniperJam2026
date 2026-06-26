using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeField]
    private string _nextLevel;
    [SerializeField]

    private Animator _curtainAnimator;
    private Animator _animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject.FindGameObjectWithTag("Curtains").TryGetComponent(out _curtainAnimator);
        _animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Player") && GameManager.Instance.Won())
        {
            _animator.SetTrigger("Activate");
            //collision.GetComponent<PlayerController>().Hide();
            StartCoroutine(ChangeSceneRoutine());
            
            if (_curtainAnimator)
                _curtainAnimator.SetTrigger("Exit");
        }
            
    }

    private IEnumerator ChangeSceneRoutine()
    {
        yield return new WaitForSeconds(0.90f);
        SceneManager.LoadScene(_nextLevel);
    }
}
