using System;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Action<Collider2D> onEnter2D;
    public Action<Collider2D> onStay2D;
    public Action<Collider2D> onExit2D;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onEnter2D?.Invoke(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        onStay2D?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        onExit2D?.Invoke(collision);
    }
}
