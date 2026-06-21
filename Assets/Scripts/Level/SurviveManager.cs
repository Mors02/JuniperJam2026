using UnityEngine;

public class SurviveManager : MonoBehaviour
{

    private bool _active;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (_active)
        {
            GameManager.Instance.CurrentWinCon.UpdateWinCon();
            _active = !GameManager.Instance.CurrentWinCon.CheckWinCon();
        }
    }

    public void Activate()
    {
        Debug.Log("Activated");
        _active = true;
    }
}
