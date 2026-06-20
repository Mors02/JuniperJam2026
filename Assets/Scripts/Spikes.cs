using UnityEngine;

public class Spikes : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<DamageReceiver>(out DamageReceiver receiver))
        {
            receiver.ReceiveDamage();
        }
    }
}
