using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UiButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent onHover;

    private Animator _animator;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Entered");
        onHover?.Invoke();

        ResetTriggers();
        if(_animator) _animator.SetTrigger("On");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Exited");
        ResetTriggers();
        if (_animator) _animator.SetTrigger("Off");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void ResetTriggers()
    {
        if (!_animator) return;

        _animator.ResetTrigger("On");
        _animator.ResetTrigger("Off");
    }

}
