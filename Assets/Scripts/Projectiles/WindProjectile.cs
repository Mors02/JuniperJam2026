using UnityEngine;

public class WindProjectile : MonoBehaviour
{
    [Min(0)] public float destroyTime = 10f;
    public AnimationCurve animationCurve = AnimationCurve.Constant(0f, 1f, 1f);
    [Min(0)] public float maxSpeed = 10;
    [Min(0)] public float knockbackForceScale = 1f;
    [Min(0)] public int damage = 1;

    private Rigidbody2D _rb;
    private float _elapsed = 0;
    private float _perc = 0;
    
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

    void Move(Vector2 motion)
    {
        _rb.position += motion * Time.fixedDeltaTime;
    }
}
