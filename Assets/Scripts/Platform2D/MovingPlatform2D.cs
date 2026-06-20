using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform2D : Platform2D
{
    private enum PlatformStatus
    {
        moving,
        idling
    }

    public Transform nextLocation;
    public float idleDuration = 4f;
    public float moveSpeed = 5f;
    public float distanceToNext = 0.1f;
    public bool lerpMovement = true;

    private Vector3 initialLoc;

    private Vector3 nextLoc;
    private PlatformStatus currentStatus = PlatformStatus.idling;
    private float idleTime;

    // Start is called before the first frame update
    void Start()
    {
        initialLoc = transform.position;
        currentStatus = PlatformStatus.idling;
        idleTime = Time.time;
        nextLoc = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (currentStatus)
        {
            case PlatformStatus.idling:
                if (Time.time - idleTime > idleDuration)
                {
                    if (nextLoc != initialLoc) nextLoc = initialLoc;
                    else nextLoc = nextLocation.position;
                    currentStatus = PlatformStatus.moving;

                    break;
                }

                break;
            case PlatformStatus.moving:
                Vector3 nextPosition = Vector3.zero;
                if (lerpMovement) nextPosition = Vector3.Lerp(transform.position, nextLoc, moveSpeed * Time.fixedDeltaTime);
                else nextPosition = Vector3.MoveTowards(transform.position, nextLoc, moveSpeed * Time.fixedDeltaTime);

                Vector3 delta = nextPosition - transform.position;
                if (delta.y > 0)
                {
                    MoveObjectsOnPlatform(delta);
                    transform.position += delta;
                }
                else
                {
                    transform.position += delta;
                    MoveObjectsOnPlatform(delta);
                }

                if (Vector3.Distance(transform.position, nextLoc) < distanceToNext)
                {
                    idleTime = Time.time;
                    currentStatus = PlatformStatus.idling;
                    break;
                }
                break;
            default:
                break;
        }

        //Quaternion prevRot = transform.rotation;
        //transform.Rotate(Vector3.up, 30 * Time.fixedDeltaTime, 0);
        //Quaternion deltaRot = transform.rotation * Quaternion.Inverse(prevRot);
        //RotateObjectsOnPlatform(deltaRot);

    }
}
