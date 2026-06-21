using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireProjectile : Projectile
{
    [Min(0)] public float destroyTime = 10f;
    [Min(0)] public float force = 10;
    public int bounce = 1;
    [Min(0)] public float damp = 0.95f;

    private int _bounceCount;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.AddForce(transform.right * force, ForceMode2D.Impulse);

        Destroy(gameObject, destroyTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _owner) return;

        if (_bounceCount >= bounce) Destroy(gameObject);

        _bounceCount++;

        Vector2 hitPoint = collision.ClosestPoint(transform.position);
        Vector2 normal = ((Vector2)transform.position - hitPoint).normalized;

        if (normal == Vector2.zero)
        {
            normal = (transform.position - collision.transform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(normal * force * damp, ForceMode2D.Impulse);
        }
        else rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normal) * damp;

        Debug.DrawRay(hitPoint, normal * 2f, Color.red, 2f);
    }

}
