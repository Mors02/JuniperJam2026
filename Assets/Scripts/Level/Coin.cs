using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private Collider2D _collider;

    private float _animationTimer;
    private float _nextJumpIn;
    [Range(1f, 3f)]
    [SerializeField]
    private float _minimumJumpTime;

    [Range(3f, 7f)]
    [SerializeField]
    private float _maximumJumpTime;
    private bool _active;
    private void Start()
    {
        _collider.enabled = false;
        _nextJumpIn = Random.Range(_minimumJumpTime, _maximumJumpTime);
        _active = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (_active)
        {
            _animationTimer += Time.deltaTime;

            if (_animationTimer >= _nextJumpIn)
            {
                _animator.SetTrigger("Jump");
                _nextJumpIn = Random.Range(_minimumJumpTime, _maximumJumpTime);
                _animationTimer = 0f;    
            }    
        }
        
    }

    public void Spawn()
    {
        _collider.enabled = true;
        _animator.SetTrigger("Activate");
        _active = true;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision is BoxCollider2D)
        {
            this._active = false;
            GameManager.Instance.CurrentWinCon.UpdateWinCon();
            _animator.SetTrigger("Collect");
            _collider.enabled = false;
            _active = false;
        }
    }


}
