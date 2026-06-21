using UnityEngine;
using TMPro;
public class WinconUI : MonoBehaviour
{

    [SerializeField]
    private TMP_Text _winConProgressText;
    [SerializeField]
    private TMP_Text _winConDescriptionText;
    private Animator _animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    
    public void UpdateWinConUI(string progressText, bool completed)
    {
        _winConProgressText.text = progressText;

        if (completed)
            _winConProgressText.color = Color.green;
    }

    public void UpdateWinConDescriptionUI(WinCon winCon)
    {
        _winConDescriptionText.text = winCon.Description;
        winCon.UpdateUI();
        _animator.SetTrigger("Enter");
    }
}
