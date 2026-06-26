using AbyssWorks.AnimatorSignal;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireProjectile : Projectile
{
    [Min(0)] public float destroyTime = 10f;
    [Min(0)] public float force = 10;
    public int bounce = 1;
    [Min(0)] public float damp = 0.95f;
    [Min(0)] public int damage = 1;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationSubscriber _animationSubscriber;
    [SerializeField] private string _startAnim;
    [SerializeField] private string _endAnim;

    private int _bounceCount;

    private Rigidbody2D _rb;
    private Collider2D _collider2D;

    private Coroutine _destroyRoutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator.Play(_startAnim, 0, 0);
        _animationSubscriber.SubscribeAction("FireballEnd", () =>
        {
            Destroy(gameObject);
        });

        _rb.AddForce(transform.right * force, ForceMode2D.Impulse);

        _destroyRoutine = StartCoroutine(DestroyEnumerator(destroyTime));
    }

    private void Update()
    {
         if(_collider2D.enabled) transform.right = _rb.linearVelocity.normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _owner) return;

        if (collision.TryGetComponent<ITakeDamage>(out var iTakeDamage))
        {
            if (QuickHitEffectHandler.instance)
                QuickHitEffectHandler.instance.PlayHitEffect(transform.position);

            iTakeDamage.TakeDamage(new DamageInfo(damage, DamageType.Normal));
        }

        if (_bounceCount >= bounce)
        {
            _collider2D.enabled = false;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;

            _animator.Play(_endAnim);

            if (_destroyRoutine != null) StopCoroutine(_destroyRoutine);

            return;
        }

        _bounceCount++;

        Vector2 hitPoint = collision.ClosestPoint(transform.position);
        Vector2 normal = ((Vector2)transform.position - hitPoint).normalized;

        if (normal == Vector2.zero)
        {
            normal = (transform.position - collision.transform.position).normalized;
            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(normal * force * damp, ForceMode2D.Impulse);
        }
        else _rb.linearVelocity = Vector2.Reflect(_rb.linearVelocity, normal) * damp;

        Debug.DrawRay(hitPoint, normal * 2f, Color.red, 2f);
    }

    IEnumerator DestroyEnumerator(float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            yield return null;
        }

        _collider2D.enabled = false;
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;

        _animator.Play(_endAnim);

        _destroyRoutine = null;

        Destroy(gameObject);
    }

}
