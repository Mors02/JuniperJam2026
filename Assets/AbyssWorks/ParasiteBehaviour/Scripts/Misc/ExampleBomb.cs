using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.ParasiteBehaviour.Misc
{
    public class ExampleBomb : MonoBehaviour
    {
        public GameObject explosionParticlePrefab;
        public Vector3 targetPos;
        public float speed = 5f;
        public float autoDestroyTimer = 10f;
        public float delayTime = 0f;

        [NonSerialized] public GameObject owner;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            StartCoroutine(DelayEnumerator());
        }

        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log(other.gameObject + " " + owner);
            if (other.gameObject == owner) return;

            Destroy(gameObject);

            if (explosionParticlePrefab) Instantiate(explosionParticlePrefab, transform.position, transform.rotation);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log(collision.gameObject + " " + owner);
            if (collision.gameObject == owner) return;

            Destroy(gameObject);

            if (explosionParticlePrefab) Instantiate(explosionParticlePrefab, transform.position, transform.rotation);
        }

        IEnumerator DelayEnumerator()
        {
            float duration = delayTime;
            yield return new WaitForSeconds(duration);

            Vector3 diff = targetPos - transform.position;
            transform.forward = diff.normalized;

            rb.AddForce(transform.forward * speed, ForceMode.Impulse);

            Destroy(gameObject, autoDestroyTimer);
        }
    }

}
