using AbyssWorks.FMODAudioManager;
using TMPro;
using UnityEngine;

public class AudioManagerDebugger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countGui;
    
    // Update is called once per frame
    void Update()
    {
        if (FMODAudioManager.Instance)
        {
            countGui.text = $"Registered Audio: {FMODAudioManager.Instance.RegisteredCount}";
        }
    }
}
