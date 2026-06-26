using AbyssWorks.AnimatorSignal;
using System.Collections;
using UnityEngine;

public class SwordProjectile : Projectile
{
    [Min(0)] public float destroyTime = 10f;
    [Min(0)] public float force = 10;
    [Min(0)] public int damage = 1;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationSubscriber _animationSubscriber;
    [SerializeField] private string _swordWaveAnim;

    private Rigidbody2D _rb;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        _animator.Play(_swordWaveAnim, 0, 0);
        _animationSubscriber.SubscribeEndAction(() =>
        {
            Destroy(gameObject);
        });
        

        _rb.AddForce(transform.right * force, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _owner) return;

        //to do
        //Stop enemies by stasis amount
        //effects

        if (collision.TryGetComponent<ITakeDamage>(out var iTakeDamage))
        {
            if (QuickHitEffectHandler.instance)
                QuickHitEffectHandler.instance.PlayHitEffect(transform.position);

            iTakeDamage.TakeDamage(new DamageInfo(damage));
        }
    }
}
