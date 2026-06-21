using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeField]
    private string _nextLevel;
    [SerializeField]

    private Animator _curtainAnimator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject.FindGameObjectWithTag("Curtains").TryGetComponent(out _curtainAnimator);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Player") && GameManager.Instance.Won())
        {
            //collision.GetComponent<PlayerController>().StopCharacter();
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
