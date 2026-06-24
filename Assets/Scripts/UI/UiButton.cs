using UnityEngine;
using UnityEngine.EventSystems;

public class UiButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private Animator _animator;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Entered");
        ResetTriggers();
        _animator.SetTrigger("On");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Exited");
        ResetTriggers();
        _animator.SetTrigger("Off");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void ResetTriggers()
    {
        _animator.ResetTrigger("On");
        _animator.ResetTrigger("Off");
    }

}
