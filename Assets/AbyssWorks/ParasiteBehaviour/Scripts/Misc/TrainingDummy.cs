using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.ParasiteBehaviour.Misc
{
    public class TrainingDummy : MonoBehaviour
    {
        [Header("Reference Set")]
        public List<GameObject> activeObjects = new();
        public Transform target;
        public Transform spawnTransform;
        public GameObject bombPrefab;

        public ParasiteBehaviourLibrary abilityLibrary;

        public ParasiteBehaviourLibrary playerAbilityLibrary;
        public TestAbilityExecuter playerAbilityExecuter;
        

        public float rotationSpeed = 30f;

        private Quaternion initialRot;
        private Vector3 initialPos;

        private BlankMonobehavior abilityMonoHelper;

        private string abilityToCopy;

        private void Awake()
        {
            initialRot = transform.rotation;
            initialPos = transform.position;

            abilityMonoHelper = GetComponent<BlankMonobehavior>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsClose(transform.rotation, initialRot))
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, initialRot, rotationSpeed * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (playerAbilityLibrary && abilityLibrary && playerAbilityExecuter)
                {
                    abilityToCopy = playerAbilityExecuter.GetCurrentAbilityName();

                    var ability = playerAbilityLibrary.GetAnyParasiteB(abilityToCopy) as ExampleAbility;

                    if (ability != null && !abilityLibrary.HasAnyParasiteB(abilityToCopy))
                    {
                        ability = abilityLibrary.AddShelfParasiteB(null, abilityToCopy, ability, true, true) as ExampleAbility;

                        ability.referenceDict["spawnTransform"] = spawnTransform;
                        ability.referenceDict["bombPrefab"] = bombPrefab;
                        ability.referenceDict["target"] = target;
                        ability.referenceDict[nameof(abilityMonoHelper)] = abilityMonoHelper;
                        ability.referenceDict[nameof(activeObjects)] = activeObjects;

                        ability.ExternalStart();
                    }

                    Debug.Log($"Copy check - ability: {ability == null}");
                }
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                if (abilityLibrary.HasAnyParasiteB(abilityToCopy))
                {
                    var ability = abilityLibrary.GetAnyParasiteB(abilityToCopy) as ExampleAbility;
                    if (ability != null) ability.TryTrigger();
                }
            }

            transform.position = initialPos;
        }

        bool IsClose(Quaternion a, Quaternion b, float fac = 1f)
        {
            float dot = Mathf.Abs(Quaternion.Dot(a, b));
            return dot > Mathf.Cos(fac * Mathf.Deg2Rad * 0.5f);
        }
    }

}

