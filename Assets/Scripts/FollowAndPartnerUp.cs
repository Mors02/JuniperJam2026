using UnityEngine;

public class FollowAndPartnerUp : MonoBehaviour
{
    public Transform lavaLineRenderer;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;

        if (lavaLineRenderer) lavaLineRenderer.transform.position = transform.position;
    }
}
