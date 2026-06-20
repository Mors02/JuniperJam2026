using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AbyssWorks.Misc
{
    public class SimplePlayerController : MonoBehaviour
    {
        public float maxSpeed = 10.0f;
        public float angularVelocity = 10.0f;
        //private Rigidbody rb;
        public float jumpForce = 30.0f;
        public Transform groundCheck;
        public float groundedRadius = 0.1f;
        public LayerMask groundMask;

        private Camera gameCam;
        private CharacterController characterController;
        private CustomForce customForce;

        private float rotateSpeed;
        
        // Start is called before the first frame update
        void Start()
        {
            //rb = GetComponent<Rigidbody>();
            characterController = GetComponent<CharacterController>();
            customForce = GetComponent<CustomForce>();
            gameCam = Camera.main;
        }
        void Update()
        {
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1)) && IsGrounded)
            {
                customForce.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }

            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(0, gameCam.transform.eulerAngles.y, 0), angularVelocity * Time.deltaTime);


            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 newVelocity = maxSpeed * (vertical * transform.forward + horizontal * transform.right);

            characterController.Move(newVelocity * Time.deltaTime);
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            customForce.SimulateForces(IsGrounded);

            characterController.Move(customForce.Velocity * Time.fixedDeltaTime);
        }

        public bool IsGrounded => Physics.CheckSphere(groundCheck.position, groundedRadius, groundMask);

        private void OnDrawGizmosSelected()
        {
            if (!groundCheck) return;

            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (IsGrounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            Gizmos.DrawSphere(groundCheck.position, groundedRadius);
        }
    }
}


