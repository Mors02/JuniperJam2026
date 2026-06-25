using UnityEngine;
using UnityEngine.UI;

public class UICarrot : MonoBehaviour
{
    [SerializeField]
    private Image _image;
    
    private float _targetTransparency;
    [SerializeField]
    private float _transparencySpeed;
    [SerializeField]
    private Animator _animator;

    private int _previousChange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float currentAlpha = this._image.color.a;

        float nextAlpha = Mathf.Lerp(currentAlpha, _targetTransparency, _transparencySpeed);

        _image.color = new Color(1, 1, 1, nextAlpha);
    }

    public void SetTargetGradiant(int val)
    {
        if (val < 10 /*&& _previousChange == 10*/)
            _animator.SetTrigger("Hit");

        _previousChange = val;
        _targetTransparency = val * 0.1f;
        
    }
}
