using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.FMODAudioManager
{
    public class FMODAudioPreloader : MonoBehaviour
    {
        public List<FMODAudioScriptable> fmodAudioScriptables;
        public bool preloadOnAwake = false;

        public bool HasLoaded { get; private set; } = false;

        [NonSerialized] public float loadProgress;

        Coroutine preloadRoutine = null;

        void Awake()
        {
            if (preloadOnAwake)
            {
                preloadRoutine ??= StartCoroutine(PreloadEnumerator());
            }
        }

        public void PreloadAllAudio()
        {
            preloadRoutine ??= StartCoroutine(PreloadEnumerator());
        }

        IEnumerator PreloadEnumerator()
        {
            HasLoaded = false;
            loadProgress = 0f;

            if (!FMODAudioManager.Instance)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"FMODAudioManager instance is missing", FMODAudioManager.Instance);
#endif
                HasLoaded = true;
                loadProgress = 1f;
                preloadRoutine = null;

                yield break;
            }

            int index = 0;
            float bankLoadProgress = 0.4f;

            while (true)
            {
                if (!FMODAudioManager.Instance)
                {
                    HasLoaded = true;
                    loadProgress = 1f;
                    preloadRoutine = null;
                    yield break;
                }
                else if (FMODAudioManager.Instance.HaveAllBanksLoaded()) break;

                yield return null;
            }


            loadProgress = bankLoadProgress / 2f;

            while (true)
            {
                if (!FMODAudioManager.Instance)
                {
                    HasLoaded = true;
                    loadProgress = 1f;
                    preloadRoutine = null;
                    yield break;
                }
                else if (!FMODAudioManager.Instance.AnySampleDataLoading()) break;

                yield return null;
            }

            
            loadProgress = bankLoadProgress;

            foreach (var audioScriptable in fmodAudioScriptables)
            {
                if (FMODAudioManager.Instance)
                {
                    if (audioScriptable)
                    {
                        FMODAudioManager.Instance.PreloadData(audioScriptable);
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Unexpected termination of FMODAudioPreLoader Routine due to missing FMODAudioManager instance", FMODAudioManager.Instance);
#endif
                    HasLoaded = true;
                    loadProgress = 1f;
                    preloadRoutine = null;
                    yield break;
                }

                index++;

                loadProgress = bankLoadProgress + ((float)index / fmodAudioScriptables.Count) * (1 - bankLoadProgress);

                yield return null;
            }

            HasLoaded = true;
            loadProgress = 1f;
            preloadRoutine = null;
        }
    }

}
