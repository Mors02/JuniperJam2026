using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.Misc
{
    public class CustomForce : MonoBehaviour
    {
        [Header("Physics Properties")]
        public float mass = 1f;

        [Header("Forces")]
        public float gravityScale = 1f;
        public float gravity = -9.8f;
        public float groundFriction = 8f;
        public float airDrag = 0.1f;
        public float staticGravity = -2f;
        public float maxTerminalVelocity = -40f;

        public Vector3 velocity = Vector3.zero;
        private Vector3 accumulatedForces = Vector3.zero;

        public Vector3 Velocity => velocity;

        public void SimulateForces(bool isGrounded)
        {
            // Gravity
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = staticGravity * gravityScale;
                ApplyFriction();
            }
            else
            {
                if (velocity.y > maxTerminalVelocity)
                    velocity.y += gravity * gravityScale * Time.deltaTime;

                ApplyDrag();
            }

            // Apply accumulated forces
            velocity += accumulatedForces * Time.deltaTime;

            // Clear forces after applying
            accumulatedForces = Vector3.zero;
        }


        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            switch (forceMode)
            {
                case ForceMode.Force:
                    accumulatedForces += force / mass;
                    break;
                case ForceMode.Acceleration:
                    accumulatedForces += force;
                    break;
                case ForceMode.Impulse:
                    velocity += force / mass;
                    break;
                case ForceMode.VelocityChange:
                    velocity += force;
                    break;
                default:
                    break;
            }
        }

        public void AddExplosionForce(
            float explosionForce,
            Vector3 explosionPosition,
            float explosionRadius,
            float upwardsModifier = 0f,
            ForceMode forceMode = ForceMode.Impulse)
        {
            if (explosionRadius <= 0f || Mathf.Approximately(explosionForce, 0f))
                return;

            // Direction from explosion to object
            Vector3 dir = (transform.position - (explosionPosition - Vector3.up * upwardsModifier));

            float distance = dir.magnitude;

            if (distance > explosionRadius)
                return; // Outside radius, no effect

            if (dir.sqrMagnitude < Mathf.Epsilon)
                dir = Vector3.up;
            else
                dir.Normalize();

            // Falloff: closer = stronger force
            float falloff = Mathf.Clamp01(1f - distance / explosionRadius);

            // Final force vector
            Vector3 force = explosionForce * falloff * dir;

            // Apply using your existing system
            AddForce(force, forceMode);
        }

        public void ResetForces() { velocity = Vector3.zero; accumulatedForces = Vector3.zero; }

        private void ApplyFriction()
        {
            velocity.x -= velocity.x * groundFriction * Time.deltaTime;
            velocity.z -= velocity.z * groundFriction * Time.deltaTime;

            if (Mathf.Abs(velocity.x) < 0.01f) velocity.x = 0;
            if (Mathf.Abs(velocity.z) < 0.01f) velocity.z = 0;
        }

        private void ApplyDrag()
        {
            velocity.x -= velocity.x * airDrag * Time.deltaTime;
            velocity.z -= velocity.z * airDrag * Time.deltaTime;

            if (Mathf.Abs(velocity.x) < 0.01f) velocity.x = 0;
            if (Mathf.Abs(velocity.z) < 0.01f) velocity.z = 0;
        }
    }
}

