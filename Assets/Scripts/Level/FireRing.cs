using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class FireRing : MonoBehaviour
{

    [SerializeField]
    private bool _active;

    [SerializeField]
    private float _knockbackForce;

    [SerializeField]
    private Animator _animator;

    private Vector2 _entrancePoint;
    private bool _enteredRight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetAllColliders(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _entrancePoint = collision.gameObject.transform.position;
            _enteredRight = _entrancePoint.x > transform.position.x;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if ((_enteredRight && collision.transform.position.x < transform.position.x) || 
            (!_enteredRight && collision.transform.position.x > transform.position.x))
            {
                _animator.SetTrigger("Collect");
                if (GameManager.Instance.CurrentWinCon.Type == WinConType.FireRings)
                    GameManager.Instance.CurrentWinCon.UpdateWinCon();
                SetAllColliders(false);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            //rb.AddForce(rb.linearVelocity.normalized * _knockbackForce, ForceMode2D.Impulse);
            collision.gameObject.GetComponent<ITakeDamage>().TakeDamage(new DamageInfo(1));
        }
    }


    public void Activate()
    {
        _active = true;
        _animator.SetTrigger("Activate");
        SetAllColliders(true);
    }

    private void SetAllColliders(bool enabled)
    {
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = enabled;
        }
    }
}
