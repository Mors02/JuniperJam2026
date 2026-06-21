using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static AbyssWorks.ParasiteBehaviour.Misc.TestAbilityExecuter;

namespace AbyssWorks.ParasiteBehaviour.Misc
{
    public class TestAbilityExecuter : MonoBehaviour
    {
        [System.Serializable]
        public class AbilityPass
        {
            public string shelfTag;
            public string abilityName;
        }

        [Header("Reference Set")]
        public Transform target;
        public Transform spawnTransform;
        public GameObject bombPrefab;

        [Header("Misc")]
        public ParasiteBehaviourLibrary abilityLibrary;

        public List<AbilityPass> abilityPasses = new();
        public List<string> abilityPBNames = new();
        public TextMeshProUGUI textMeshProUGUI;

        private int currentIndex = 0;
        private ExampleAbility currentAbility;
        private List<(string abilityName, ExampleAbility ability)> allExternalAbilities = new();

        private void Start()
        {
            if (abilityPasses.Count > 0 && abilityLibrary)
            {
                var abilityPass = abilityPasses[currentIndex];
                if (textMeshProUGUI) textMeshProUGUI.text = abilityPass.abilityName;

                currentAbility = abilityLibrary.GetShelfParasiteB(abilityPass.shelfTag, abilityPass.abilityName) as ExampleAbility;
            }

            foreach (var abilityPass in abilityPasses)
            {
                if (abilityLibrary)
                {
                    var ability = abilityLibrary.GetShelfParasiteB(abilityPass.shelfTag, abilityPass.abilityName) as ExampleAbility;
                    if (ability != null)
                    {
                        ability.referenceDict["spawnTransform"] = spawnTransform;
                        ability.referenceDict["bombPrefab"] = bombPrefab;
                        ability.referenceDict["target"] = target;

                        allExternalAbilities.Add((abilityPass.abilityName, ability));
                    }
                }
            }

            foreach (var abilityName in abilityPBNames)
            {
                if (abilityLibrary)
                {
                    var ability = abilityLibrary.GetShelfParasiteB(null, abilityName) as ExampleAbility;
                    if (ability != null)
                    {
                        ability.referenceDict["spawnTransform"] = spawnTransform;
                        ability.referenceDict["bombPrefab"] = bombPrefab;
                        ability.referenceDict["target"] = target;

                        allExternalAbilities.Add((abilityName, ability));
                    }
                }
            }

            foreach (var externalAbility in allExternalAbilities)
            {
                externalAbility.ability.ExternalStart();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (currentAbility != null)
                {
                    currentAbility.TryTrigger();
                }
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                currentIndex++;
                currentIndex %= allExternalAbilities.Count;

                currentAbility = allExternalAbilities[currentIndex].ability;

                if (textMeshProUGUI) textMeshProUGUI.text = allExternalAbilities[currentIndex].abilityName;
            }

            foreach (var externalAbility in allExternalAbilities)
            {
                externalAbility.ability.ExternalUpdate();
            }
        }

        public string GetCurrentAbilityName()
        {
            return allExternalAbilities[currentIndex].abilityName;
        }
    }

}

