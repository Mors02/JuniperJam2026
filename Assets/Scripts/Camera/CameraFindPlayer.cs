using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCameraBase))]
public class CameraFindPlayer : MonoBehaviour
{
    private CinemachineVirtualCameraBase _cineCam;

    private void Awake()
    {
        _cineCam = GetComponent<CinemachineVirtualCameraBase>();

        if (!_cineCam.Follow)
        {
            var player = GameObject.FindWithTag("Player");
            _cineCam.Follow = player.transform;
        }
    }
}
