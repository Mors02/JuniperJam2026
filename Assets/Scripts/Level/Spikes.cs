using UnityEngine;

public class Spikes : MonoBehaviour
{
    [SerializeField, Min(0)] private float _knockbackScale = 2f;
    [SerializeField, Min(0)] private int _damage = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out ITakeDamage itakeDamage))
        {
            Vector2 knockback = (collision.transform.position - transform.position).normalized;

            itakeDamage.TakeDamage(new DamageInfo(_damage, DamageType.Knockback, 0, _knockbackScale * knockback));
        }
    }
}
