using UnityEngine;
using UnityEngine.UI;
public class UIImageContainer : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    private Sprite _nextSprite;

    [SerializeField]
    private Animator _animator;
    

    public void ChangeImage()
    {
        this._image.sprite = _nextSprite;
    }

    public void Change(Sprite nextSprite)
    {
        if (_image.sprite == null)
        {
            _animator.ResetTrigger("Appear");
            _animator.SetTrigger("Appear");    
        } else
        {
            _animator.ResetTrigger("Change");
            _animator.SetTrigger("Change");    
        }

        _nextSprite = nextSprite;
    }
}
