using AbyssWorks.AnimatorSignal;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationSubscriber _animationSubscriber;

    private void Awake()
    {
        _animationSubscriber.SubscribeEndAction(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Appear(Vector2 position, Quaternion rotation)
    {
        gameObject.SetActive(true);
        transform.position = position;
        transform.rotation = rotation;
        _animator.Play("CloudAppear", 0, 0);
    }

    public void Disappear()
    {
        if (!gameObject.activeSelf) return;

        _animator.Play("CloudDisappear", 0, 0);
    }
}
