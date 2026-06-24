using System;
using UnityEngine;

public class ExternalCollider : MonoBehaviour
{
    public Action<Collider2D> onExtTriggerEnter2D;
    public Action<Collider2D> onExtTriggerStay2D;
    public Action<Collider2D> onExtTriggerExit2D;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onExtTriggerEnter2D?.Invoke(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        onExtTriggerStay2D?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        onExtTriggerExit2D?.Invoke(collision);
    }
}
