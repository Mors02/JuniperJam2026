using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform2D : MonoBehaviour
{
    protected Dictionary<GameObject, PlatformMotion2D> objectsOnPlatformDict = new();

    public int CountOfObjectsOnPlatform => objectsOnPlatformDict.Count;

    public void MoveObjectsOnPlatform(Vector3 delta)
    {
        foreach (var obj in objectsOnPlatformDict)
        {
            if (obj.Key && obj.Value)
            {
                obj.Value.Move(delta);
            }
        }
    }

    public void RotateObjectsOnPlatform(Quaternion delta)
    {
        Vector3 offset;
        Vector3 rotatedOffset;
        Vector3 translateDelta;
        foreach (var obj in objectsOnPlatformDict)
        {
            if (obj.Key && obj.Value)
            {
                obj.Value.Rotate(delta);
                offset = obj.Value.transform.position - transform.position;
                rotatedOffset = delta * offset;
                translateDelta = rotatedOffset - offset;
                translateDelta.y = 0;
                obj.Value.Move(translateDelta);
            }
        }
    }
    /* //Trigger
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        var other = collision;

        if (other.TryGetComponent<PlatformMotion2D>(out var platformMotion))
        {
            objectsOnPlatformDict[other.gameObject] = platformMotion;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        var other = collision;

        if (objectsOnPlatformDict.ContainsKey(other.gameObject))
        {
            objectsOnPlatformDict.Remove(other.gameObject);
        }
    }
    */

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.collider;

        if (other.TryGetComponent<PlatformMotion2D>(out var platformMotion))
        {
            objectsOnPlatformDict[other.gameObject] = platformMotion;
        }
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        var other = collision.collider;

        if (objectsOnPlatformDict.ContainsKey(other.gameObject))
        {
            objectsOnPlatformDict.Remove(other.gameObject);
        }
    }
}
