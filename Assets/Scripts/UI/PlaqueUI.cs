using TMPro;
using UnityEngine;

public class PlaqueUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text;

    [SerializeField]
    private string _name;
    [SerializeField]

    private string _url;

    public void OpenUrl()
    {
        Application.OpenURL(_url);
    }

    private void Start()
    {
        _text.text = _name;
    }
}
