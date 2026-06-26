using AbyssWorks.AnimatorSignal;
using AbyssWorks.FMODAudioManager;
using System.Collections;
using UnityEngine;

public class StasisProjectile : Projectile
{
    [Min(0)] public float destroyTime = 10f;
    [Min(0)] public float force = 10;
    [Min(0)] public float stasisDuration = 1f;
    [Min(0)] public int damage = 1;
    [SerializeField] private FMODAudioScriptable _impactAudio;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationSubscriber _animationSubscriber;
    [SerializeField] private string _startAnim;
    [SerializeField] private string _endAnim;

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
        _animationSubscriber.SubscribeAction("StasisEnd", () =>
        {
            Destroy(gameObject);
        });

        _rb.AddForce(transform.right * force, ForceMode2D.Impulse);

        _destroyRoutine = StartCoroutine(DestroyEnumerator(destroyTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _owner) return;

        int audioHitCheck = 0;

        if (collision.TryGetComponent<ITakeDamage>(out var iTakeDamage))
        {
            if (QuickHitEffectHandler.instance)
                QuickHitEffectHandler.instance.PlayHitEffect(transform.position);

            audioHitCheck = 1;

            iTakeDamage.TakeDamage(new DamageInfo(damage, DamageType.Stasis, stasisDuration));
        }

        if (FMODAudioManager.Instance && _impactAudio)
        {
            var impactInstance = FMODAudioManager.Instance.PlayOnce(_impactAudio, transform.position, true);
            if (impactInstance.HasValue) impactInstance.Value.setParameterByName("StasisHitCheck", audioHitCheck);
        }

        _collider2D.enabled = false;
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;

        _animator.Play(_endAnim);

        if (_destroyRoutine != null) StopCoroutine(_destroyRoutine);
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
