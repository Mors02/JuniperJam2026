using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.AnimatorSignal
{
    public class AnimatorESLManager : MonoBehaviour
    {
        private static AnimatorESLManager _instance;
        public static AnimatorESLManager Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            if (_instance == null)
            {
                GameObject go = new("AnimatorSELManager");
                _instance = go.AddComponent<AnimatorESLManager>();
                DontDestroyOnLoad(go);
            }
        }

        private Dictionary<GameObject, AnimatorEventStateListener> animatorESLDict = new();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;

                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void AddListener(GameObject go, AnimatorEventStateListener listener)
        {
            if (!go || !listener) return;

            if (!HasListener(go))
            {
                animatorESLDict[go] = listener;
            }
        }

        public void RemoveListener(GameObject go)
        {
            if (HasListener(go))
            {
                animatorESLDict.Remove(go);
            }
        }

        public bool HasListener(GameObject go)
        {
            return go && animatorESLDict.ContainsKey(go);
        }

        public AnimatorEventStateListener GetListener(GameObject go)
        {
            animatorESLDict.TryGetValue(go, out AnimatorEventStateListener listener);
            return listener;
        }
    }

}

