using AbyssWorks.Misc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMotion2D : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private CustomForce customForce;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        customForce = GetComponent<CustomForce>();
    }

    public void Move(Vector3 delta)
    {
        if (rb2D)
        {
            rb2D.position += (Vector2)delta;
        }
    }

    public void Rotate(Quaternion delta)
    {
        if (rb2D)
        {
            rb2D.MoveRotation(transform.rotation * delta);
        }
        else
        {
            transform.rotation *= delta;
        }
    }

    public void AddForce(Vector3 force, ForceMode forceMode)
    {
        if (customForce)
        {
            customForce.AddForce(force, forceMode);
        }
        else if(rb2D)
        {
            ForceMode2D forceMode2D = ForceMode2D.Force;
            switch(forceMode)
            {
                case ForceMode.Impulse:
                    forceMode2D = ForceMode2D.Impulse;
                    break;
                case ForceMode.VelocityChange:
                    forceMode2D = ForceMode2D.Impulse;
                    break;
                default:
                    break;
            }
            rb2D.AddForce(force, forceMode2D);
        }
    }
}
