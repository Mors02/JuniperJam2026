using UnityEngine;

namespace AbyssWorks.AnimatorSignal
{
    [RequireComponent(typeof(Animator))]
    public class RootMotionToCharacterController : MonoBehaviour
    {
        [SerializeField] CharacterController characterController;
        [SerializeField] bool applyRotation;
        [SerializeField] bool applyMovement;

        public float speed = 1f;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnAnimatorMove()
        {
            if (!characterController || !animator) return;

            if (applyMovement)
            {
                characterController.Move(animator.deltaPosition * speed);
            }

            if (applyRotation)
            {
                characterController.transform.rotation = characterController.transform.rotation * animator.deltaRotation;
            }
        }
    }
}


