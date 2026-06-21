using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.FMODAudioManager
{
    [CreateAssetMenu(fileName = "New FMOD Audio Event", menuName = "Scriptable Objects/FMODAudio")]
    public class FMODAudioScriptable : ScriptableObject
    {
        //ID assigned by the AudioManager on first use. Used for runtime lookup and reuse.
        [HideInInspector] public Guid runtimeID = Guid.Empty;

        public EventReference audioEventReference;

        [Min(0f)] public float volumeScale = 1f;
        public float pitchScale = 1f;

        [Min(0f)] public float attenuationMin = 1f;
        [Min(0f)] public float attenuationMax = 20f;

        private static HashSet<FMODAudioScriptable> initializedAssets = new();

        private void OnEnable()
        {
            if (!initializedAssets.Contains(this))
            {
                initializedAssets.Add(this);
                runtimeID = Guid.Empty;
            }
        }

        private void OnDestroy()
        {
            if (FMODAudioManager.Instance) FMODAudioManager.Instance.Release(this);
        }

        public override string ToString()
        {
            string path = "EDITOR ONLY";
#if UNITY_EDITOR
            path = audioEventReference.Path; // Only works in Editor
#endif

            return $"[FMODAudioScriptable] " +
                   $"Name: {name}, " +
                   $"RuntimeID: {runtimeID}, " +
                   $"Event: {path}, " +
                   $"Volume: {volumeScale}, " +
                   $"Pitch: {pitchScale}, " +
                   $"Attenuation: {attenuationMin}-{attenuationMax}, ";
        }
    }

}

