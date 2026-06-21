using UnityEngine;

public class StasisProjectile : Projectile
{
    [Min(0)] public float destroyTime = 10f;
    [Min(0)] public float force = 10;
    [Min(0)] public float stasisDuration = 1f;

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

        //to do
        //Stop enemies by stasis amount
        //effects

        Destroy(gameObject);
    }
}
