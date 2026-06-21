using AbyssWorks.Misc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.ParasiteBehaviour.Misc
{
    [System.Serializable]
    public class ExampleKnuckleSandwich : ExampleAbility
    {
        [Header("Knuckle Sandwich")]

        public BlankMonobehavior abilityMonoHelper;
        public float sequenceDelay = 0.5f;

        [Header("Animation")]
        public Animator leftAnimator;
        public AnimationEnd leftAnimationEnd;
        public Animator rightAnimator;
        public AnimationEnd rightAnimationEnd;

        private Coroutine m_Routine = null;

        protected override void OnDeepCopy()
        {
            base.OnDeepCopy();

            leftAnimator = null; leftAnimationEnd = null;
            rightAnimator = null; rightAnimationEnd = null;

            abilityMonoHelper = null;
        }

        public override bool CanTrigger()
        {
            return base.CanTrigger() && leftAnimator && leftAnimationEnd 
                && rightAnimator && rightAnimationEnd && abilityMonoHelper;
        }

        public override void Trigger()
        {
            base.Trigger();

            if (abilityMonoHelper)
            {
                if (m_Routine == null)
                {
                    m_Routine = abilityMonoHelper.StartCoroutine(KnuckleEnumerator());
                }
            }
        }

        IEnumerator KnuckleEnumerator()
        {
            ResetAnimationSequence();

            float seqDelay = sequenceDelay;
            string currentAnim = GetCurrentAnimation();

            bool hasLeftEnded = false;
            bool hasRightEnded = false;

            bool inputPressed = false;

            IEnumerator SequenceWait()
            {
                float elapsedTime = 0f;

                while (elapsedTime < seqDelay)
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        inputPressed = true;
                        yield break;
                    }

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            leftAnimationEnd.SetAnimationEnd(() => { hasLeftEnded = true; });
            rightAnimationEnd.SetAnimationEnd(() => { hasRightEnded = true; });

            if (currentAnim == "SeqQuickPunch")
            {
                leftAnimator.CrossFade("LeftQuickPunch", 0.1f);
                while (!hasLeftEnded) yield return null;

                inputPressed = false;
                yield return SequenceWait();
                if (!inputPressed)
                {
                    leftAnimator.CrossFade("HandIdle", 0.1f);
                    rightAnimator.CrossFade("HandIdle", 0.1f);

                    m_Routine = null;
                    yield break;
                }

                rightAnimator.Play("RightQuickPunch");
                while (!hasRightEnded) yield return null;

                inputPressed = false;
                yield return SequenceWait();
                if (!inputPressed)
                {
                    leftAnimator.CrossFade("HandIdle", 0.1f);
                    rightAnimator.CrossFade("HandIdle", 0.1f);

                    m_Routine = null;
                    yield break;
                }
            }

            UpdateAnimationSequence();

            currentAnim = GetCurrentAnimation();


            if (currentAnim == "QuickPunch")
            {
                hasLeftEnded = false;
                hasRightEnded = false;

                leftAnimator.Play("LeftQuickPunchRep");
                rightAnimator.Play("RightQuickPunchRep");



                while (!hasLeftEnded || !hasRightEnded) yield return null;

                inputPressed = false;
                yield return SequenceWait();
                if (!inputPressed)
                {
                    leftAnimator.CrossFade("HandIdle", 0.1f);
                    rightAnimator.CrossFade("HandIdle", 0.1f);

                    m_Routine = null;
                    yield break;
                }
            }

            UpdateAnimationSequence();

            currentAnim = GetCurrentAnimation();

            if (currentAnim == "PunchSlam")
            {
                hasLeftEnded = false;
                hasRightEnded = false;

                leftAnimator.Play("LeftPunchSlam");
                rightAnimator.Play("RightPunchSlam");
                while (!hasLeftEnded || !hasRightEnded) yield return null;
            }

            leftAnimator.CrossFade("HandIdle", 0.1f);
            rightAnimator.CrossFade("HandIdle", 0.1f);

            m_Routine = null;
        }
    }

}

