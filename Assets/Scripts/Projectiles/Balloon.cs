using AbyssWorks.FMODAudioManager;
using UnityEditor.Animations;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rb;
    [SerializeField]
    private float _initialForce;
    [SerializeField]
    private float _slowingForce;
    private Vector2 _target;

    [SerializeField]
    private RuntimeAnimatorController[] _controllers;

    [SerializeField]
    private Animator _animator;

    [SerializeField] private DeathExplosionEffect _deathEffect; 

    [SerializeField]
    private FMODAudioScriptable _popAudio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform.position;
        Vector2 direction = _target - (Vector2)transform.position;
        //this.transform.position = Camera.main.ViewportToWorldPoint(new Vector2(1.5f, 1.5f));
        _rb.AddForce(direction.normalized * _initialForce, ForceMode2D.Impulse);
        _animator = GetComponent<Animator>();
        int index = Random.Range(0, _controllers.Length);
        Debug.Log(_controllers[index]);
        //this._animator.enabled = false;
        this._animator.runtimeAnimatorController = _controllers[index];
        //this._animator.enabled = true;
    }

    public void FixedUpdate()
    {   
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * _slowingForce);
    }

    /// <summary>
    /// The balloon explodes
    /// </summary>
    public void Pop()
    {
        _deathEffect.PlayEffect();
        _animator.SetTrigger("Pop");

        if (_popAudio && FMODAudioManager.Instance)
            FMODAudioManager.Instance.PlayOnce(_popAudio, transform.position);

        Destroy(this.gameObject, .5f);

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DamageInfo info = new DamageInfo(1, DamageType.None);
            collision.GetComponent<ITakeDamage>().TakeDamage(info);
            Pop();
        }   
    }
}
