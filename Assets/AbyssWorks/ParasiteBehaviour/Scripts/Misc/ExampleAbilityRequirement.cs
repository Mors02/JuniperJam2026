using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.ParasiteBehaviour.Misc
{
    [CreateAssetMenu(fileName = "New Ex Ability Requirement", menuName = "Scriptable Objects/ExampleAbilityRequirement")]
    public class ExampleAbilityRequirement : ScriptableObject
    {
        public float requiredDistance = 1.5f;
        public float requiredHp = 5f;
        public float requiredMana = 10f;
        public float requiredStamina = 5f;
    }
}


