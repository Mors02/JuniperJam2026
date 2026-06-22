using UnityEngine;

namespace AbyssWorks.AnimatorSignal
{
    public class AnimatorEventStateTransmitter : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (AnimatorESLManager.Instance)
            {
                var listener = AnimatorESLManager.Instance.GetListener(animator.gameObject);
                if (listener) listener.OnAnimatorStateEnter(animator, stateInfo, layerIndex);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (AnimatorESLManager.Instance)
            {
                var listener = AnimatorESLManager.Instance.GetListener(animator.gameObject);
                if (listener) listener.OnAnimatorStateUpdate(animator, stateInfo, layerIndex);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (AnimatorESLManager.Instance)
            {
                var listener = AnimatorESLManager.Instance.GetListener(animator.gameObject);
                if (listener) listener.OnAnimatorStateExit(animator, stateInfo, layerIndex);
            }
        }
    }

}

