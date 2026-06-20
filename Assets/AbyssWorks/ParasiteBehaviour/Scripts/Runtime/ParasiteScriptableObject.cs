using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.ParasiteBehaviour
{
    [CreateAssetMenu(fileName = "New Parasite Object", menuName = "Scriptable Objects/ParasiteObject")]
    public class ParasiteScriptableObject : ScriptableObject
    {
        [SerializeReference]
        public ParasiteBehaviour parasiteBehaviour;
    }
}

