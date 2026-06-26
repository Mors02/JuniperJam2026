using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.ParasiteBehaviour.Misc
{
    public enum POMobilityType
    {
        Stiff,
        Root_Motion,
        Multi_Movement,
        Forward_Movement,
        One_Direction
    }

    public enum POAbilityExecutionType
    {
        Duration,
        Animation,
        DurationAndAnimation,
        ExternalControl
    }

    [System.Serializable]
    public class ExampleAbility : ParasiteBehaviour
    {
        [Tooltip("Enable this game object during the attack")]
        public List<GameObject> activeObjects = new();

        [Tooltip("Set this for tracking abilities")]
        public Transform target;

        [Header("Execution Control")]
        [Tooltip("Define how the animation will execute and end")]
        public POAbilityExecutionType executionType = POAbilityExecutionType.Duration;

        [Tooltip("Assign necessary animation sequence or 1 if only one animation")]
        public List<string> characterAnimationSequence = new();

        [Tooltip("Use when the ability has no animation tied to it")]
        public float abilityDuration = 1f;

        [Header("Mobility control")]
        [Tooltip("Declare mobility constraint upon execution")]
        public POMobilityType attackMobility = POMobilityType.Stiff;

        [Tooltip("Set for mobility")]
        public float directionSpeed = 2f;

        public ExampleAbilityRequirement abilityRequirement;

        protected int currentAnimationSequence = 0;
        protected string currentAnimation = null;

        [Tooltip("Subscribe to callback when external control completes")]
        public Action onExternalComplete;

        public override void Initialize(GameObject go = null)
        {
            base.Initialize(go);

            UpdateAnimationSequence();
        }


        public virtual bool CheckDistance(float distance)
        {
            if (!abilityRequirement) return false;
            return distance > abilityRequirement.requiredDistance;
        }

        public virtual bool SatisfiesRequirements(float magic, float stamina, float hp)
        {
            return true;
        }

        public virtual bool CanTrigger()
        {
            return true;
        }

        public virtual void Trigger()
        {
            //Debug.Log("Supposed to do something");

            ActivateObjects();
        }

        public void TryTrigger()
        {
            if (!CanTrigger())
                return;

            Trigger();
        }

        public virtual void ExecutionCancel(bool forceCancel = false)
        {

        }


        public void UpdateAnimationSequence()
        {
            if (characterAnimationSequence.Count <= 0) return;

            //currentAnimationSequence = SHF.WrapIndex(currentAnimationSequence + 1, characterAnimationSequence.Count);
            
            if (currentAnimationSequence < characterAnimationSequence.Count)
            {
                currentAnimation = characterAnimationSequence[currentAnimationSequence];
                //Debug.Log("Next Animation " + currentAnimation);
            }
            else currentAnimation = null;

            currentAnimationSequence++;
        }

        public void ResetAnimationSequence()
        {
            if (characterAnimationSequence.Count <= 0) return;

            currentAnimationSequence = 0;
            UpdateAnimationSequence();
        }

        public string GetCurrentAnimation()
        {
            return currentAnimation;
        }

        public int GetSequenceCount()
        {
            return characterAnimationSequence.Count;
        }

        public void ActivateObjects()
        {
            foreach (var activeWeapon in activeObjects)
            {
                activeWeapon.SetActive(true);
            }
        }

        public void DeactivateObjects()
        {
            foreach (var activeWeapon in activeObjects)
            {
                activeWeapon.SetActive(false);
            }
        }

        public virtual void ActivateObjectsWithEffects()
        {
            ActivateObjects();
        }

        public virtual void DeactivateObjectsWithEffects()
        {
            DeactivateObjects();
        }
    }

}

