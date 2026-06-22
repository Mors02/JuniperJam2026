using UnityEngine;

public class LavaFloorBehaviour : MonoBehaviour
{

    [SerializeField]
    private float _speed;
    private bool _active;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_active)
        {
            float verticalMovement = _speed * Time.fixedDeltaTime;
            this.transform.position = new Vector2(transform.position.x, transform.position.y + verticalMovement);
        }
    }

    public void Activate()
    {
        _active = true;
    }
}
