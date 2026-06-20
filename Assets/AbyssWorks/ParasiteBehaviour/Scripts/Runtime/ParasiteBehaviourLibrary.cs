using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.ParasiteBehaviour
{
    public class ParasiteBehaviourLibrary : MonoBehaviour
    {
        [System.Serializable]
        public class ParasiteBHolder
        {
            public string name;
            [SerializeReference] public ParasiteBehaviour parasiteBehaviour;
        }

        [System.Serializable]
        public class ParasiteBShelf
        {
            public string tag;
            public List<ParasiteBHolder> parasiteBHolders = new();
        }

        [System.Serializable]
        public class ParasiteSOHolder
        {
            public string name;
            public ParasiteScriptableObject parasiteSO;
        }

        public GameObject abilityOwner;

        [Header("Abilities")]
        [SerializeField] private List<ParasiteBShelf> abilityShelfs = new();
        [SerializeField] private List<ParasiteSOHolder> parasiteSOHolders = new();

        private Dictionary<string, Dictionary<string, ParasiteBehaviour>> parasiteBShelfDict = new();
        
        bool hasInitialized = false;

        const string GENERAL_SHELF = "General";

        private void Initialize()
        {
            hasInitialized = true;

            Dictionary<string, ParasiteBehaviour> parasiteBDict = new();

            foreach (var abilityShelf in abilityShelfs)
            {
                parasiteBDict = new();
                foreach (var abilityHolder in abilityShelf.parasiteBHolders)
                {
                    if (abilityHolder.parasiteBehaviour != null) abilityHolder.parasiteBehaviour.Initialize(abilityOwner);
                    parasiteBDict.Add(abilityHolder.name, abilityHolder.parasiteBehaviour);
                }
                parasiteBShelfDict.Add(abilityShelf.tag, parasiteBDict);
            }

            parasiteBDict = parasiteBShelfDict.ContainsKey(GENERAL_SHELF) ?
                    parasiteBShelfDict[GENERAL_SHELF] : new();

            foreach (var parasiteSOHolder in parasiteSOHolders)
            {
                if (parasiteSOHolder != null && parasiteSOHolder.parasiteSO)
                {
                    parasiteBDict.Add(parasiteSOHolder.name, 
                        ParasiteBehaviour.DeepCopyJson(parasiteSOHolder.parasiteSO.parasiteBehaviour));
                }
            }

            parasiteBShelfDict.Add(GENERAL_SHELF, parasiteBDict);
        }

        private void Awake()
        {
            if (!hasInitialized) Initialize();
        }

        public ParasiteBehaviour AddShelfParasiteB(string shelfTag, string parasiteBName, ParasiteBehaviour parasiteBehaviour, bool initialise = false, bool makeCopy = false)
        {
            if (!hasInitialized) Initialize();

            if (parasiteBehaviour == null) return null;

            if (string.IsNullOrWhiteSpace(shelfTag))
            {
                return AddShelfParasiteB(GENERAL_SHELF, parasiteBName, parasiteBehaviour, initialise, makeCopy);
            }

            if (!parasiteBShelfDict.ContainsKey(shelfTag))
            {
                var pb = makeCopy ? ParasiteBehaviour.DeepCopyJson(parasiteBehaviour) : parasiteBehaviour;

                if (initialise) pb.Initialize(abilityOwner);

                Dictionary<string, ParasiteBehaviour> newParasiteBDict = new()
                {
                    { parasiteBName, pb }
                };
                parasiteBShelfDict.Add(shelfTag, newParasiteBDict);

                return pb;
            }
            
            var parasiteBDict = parasiteBShelfDict[shelfTag];

            var parasiteB = makeCopy ? ParasiteBehaviour.DeepCopyJson(parasiteBehaviour) : parasiteBehaviour;

            if (initialise) parasiteB.Initialize(abilityOwner);

            parasiteBDict[parasiteBName] = parasiteB;

            return parasiteB;
        }

        public void RemoveShelfParasiteB(string shelfTag, string parasiteBName)
        {
            if (!hasInitialized) Initialize();

            if (string.IsNullOrWhiteSpace(shelfTag))
            {
                RemoveShelfParasiteB(GENERAL_SHELF, parasiteBName);
                return;
            }

            if (parasiteBShelfDict.TryGetValue(shelfTag, out var parasiteBDict))
            {
                if (parasiteBDict.ContainsKey(parasiteBName))
                {
                    parasiteBDict.Remove(parasiteBName);
                }
            }
        }

        public bool HasShelfParasiteB(string shelfTag, string parasiteBName)
        {
            if (string.IsNullOrWhiteSpace(shelfTag))
            {
                return HasShelfParasiteB(GENERAL_SHELF, parasiteBName);
            }

            if (parasiteBShelfDict.TryGetValue(shelfTag, out var parasiteBDict))
            {
                return parasiteBDict.ContainsKey(parasiteBName);
            }

            return false;
        }

        public bool HasAnyParasiteB(string parasiteBName)
        {
            if (!hasInitialized) Initialize();

            foreach (var (key, dict) in parasiteBShelfDict)
            {
                if (dict.ContainsKey(parasiteBName))
                    return true;
            }
            return false;
        }

        public ParasiteBehaviour GetShelfParasiteB(string shelfTag, string parasiteBName)
        {
            if (!hasInitialized) Initialize();

            if (string.IsNullOrWhiteSpace(shelfTag))
            {
                return GetShelfParasiteB(GENERAL_SHELF, parasiteBName);
            }

            if (parasiteBShelfDict.TryGetValue(shelfTag, out var parasiteBDict))
            {
                parasiteBDict.TryGetValue(parasiteBName, out ParasiteBehaviour parasiteBehaviour);
                return parasiteBehaviour;
            }

            return null;
        }

        public ParasiteBehaviour GetAnyParasiteB(string parasiteBName)
        {
            if (!hasInitialized) Initialize();

            foreach (var (key, dict) in parasiteBShelfDict)
            {
                if (dict.TryGetValue(parasiteBName, out var parasiteBehaviour))
                    return parasiteBehaviour;
            }
            return null;
        }
    }

}
