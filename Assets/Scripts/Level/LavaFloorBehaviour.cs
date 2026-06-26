using UnityEngine;

public class LavaFloorBehaviour : MonoBehaviour
{

    [SerializeField]
    private float _speed;
    [SerializeField]
    private Transform _transform;
    private bool _active;

    [SerializeField]
    private float _knockbackForce;
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private float _lerpSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_active)
        {
            float verticalMovement = _speed * Time.fixedDeltaTime;
            Vector2 position = Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 0));
            this._transform.position = Vector2.Lerp(this._transform.position, new Vector2(position.x, _transform.position.y + verticalMovement), _lerpSpeed);
        }
    }

    public void Activate()
    {
        _active = true;
        _animator.SetTrigger("Enter");
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            rb.linearVelocityY = 0;
            rb.AddForceY(_knockbackForce);
            DamageInfo info = new DamageInfo(3, DamageType.Normal);


            collision.gameObject.GetComponent<ITakeDamage>().TakeDamage(info);
        }
    }
}
