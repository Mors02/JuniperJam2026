using Unity.VisualScripting;
using UnityEngine;

public class SceneInit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.Init();
    }
}
