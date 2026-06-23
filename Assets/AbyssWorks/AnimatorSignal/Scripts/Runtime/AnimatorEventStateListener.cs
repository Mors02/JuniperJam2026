using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AbyssWorks.AnimatorSignal
{
    public enum AESLMode
    {
        Enter,
        Update,
        Exit
    }

    [System.Serializable]
    public struct AnimatorUnityEvent
    {
        public string name;
        [Range(0f, 1f)] public float animationTime;
        public UnityEvent unityEvent;
    }

    public struct AnimatorAction
    {
        public string name;
        [Range(0f, 1f)] public float animationTime;
        public Action action;
    }

    [RequireComponent(typeof(Animator))]
    public class AnimatorEventStateListener : MonoBehaviour
    {
        [System.Serializable]
        public class AnimatorEventPackage
        {
            public string stateName;

            [Tooltip("Called on Enter \n Values are sent into a dictionary when run.")]
            public List<AnimatorUnityEvent> ent_animatorUnityEvents;
            [Tooltip("Called on Update \n Values are sent into a dictionary when run.")]
            public List<AnimatorUnityEvent> upd_animatorUnityEvents;
            [Tooltip("Called on Exit \n Values are sent into a dictionary when run.")]
            public List<AnimatorUnityEvent> ext_animatorUnityEvents;

            public Dictionary<int, UnityEvent> ent_animatorUnityEventDict = new();
            public Dictionary<ushort, Dictionary<int, UnityEvent>> upd_animatorUnityEventDict = new();
            public Dictionary<int, UnityEvent> ext_animatorUnityEventDict = new();

            public Dictionary<int, Action> ent_animatorActionDict = new();
            public Dictionary<ushort, Dictionary<int, Action>> upd_animatorActionDict = new();
            public Dictionary<int, Action> ext_animatorActionDict = new();
        }

        [Header("Value Reader")]
        [SerializeField, Min(1)] private int maxFrame = 1;
        [SerializeField, Min(0)] private int clipFrame;
        [SerializeField, Min(0)] private float clipFps;
        [SerializeField] private string animationTimeReader;

        [Tooltip("Values are sent into a dictionary when run. \n Duplicate state names are not allowed")]
        [SerializeField] private List<AnimatorEventPackage> animatorEventPackages = new();
        private Dictionary<int, AnimatorEventPackage> animatorEventPackageDict = new();

        private Dictionary<int, ushort> animationTimeDict = new();

        private void OnValidate()
        {
            animationTimeReader = Mathf.Clamp01((float)clipFrame / maxFrame).ToString("F3");
        }

        private void Awake()
        {
            if (AnimatorESLManager.Instance)
            {
                AnimatorESLManager.Instance.AddListener(gameObject, this);
            }

            foreach (var animatorPackage in animatorEventPackages)
            {
                foreach (var animatorUnityEvent in animatorPackage.ent_animatorUnityEvents)
                {
                    int hashedEventName = Animator.StringToHash(animatorUnityEvent.name);
                    animatorPackage.ent_animatorUnityEventDict[hashedEventName] = animatorUnityEvent.unityEvent;
                }

                foreach (var animatorUnityEvent in animatorPackage.ext_animatorUnityEvents)
                {
                    int hashedEventName = Animator.StringToHash(animatorUnityEvent.name);
                    animatorPackage.ext_animatorUnityEventDict[hashedEventName] = animatorUnityEvent.unityEvent;
                }

                foreach (var animatorUnityEvent in animatorPackage.upd_animatorUnityEvents)
                {
                    int hashedEventName = Animator.StringToHash(animatorUnityEvent.name);
                    ushort normalizedInt = NormTimeToInt(animatorUnityEvent.animationTime);
                    if (!animatorPackage.upd_animatorUnityEventDict.TryGetValue(normalizedInt, out var stateEventDict))
                        stateEventDict = animatorPackage.upd_animatorUnityEventDict[normalizedInt] = new();
                    stateEventDict[hashedEventName] = animatorUnityEvent.unityEvent;
                }

                int hashedStateName = Animator.StringToHash(animatorPackage.stateName);
                animatorEventPackageDict[hashedStateName] = animatorPackage;
            }
        }

        public void OnAnimatorStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animationTimeDict[stateInfo.shortNameHash] = 0;

            if (animatorEventPackageDict.TryGetValue(stateInfo.shortNameHash, out var animatorEventPackage))
            {
                foreach (var (_, unityEvent) in animatorEventPackage.ent_animatorUnityEventDict)
                {
                    unityEvent?.Invoke();
                }

                foreach (var (_, action) in animatorEventPackage.ent_animatorActionDict)
                {
                    action?.Invoke();
                }
            }
        }

        public void OnAnimatorStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animatorEventPackageDict.TryGetValue(stateInfo.shortNameHash, out var animatorEventPackage))
            {
                float normalizedTime = stateInfo.normalizedTime % 1f;
                float step = normalizedTime * stateInfo.length;
                ushort currentTime = (stateInfo.length > 0f) ? NormTimeToInt(step / stateInfo.length) : (ushort)0;

                var prevTime = animationTimeDict[stateInfo.shortNameHash];

                foreach (var (eventTime, stateEventDict) in animatorEventPackage.upd_animatorUnityEventDict)
                {
                    int delta = currentTime - prevTime;
                    bool checkThreshold = delta >= 0 ? prevTime < eventTime && eventTime <= currentTime
                                : currentTime <= eventTime && eventTime < prevTime;

                    if (checkThreshold)
                    {
                        foreach (var (_, unityEvent) in stateEventDict)
                        {
                            unityEvent?.Invoke();
                        }
                    }
                }

                foreach (var (eventTime, stateEventDict) in animatorEventPackage.upd_animatorActionDict)
                {
                    int delta = currentTime - prevTime;
                    bool checkThreshold = delta >= 0 ? prevTime < eventTime && eventTime <= currentTime
                                : currentTime <= eventTime && eventTime < prevTime;

                    if (checkThreshold)
                    {
                        foreach (var (_, action) in stateEventDict)
                        {
                            action?.Invoke();
                        }
                    }
                }

                animationTimeDict[stateInfo.shortNameHash] = currentTime;
            }
        }

        public void OnAnimatorStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animatorEventPackageDict.TryGetValue(stateInfo.shortNameHash, out var animatorEventPackage))
            {
                foreach (var (_, unityEvent) in animatorEventPackage.ext_animatorUnityEventDict)
                {
                    unityEvent?.Invoke();
                }

                foreach (var (_, action) in animatorEventPackage.ext_animatorActionDict)
                {
                    action?.Invoke();
                }
            }
        }

        public ushort NormTimeToInt(float t)
        {
            t = Mathf.Clamp(t, 0f, 0.999f);
            return (ushort)Mathf.RoundToInt(t * 1000f);
        }

        public void ForceStateTimeReset(string stateName)
        {
            int hashedState = Animator.StringToHash(stateName);
            if (animationTimeDict.ContainsKey(hashedState))
            {
                animationTimeDict[hashedState] = 0;
            }
        }

        public void AddUnityEvent(string stateName, AnimatorUnityEvent animatorUnityEvent,
            AESLMode aeslMode = AESLMode.Update)
        {
            int hashedState = Animator.StringToHash(stateName);
            if (!animatorEventPackageDict.ContainsKey(hashedState))
            {
                AnimatorEventPackage newPackage = new();
                newPackage.stateName = stateName;
                animatorEventPackageDict[hashedState] = newPackage;
            }

            int hashedEvent = Animator.StringToHash(animatorUnityEvent.name);
            var animatorEventPackage = animatorEventPackageDict[hashedState];

            if (aeslMode != AESLMode.Update)
            {
                var quickEventDict = (aeslMode == AESLMode.Enter) ? animatorEventPackage.ent_animatorUnityEventDict
                    : animatorEventPackage.ext_animatorUnityEventDict;
                quickEventDict[hashedEvent] = animatorUnityEvent.unityEvent;
                return;
            }

            var animatorUnityEventDict = animatorEventPackage.upd_animatorUnityEventDict;
            ushort normalizedInt = NormTimeToInt(animatorUnityEvent.animationTime);
            animatorUnityEventDict.TryGetValue(normalizedInt, out var stateEventDict);
            stateEventDict ??= animatorUnityEventDict[normalizedInt] = new();

            stateEventDict[hashedEvent] = animatorUnityEvent.unityEvent;
        }

        public void AddAction(string stateName, AnimatorAction animatorAction,
            AESLMode aeslMode = AESLMode.Update)
        {
            int hashedState = Animator.StringToHash(stateName);
            if (!animatorEventPackageDict.ContainsKey(hashedState))
            {
                AnimatorEventPackage newPackage = new();
                newPackage.stateName = stateName;
                animatorEventPackageDict[hashedState] = newPackage;
            }

            int hashedEvent = Animator.StringToHash(animatorAction.name);
            var animatorEventPackage = animatorEventPackageDict[hashedState];

            if (aeslMode != AESLMode.Update)
            {
                var quickEventDict = (aeslMode == AESLMode.Enter) ? animatorEventPackage.ent_animatorActionDict
                    : animatorEventPackage.ext_animatorActionDict;
                quickEventDict[hashedEvent] = animatorAction.action;
                return;
            }

            var animatorActionDict = animatorEventPackage.upd_animatorActionDict;
            ushort normalizedInt = NormTimeToInt(animatorAction.animationTime);
            animatorActionDict.TryGetValue(normalizedInt, out var stateEventDict);
            stateEventDict ??= animatorActionDict[normalizedInt] = new();

            stateEventDict[hashedEvent] = animatorAction.action;
        }

        public void RemoveUnityEvent(string stateName, float animationTime, string eventName
            , AESLMode aeslMode = AESLMode.Update)
        {
            animationTime = Mathf.Clamp(animationTime, 0f, 0.999f);

            int hashedState = Animator.StringToHash(stateName);
            if (animatorEventPackageDict.TryGetValue(hashedState, out var animatorEventPackage))
            {
                int hashedEvent = Animator.StringToHash(eventName);

                if (aeslMode != AESLMode.Update)
                {
                    var quickEventDict = (aeslMode == AESLMode.Enter) ? animatorEventPackage.ent_animatorUnityEventDict
                        : animatorEventPackage.ext_animatorUnityEventDict;
                    if (quickEventDict.ContainsKey(hashedEvent)) quickEventDict.Remove(hashedEvent);
                    return;
                }

                var animatorUnityEventDict = animatorEventPackage.upd_animatorUnityEventDict;
                ushort normalizedInt = NormTimeToInt(animationTime);
                if (animatorUnityEventDict.TryGetValue(normalizedInt, out var stateEventDict))
                {
                    stateEventDict.Remove(hashedEvent);
                }
            }
        }

        public void RemoveAction(string stateName, float animationTime, string eventName,
            AESLMode aeslMode = AESLMode.Update)
        {
            animationTime = Mathf.Clamp(animationTime, 0f, 0.999f);

            int hashedState = Animator.StringToHash(stateName);
            if (animatorEventPackageDict.TryGetValue(hashedState, out var animatorEventPackage))
            {
                int hashedEvent = Animator.StringToHash(eventName);

                if (aeslMode != AESLMode.Update)
                {
                    var quickEventDict = (aeslMode == AESLMode.Enter) ? animatorEventPackage.ent_animatorActionDict
                        : animatorEventPackage.ext_animatorActionDict;
                    if (quickEventDict.ContainsKey(hashedEvent)) quickEventDict.Remove(hashedEvent);
                    return;
                }

                var animatorActionDict = animatorEventPackage.upd_animatorActionDict;

                ushort normalizedInt = NormTimeToInt(animationTime);
                if (animatorActionDict.TryGetValue(normalizedInt, out var stateEventDict))
                {
                    if (stateEventDict.ContainsKey(hashedEvent)) stateEventDict.Remove(hashedEvent);
                }
            }
        }

        public UnityEvent GetUnityEvent(string stateName, float animationTime, string eventName
            , AESLMode aeslMode = AESLMode.Update)
        {
            animationTime = Mathf.Clamp(animationTime, 0f, 0.999f);

            int hashedState = Animator.StringToHash(stateName);
            if (animatorEventPackageDict.TryGetValue(hashedState, out var animatorEventPackage))
            {
                int hashedEvent = Animator.StringToHash(eventName);

                if (aeslMode != AESLMode.Update)
                {
                    var quickEventDict = (aeslMode == AESLMode.Enter) ? animatorEventPackage.ent_animatorUnityEventDict
                        : animatorEventPackage.ext_animatorUnityEventDict;
                    quickEventDict.TryGetValue(hashedEvent, out var unityEvent);
                    return unityEvent;
                }

                var animatorUnityEventDict = animatorEventPackage.upd_animatorUnityEventDict;

                ushort normalizedInt = NormTimeToInt(animationTime);
                if (animatorUnityEventDict.TryGetValue(normalizedInt, out var stateEventDict))
                {
                    stateEventDict.TryGetValue(hashedEvent, out var unityEvent);
                    return unityEvent;
                }
            }

            return null;
        }

        public Action GetAction(string stateName, float animationTime, string eventName,
            AESLMode aeslMode = AESLMode.Update)
        {
            animationTime = Mathf.Clamp(animationTime, 0f, 0.999f);

            int hashedState = Animator.StringToHash(stateName);
            if (animatorEventPackageDict.TryGetValue(hashedState, out var animatorEventPackage))
            {
                int hashedEvent = Animator.StringToHash(eventName);

                if (aeslMode != AESLMode.Update)
                {
                    var quickEventDict = (aeslMode == AESLMode.Enter) ? animatorEventPackage.ent_animatorActionDict
                        : animatorEventPackage.ext_animatorActionDict;
                    quickEventDict.TryGetValue(hashedEvent, out var action);
                    return action;
                }

                var animatorActionDict = animatorEventPackage.upd_animatorActionDict;

                ushort normalizedInt = NormTimeToInt(animationTime);
                if (animatorActionDict.TryGetValue(normalizedInt, out var stateEventDict))
                {
                    stateEventDict.TryGetValue(hashedEvent, out var action);
                    return action;
                }
            }

            return null;
        }

        public bool HasUnityEvent(string stateName, float animationTime, string eventName
            , AESLMode aeslMode = AESLMode.Update)
        {
            animationTime = Mathf.Clamp(animationTime, 0f, 0.999f);

            int hashedState = Animator.StringToHash(stateName);
            if (animatorEventPackageDict.TryGetValue(hashedState, out var animatorEventPackage))
            {
                int hashedEvent = Animator.StringToHash(eventName);

                if (aeslMode != AESLMode.Update)
                {
                    var quickEventDict = (aeslMode == AESLMode.Enter) ? animatorEventPackage.ent_animatorUnityEventDict
                        : animatorEventPackage.ext_animatorUnityEventDict;
                    return quickEventDict.ContainsKey(hashedEvent);
                }

                var animatorUnityEventDict = animatorEventPackage.upd_animatorUnityEventDict;

                ushort normalizedInt = NormTimeToInt(animationTime);
                if (animatorUnityEventDict.TryGetValue(normalizedInt, out var stateEventDict))
                {
                    return stateEventDict.ContainsKey(hashedEvent);
                }
            }

            return false;
        }

        public bool HasAction(string stateName, float animationTime, string eventName
            , AESLMode aeslMode = AESLMode.Update)
        {
            animationTime = Mathf.Clamp(animationTime, 0f, 0.999f);

            int hashedState = Animator.StringToHash(stateName);
            if (animatorEventPackageDict.TryGetValue(hashedState, out var animatorEventPackage))
            {
                int hashedEvent = Animator.StringToHash(eventName);

                if (aeslMode != AESLMode.Update)
                {
                    var quickEventDict = (aeslMode == AESLMode.Enter) ? animatorEventPackage.ent_animatorActionDict
                        : animatorEventPackage.ext_animatorActionDict;
                    return quickEventDict.ContainsKey(hashedEvent);
                }
                var animatorActionDict = animatorEventPackage.upd_animatorActionDict;

                ushort normalizedInt = NormTimeToInt(animationTime);
                if (animatorActionDict.TryGetValue(normalizedInt, out var stateEventDict))
                {
                    return stateEventDict.ContainsKey(hashedEvent);
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            if (AnimatorESLManager.Instance && AnimatorESLManager.Instance.HasListener(gameObject))
            {
                AnimatorESLManager.Instance.RemoveListener(gameObject);
            }
        }
    }
}

