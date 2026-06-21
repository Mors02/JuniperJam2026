using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected GameObject _owner;
    public GameObject Owner => _owner;

    public void SetOwner(GameObject owner) => _owner = owner;
}
