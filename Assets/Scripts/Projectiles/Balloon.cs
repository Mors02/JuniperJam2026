using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rb;
    [SerializeField]
    private float _initialForce;
    [SerializeField]
    private float _slowingForce;
    private Vector2 _target;

    [SerializeField]
    private Animator _animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform.position;
        Vector2 direction = _target - (Vector2)transform.position;
        //this.transform.position = Camera.main.ViewportToWorldPoint(new Vector2(1.5f, 1.5f));
        _rb.AddForce(direction.normalized * _initialForce, ForceMode2D.Impulse);
        _animator = GetComponent<Animator>();
    }

    public void FixedUpdate()
    {   
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * _slowingForce);
    }

    /// <summary>
    /// The balloon explodes
    /// </summary>
    public void Pop()
    {
        _animator.SetTrigger("Pop");
        Destroy(this.gameObject, .5f);

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Pop();
        }   
    }
}
