using UnityEngine;

// I ripped this off the internet
public class ParallaxController : MonoBehaviour
{
    private Vector2 size;
    private Vector2 startPos;
    public GameObject cam;
    public float parallaxEffect;

    void Start()
    {
        startPos = transform.position;
        size = GetComponent<SpriteRenderer>().bounds.size;
    }

    void Update()
    {
        Vector2 temp = (cam.transform.position * (1 - parallaxEffect));
        Vector2 dist = (cam.transform.position * parallaxEffect);

        transform.position = new Vector3(startPos.x + dist.x, startPos.y + dist.y, transform.position.z);

        //if (temp > startpos + length) startpos += length;
        //else if (temp < startpos - length) startpos -= length;
    }
}