using System.Collections.Generic;
using UnityEngine;

public class WindProjectile : Projectile
{
    [Min(0)] public float destroyTime = 10f;
    public AnimationCurve animationCurve = AnimationCurve.Constant(0f, 1f, 1f);
    [Min(0)] public float maxSpeed = 10;
    [Min(0)] public float knockbackForceScale = 1f;
    [Min(0)] public int damage = 1;

    private Rigidbody2D _rb;
    private float _elapsed = 0;
    private float _perc = 0;

    private HashSet<GameObject> _damagedObjects = new();
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (destroyTime > 0)
        {
            if (_perc >= 1) Destroy(gameObject);

            _perc = Mathf.Clamp01(_elapsed / destroyTime);

            Move(animationCurve.Evaluate(_perc) * maxSpeed * transform.right);

            _elapsed += Time.fixedDeltaTime;
        }
        else Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _owner) return;

        if (_damagedObjects.Contains(collision.gameObject)) return;

        _damagedObjects.Add(collision.gameObject);

        //to do
        //Stop enemies by stasis amount
        //effects

        if (collision.TryGetComponent<ITakeDamage>(out var iTakeDamage))
        {
            Vector2 knockback = (collision.transform.position - transform.position).normalized;
            knockback.y = 0;

            iTakeDamage.TakeDamage(new DamageInfo(damage, DamageType.Knockback, 0, 
                knockbackForceScale * knockback));
        }
    }

    void Move(Vector2 motion)
    {
        _rb.position += motion * Time.fixedDeltaTime;
    }
}
