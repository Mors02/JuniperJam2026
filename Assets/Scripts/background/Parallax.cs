using UnityEngine;

public class Parallax : MonoBehaviour
{
private float length, startposx, startposy, height;
public GameObject cam;
public float parallaxEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startposx = transform.position.x;
        startposy = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        height = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float xtemp = (cam.transform.position.x*(1 - parallaxEffect));
        float ytemp = (cam.transform.position.y*(1 - parallaxEffect));
        
        float xdist = (cam.transform.position.x * parallaxEffect);
        float ydist = (cam.transform.position.y * parallaxEffect);

        transform.position = new Vector3(startposx + xdist, startposy + ydist, transform.position.z);

        if (xtemp > startposx + length) startposx += length;
        else if (xtemp < startposx - length) startposx -= length;

        if (ytemp > startposy + height) startposy += height;
        else if (ytemp < startposy - height) startposy -= height;
    }
}
