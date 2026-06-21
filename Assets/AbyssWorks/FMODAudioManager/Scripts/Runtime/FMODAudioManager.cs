using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.FMODAudioManager
{
    public class FMODAudioManager : MonoBehaviour
    {
        const float BASEMANAGERVOLUME = 1f;

        public static readonly int NOVALUE = -999;

        private static FMODAudioManager _instance;
        public static FMODAudioManager Instance => _instance;

        public struct FMODAudioInstanceModifiers
        {
            public float baseVolume;
            public float managerVolume;

            public readonly float finalVolume => Mathf.Max(0, baseVolume * managerVolume);
        }

        public struct FAI_FadeMemory
        {
            public AnimationCurve animationCurve;
            public float durationElapsed;
            public bool wasFadeToStop;
            public bool wasFadeToPlay;
            public bool reverse;

            public void ResetValues()
            {
                animationCurve = null;
                durationElapsed = 0;

                wasFadeToPlay = false;
                wasFadeToStop = false;

                reverse = false;
            }
        }

        public class FMODAudioInstance
        {
            public EventInstance eventInstance;
            public FMOD.ChannelGroup channelGroup;
            public FMOD.DSP dsp;
            public FMODAudioInstanceModifiers audioModifiers = new();
            public Coroutine fadeRoutine = null;
            public FAI_FadeMemory fadeMemory = new();
            public HashSet<string> paramsNames = new();
            public HashSet<PARAMETER_ID> paramsIDs = new();
            public Dictionary<string, PARAMETER_ID> paramsDict = new();
        }

        private Dictionary<Guid, FMODAudioInstance> fmodAudioInstances = new();

        /// <summary>
        /// Returns the number of currently registered audio instances.  
        /// Readonly value for informational purposes only.
        /// </summary>
        public int RegisteredCount => fmodAudioInstances.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            if (_instance == null)
            {
                GameObject go = new("FMODAudioManager");
                _instance = go.AddComponent<FMODAudioManager>();
                DontDestroyOnLoad(go);
            }
        }

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

        #region Private Functions
        private EventInstance? CreateAudioInstance(FMODAudioScriptable audioScriptable)
        {
            var audioInstance = RuntimeManager.CreateInstance(audioScriptable.audioEventReference);

            if (!audioInstance.isValid())
            {
#if UNITY_EDITOR
                Debug.LogError($"Instance is invalid", audioScriptable);
                return null;
#endif
            }

            audioInstance.setVolume(audioScriptable.volumeScale);
            audioInstance.setPitch(audioScriptable.pitchScale);
            audioInstance.setProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, audioScriptable.attenuationMin);
            audioInstance.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, audioScriptable.attenuationMax);


            FMOD.ATTRIBUTES_3D attributes = new()
            {
                position = new FMOD.VECTOR { x = 0, y = 0, z = 0 },
                velocity = new FMOD.VECTOR { x = 0, y = 0, z = 0 },
                forward = new FMOD.VECTOR { x = 0, y = 0, z = 1 },
                up = new FMOD.VECTOR { x = 0, y = 1, z = 0 }
            };

            audioInstance.set3DAttributes(attributes);

            return audioInstance;
        }

        private bool IsSampleDataLoaded(EventDescription desc)
        {
            desc.getSampleLoadingState(out var state);
            return state == LOADING_STATE.LOADED || state == LOADING_STATE.LOADING;
        }
        private void AudioScriptableLogger(FMODAudioScriptable audioScriptable)
        {
#if UNITY_EDITOR
            if (!audioScriptable)
            {
                Debug.LogAssertion("Missing Audio Scriptable reference", audioScriptable);
                return;
            }
            if (!IsRegistered(audioScriptable))
            {
                Debug.LogWarning($"The audio Scriptable {audioScriptable.name} is not registered", audioScriptable);
            }
#endif
        }

        private void ParamsNameLogger(Guid runtimeID, string parameterName)
        {
#if UNITY_EDITOR
            if (!fmodAudioInstances[runtimeID].paramsNames.Contains(parameterName))
            {
                Debug.LogWarning($"The parameter name {parameterName} does not exist");
            }
#endif
        }

        private void ParamsIDLogger(Guid runtimeID, PARAMETER_ID parameterID)
        {
#if UNITY_EDITOR
            if (!fmodAudioInstances[runtimeID].paramsIDs.Contains(parameterID))
            {
                Debug.LogWarning($"The parameter id {parameterID} does not exist");
            }
#endif
        }
        #endregion

        #region Registration

        /// <summary>
        /// Registers the audio.
        /// </summary>
        /// <param name="audioScriptable"></param>
        public void RegisterAudio(FMODAudioScriptable audioScriptable)
        {
            if (!audioScriptable || fmodAudioInstances.ContainsKey(audioScriptable.runtimeID))
            {
                return;
            }


            var nullableInstance = CreateAudioInstance(audioScriptable);

            if (!nullableInstance.HasValue)
            {
                return;
            }

            Guid audioID = Guid.NewGuid();

            audioScriptable.runtimeID = audioID;
            var audioInstance = nullableInstance.Value;

            fmodAudioInstances[audioID] = new()
            {
                eventInstance = audioInstance,
            };

            var audioParamsNames = fmodAudioInstances[audioID].paramsNames;
            var audioParamsIDs = fmodAudioInstances[audioID].paramsIDs;

            var audioParams = fmodAudioInstances[audioID].paramsDict = GetEventParametersRaw(audioInstance);
            foreach (var audioParam in audioParams)
            {
                audioParamsNames.Add(audioParam.Key);
                audioParamsIDs.Add(audioParam.Value);
            }

            audioInstance.getVolume(out float volume);
            fmodAudioInstances[audioID].audioModifiers.baseVolume = volume;
            fmodAudioInstances[audioID].audioModifiers.managerVolume = BASEMANAGERVOLUME;

            StartCoroutine(LoadChannelGroup(fmodAudioInstances[audioID]));
        }


        /// <summary>
        /// Preloads data for performance boost.
        /// Registers audio if not registered
        /// </summary>
        /// <param name="audioScriptable"></param>
        public void PreloadData(FMODAudioScriptable audioScriptable)
        {
            if (!audioScriptable)
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }

            if (!IsRegistered(audioScriptable))
            {
                RegisterAudio(audioScriptable);
            }

            var fmodAudioInstance = fmodAudioInstances[audioScriptable.runtimeID];

            fmodAudioInstance.eventInstance.getDescription(out EventDescription desc);

            if (IsSampleDataLoaded(desc))
            {
                return;
            }

            desc.loadSampleData();
        }
        #endregion

        #region Play
        /// <summary>
        /// Instantly plays and detaches a non-trackable audio directly with RuntimeManager 
        /// and plays at given position.
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="position"></param>
        public void PlayOneShot(FMODAudioScriptable audioScriptable, Vector3 position)
        {
            RuntimeManager.PlayOneShot(audioScriptable.audioEventReference, position);
        }

        /// <summary>
        /// Instantly plays and detaches a non-trackable audio directly with RuntimeManager 
        /// and plays while following the given gameobject
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="gameObject"></param>
        public void PlayOneShotAttached(FMODAudioScriptable audioScriptable, GameObject gameObject)
        {
            RuntimeManager.PlayOneShotAttached(audioScriptable.audioEventReference, gameObject);
        }

        /// <summary>
        /// Simulates the play one shot but allows tracking of audio before it completely ends. <br/>
        /// On End <br/>
        /// - If detach is true, returns the raw EventInstance. <br/>
        /// - If detach is false, returns null. <br/>
        /// NOTE: DO NOT USE FOR LOOPING AUDIO IF DETACHED
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="position"></param>
        /// <param name="detach"></param>
        /// <returns></returns>
        public EventInstance? PlayOnce(FMODAudioScriptable audioScriptable, Vector3? position = null, bool detach = false)
        {
            Vector3 pos = (position != null && position.HasValue) ? position.Value : Vector3.zero;

            if (!audioScriptable)
            {
                AudioScriptableLogger(audioScriptable);
                return null;
            }

            if (detach)
            {
                var nullableInstance = CreateAudioInstance(audioScriptable);

                if (!nullableInstance.HasValue)
                {
                    return null;
                }

                var audioInstance = nullableInstance.Value;

                audioInstance.get3DAttributes(out FMOD.ATTRIBUTES_3D attributes);

                attributes.position = new FMOD.VECTOR
                {
                    x = pos.x,
                    y = pos.y,
                    z = pos.z
                };

                audioInstance.set3DAttributes(attributes);

                StartCoroutine(PlayOnceRoutine(audioInstance, detach));

                return audioInstance;
            }
            else
            {
                if (!fmodAudioInstances.ContainsKey(audioScriptable.runtimeID))
                {
                    RegisterAudio(audioScriptable);
                }
                var fmodAudioInstance = fmodAudioInstances[audioScriptable.runtimeID];

                if (position != null && position.HasValue) SetPosition(audioScriptable, position.Value);

                StartCoroutine(PlayOnceRoutine(fmodAudioInstance.eventInstance, detach));

                return null;
            }
        }

        /// <summary>
        /// Plays an audio. Recommended for loops <br/>
        /// If fadeAnimationCurve is valid, audio fades in using the curve
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="fadeAnimationCurve"></param>
        /// <param name="onFadeEnd"></param>
        public void PlayAudio(FMODAudioScriptable audioScriptable, AnimationCurve fadeAnimationCurve = null,
            Action onFadeEnd = null, bool reverseStopFade = false)
        {
            if (!audioScriptable)
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }

            if (!fmodAudioInstances.ContainsKey(audioScriptable.runtimeID))
            {
                RegisterAudio(audioScriptable);
            }

            var fmodAudioInstance = fmodAudioInstances[audioScriptable.runtimeID];

            if (reverseStopFade)
            {
                if (fmodAudioInstance.fadeMemory.wasFadeToPlay) return;

                if (fmodAudioInstance.fadeRoutine != null)
                {
                    StopCoroutine(fmodAudioInstance.fadeRoutine);
                    fmodAudioInstance.fadeRoutine = null;
                }
            }
            else if (IsPlaying(audioScriptable))
            {
                StopAudio(audioScriptable);
            }

            fmodAudioInstance.audioModifiers.managerVolume = BASEMANAGERVOLUME;

            if (reverseStopFade && fmodAudioInstance.fadeMemory.animationCurve != null)
            {
                fmodAudioInstance.fadeMemory.wasFadeToPlay = true;
                fmodAudioInstance.fadeMemory.wasFadeToStop = false;

                fmodAudioInstance.fadeMemory.reverse = !fmodAudioInstance.fadeMemory.reverse;

                fmodAudioInstance.fadeRoutine = StartCoroutine(ReverseFade(fmodAudioInstance, onFadeEnd,
                    fmodAudioInstance.fadeMemory.reverse));
                return;
            }

            fmodAudioInstance.fadeMemory.ResetValues();

            if (fadeAnimationCurve != null)
            {
                fmodAudioInstance.fadeMemory.wasFadeToPlay = true;
                fmodAudioInstance.fadeRoutine = StartCoroutine(Fade(fmodAudioInstance, fadeAnimationCurve, onFadeEnd));
            }

            fmodAudioInstance.eventInstance.setVolume(fmodAudioInstance.audioModifiers.finalVolume);
            fmodAudioInstance.eventInstance.start();
        }

        /// <summary>
        /// Similar to PlayAudio, Recommended for loops <br/>
        /// If fadeDuration is valid, audio fades in using the duration
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="fadeDuration"></param>
        /// <param name="onFadeEnd"></param>
        public void PlayAudioLinear(FMODAudioScriptable audioScriptable, float? fadeDuration = null,
            Action onFadeEnd = null, bool reverseStopFade = false)
        {
            if (fadeDuration.HasValue)
            {
                PlayAudio(audioScriptable, AnimationCurve.Linear(0f, 0f, fadeDuration.Value, 1f), onFadeEnd, reverseStopFade);
            }
            else
            {
                PlayAudio(audioScriptable, null, onFadeEnd, reverseStopFade);
            }
        }
        #endregion

        #region Stop
        /// <summary>
        /// Stops an audio if it has not been stopped <br/>
        /// If fadeAnimationCurve is valid, audio fades out using the curve
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="fadeAnimationCurve"></param>
        /// <param name="onFadeEnd"></param>
        public void StopAudio(FMODAudioScriptable audioScriptable, AnimationCurve fadeAnimationCurve = null,
            Action onFadeEnd = null, bool reversePlayFade = false)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }

            if (!IsPlaying(audioScriptable)) return;

            var fmodAudioInstance = fmodAudioInstances[audioScriptable.runtimeID];

            if (reversePlayFade && fmodAudioInstance.fadeMemory.wasFadeToStop) return;

            if (fmodAudioInstance.fadeRoutine != null)
            {
                StopCoroutine(fmodAudioInstance.fadeRoutine);
                fmodAudioInstance.fadeRoutine = null;
            }

            if (reversePlayFade && fmodAudioInstance.fadeMemory.animationCurve != null)
            {
                fmodAudioInstance.fadeMemory.wasFadeToStop = true;
                fmodAudioInstance.fadeMemory.wasFadeToPlay = false;
                fmodAudioInstance.fadeMemory.reverse = !fmodAudioInstance.fadeMemory.reverse;

                fmodAudioInstance.fadeRoutine = StartCoroutine(ReverseFade(fmodAudioInstance, onFadeEnd,
                    fmodAudioInstance.fadeMemory.reverse, true));

                return;
            }

            fmodAudioInstance.fadeMemory.ResetValues();

            if (fadeAnimationCurve != null)
            {
                fmodAudioInstance.fadeMemory.wasFadeToStop = true;
                fmodAudioInstance.fadeRoutine = StartCoroutine(Fade(fmodAudioInstance, fadeAnimationCurve, onFadeEnd, true));
            }
            else
            {
                fmodAudioInstance.eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        /// <summary>
        /// Similar to StopAudio, Stops an audio if it has not been stopped<br/>
        /// If fadeDuration is valid, audio fades out using the duration
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="fadeDuration"></param>
        /// <param name="onFadeEnd"></param>
        public void StopAudioLinear(FMODAudioScriptable audioScriptable, float? fadeDuration = null, Action onFadeEnd = null
            , bool reversePlayFade = false)
        {
            if (fadeDuration.HasValue)
            {
                StopAudio(audioScriptable, AnimationCurve.Linear(0f, 1f, fadeDuration.Value, 0f), onFadeEnd, reversePlayFade);
            }
            else
            {
                StopAudio(audioScriptable, null, onFadeEnd, reversePlayFade);
            }
        }
        #endregion

        #region Validators
        /// <summary>
        /// Checks if a parameter exist by name
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public bool HasParameterName(FMODAudioScriptable audioScriptable, string parameterName)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return false;
            }

            if (fmodAudioInstances[audioScriptable.runtimeID].paramsNames.Contains(parameterName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a parameter exists by id
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="parameterId"></param>
        /// <returns></returns>
        public bool HasParameterID(FMODAudioScriptable audioScriptable, PARAMETER_ID parameterId)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return false;
            }

            if (fmodAudioInstances[audioScriptable.runtimeID].paramsIDs.Contains(parameterId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks registeration of an audio
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public bool IsRegistered(FMODAudioScriptable audioScriptable)
        {
            return audioScriptable && fmodAudioInstances.ContainsKey(audioScriptable.runtimeID);
        }

        /// <summary>
        /// Checks if a registered audio was preloaded
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public bool IsPreloaded(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return false;
            }

            var fmodAudioInstance = fmodAudioInstances[audioScriptable.runtimeID];

            fmodAudioInstance.eventInstance.getDescription(out EventDescription desc);

            return IsSampleDataLoaded(desc);
        }

        /// <summary>
        /// Detects if the starting audio is fading (controlled by the Manager) or not
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public bool IsFading(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return false;
            }

            return fmodAudioInstances[audioScriptable.runtimeID].fadeRoutine != null;
        }

        /// <summary>
        /// Detects if a registered audio is playing
        /// </summary>
        public bool IsPlaying(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return false;
            }

            var instance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;
            return IsPlayingRaw(instance);
        }

        /// <summary>
        /// Detects if audio is playing and checks if audio instance is valid
        /// Does not care for registration state
        /// </summary>
        public bool IsPlayingRaw(EventInstance audioInstance)
        {
            if (!audioInstance.isValid()) return false;

            audioInstance.getPlaybackState(out PLAYBACK_STATE state);
            return state != PLAYBACK_STATE.STOPPED;
        }

        /// <summary>
        /// Detects if the registered audio is paused
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public bool IsPaused(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return false;
            }

            return IsPausedRaw(fmodAudioInstances[audioScriptable.runtimeID].eventInstance);
        }

        /// <summary>
        /// Detects if the unregistered or detached audio instance is paused
        /// </summary>
        /// <param name="audioInstance"></param>
        /// <returns></returns>
        public bool IsPausedRaw(EventInstance audioInstance)
        {
            if (!audioInstance.isValid()) return false;

            audioInstance.getPaused(out var paused);
            return paused;
        }

        /// <summary>
        /// Detects if a specific bus audio is paused
        /// </summary>
        /// <param name="busPath"></param>
        /// <returns></returns>
        public bool IsPausedByBusAudio(string busPath)
        {
            Bus bus = RuntimeManager.GetBus(busPath);
            bus.getPaused(out var paused);
            return paused;
        }

        /// <summary>
        /// Detects if the master bus is paused
        /// </summary>
        /// <returns></returns>
        public bool IsPausedMasterAudio()
        {
            Bus masterBus = RuntimeManager.GetBus("bus:/");
            masterBus.getPaused(out var paused);
            return paused;
        }

        /// <summary>
        /// Checks if a bus path exists
        /// </summary>
        /// <param name="busPath"></param>
        /// <returns></returns>
        public bool HasBus(string busPath)
        {
            RuntimeManager.StudioSystem.getBankList(out Bank[] banks);

            foreach (var bank in banks)
            {
                bank.getBusList(out Bus[] buses);
                foreach (var bus in buses)
                {
                    bus.getPath(out string path);
                    if (path == busPath) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if all banks have loaded
        /// </summary>
        /// <returns></returns>
        public bool HaveAllBanksLoaded()
        {
            return RuntimeManager.HaveAllBanksLoaded;
        }

        /// <summary>
        /// Checks if any sample data is loading
        /// </summary>
        /// <returns></returns>
        public bool AnySampleDataLoading()
        {
            return RuntimeManager.AnySampleDataLoading();
        }
        #endregion

        #region Setters
        /// <summary>
        /// Sets the position of a registered audio
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="position"></param>
        public void SetPosition(FMODAudioScriptable audioScriptable, Vector3 position)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }

            var instance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;

            SetPositionRaw(instance, position);
        }

        /// <summary>
        /// Sets the position of an unregistered audio
        /// </summary>
        /// <param name="audioInstance"></param>
        /// <param name="position"></param>
        public void SetPositionRaw(EventInstance audioInstance, Vector3 position)
        {
            if (!audioInstance.isValid()) return;

            audioInstance.get3DAttributes(out FMOD.ATTRIBUTES_3D attributes);

            attributes.position = new FMOD.VECTOR
            {
                x = position.x,
                y = position.y,
                z = position.z
            };

            audioInstance.set3DAttributes(attributes);
        }

        /// <summary>
        /// Sets the attenuation of a registered audio
        /// </summary>
        /// <param name="audioScriptable"></param>
        public void SetAttenuation(FMODAudioScriptable audioScriptable, float attenuationMin, float attenuationMax)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }

            var audioInstance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;

            audioInstance.setProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, Mathf.Max(attenuationMin, 0));
            audioInstance.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, Mathf.Max(attenuationMax, 0));
        }

        /// <summary>
        /// Sets the timeline position of a registered audio
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="position"></param>
        public void SetTimelinePosition(FMODAudioScriptable audioScriptable, int position)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }

            var audioInstance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;
            audioInstance.setTimelinePosition(Mathf.Max(position, 0));
        }

        /// <summary>
        /// Sets the value of parameter for a registered audio by the given name
        /// </summary>
        public void SetParameterByName(FMODAudioScriptable audioScriptable, string paramName, float value)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }

            var audioParamsNames = fmodAudioInstances[audioScriptable.runtimeID].paramsNames;

            if (!audioParamsNames.Contains(paramName))
            {
                ParamsNameLogger(audioScriptable.runtimeID, paramName);
                return;
            }

            fmodAudioInstances[audioScriptable.runtimeID].eventInstance.setParameterByName(paramName, value);
        }

        /// <summary>
        /// Sets the value of parameter for a registered audio by the given ID
        /// </summary>
        public void SetParameterByID(FMODAudioScriptable audioScriptable, PARAMETER_ID paramId, float value)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }

            var audioParamsIDs = fmodAudioInstances[audioScriptable.runtimeID].paramsIDs;

            if (!audioParamsIDs.Contains(paramId))
            {
                ParamsIDLogger(audioScriptable.runtimeID, paramId);
                return;
            }

            fmodAudioInstances[audioScriptable.runtimeID].eventInstance.setParameterByID(paramId, value);
        }

        /// <summary>
        /// Sets the value of a parameter for all audio by the given name
        /// </summary>
        public void SetGlobalParameter(string paramName, float value)
        {
            var status = RuntimeManager.StudioSystem.setParameterByName(paramName, value);
#if UNITY_EDITOR
            if (status != FMOD.RESULT.OK)
            {
                Debug.LogWarning($"The global parameter name: {paramName} does not exist");
            }
#endif
        }


        /// <summary>
        /// Sets the playback speed of a registered audio by adjusting pitch.
        /// </summary>
        public void SetPitch(FMODAudioScriptable audioScriptable, float pitch)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }

            fmodAudioInstances[audioScriptable.runtimeID].eventInstance.setPitch(Mathf.Max(pitch, 0f));
        }

        /// <summary>
        /// Overrides the audio volume of a registered audio and accounts for manager changes
        /// </summary>
        public void SetVolume(FMODAudioScriptable audioScriptable, float volume)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }

            var fmodAudioInstance = fmodAudioInstances[audioScriptable.runtimeID];
            fmodAudioInstance.audioModifiers.baseVolume = Mathf.Max(volume, 0f);
            fmodAudioInstance.eventInstance.setVolume(fmodAudioInstance.audioModifiers.finalVolume);
        }

        /// <summary>
        /// Overrides the global bus volume
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            Bus masterBus = RuntimeManager.GetBus("bus:/");
            masterBus.setVolume(Mathf.Max(volume, 0f));
        }

        /// <summary>
        /// Overrides a specific bus volume
        /// </summary>
        public void SetAudioTypeVolume(string busPath, float volume)
        {
            Bus bus = RuntimeManager.GetBus(busPath);
            bus.setVolume(Mathf.Max(volume, 0f));
        }

        /// <summary>
        /// Pauses or resumes a registered audio
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="pause"></param>
        public void SetPauseAudio(FMODAudioScriptable audioScriptable, bool pause = false)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return;
            }
            var instance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;

            SetPauseAudioRaw(instance, pause);
        }

        /// <summary>
        /// Pauses or continues an unregistered or detached audio instance
        /// </summary>
        /// <param name="audioInstance"></param>
        /// <param name="pause"></param>
        public void SetPauseAudioRaw(EventInstance audioInstance, bool pause = false)
        {
            if (!audioInstance.isValid()) return;

            audioInstance.getPaused(out var paused);
            if (paused == pause) return;
            audioInstance.setPaused(pause);
        }

        /// <summary>
        /// Pauses or resumes all audio of a specific bus
        /// </summary>
        /// <param name="busPath"></param>
        /// <param name="pause"></param>
        public void SetPauseByBusAudio(string busPath, bool pause)
        {
            Bus bus = RuntimeManager.GetBus(busPath);
            bus.getPaused(out var paused);
            if (paused == pause) return;
            bus.setPaused(pause);
        }

        /// <summary>
        /// Pauses or resumes all audio
        /// </summary>
        /// <param name="pause"></param>
        public void SetPauseMasterAudio(bool pause)
        {
            Bus masterBus = RuntimeManager.GetBus("bus:/");
            masterBus.getPaused(out var paused);
            if (paused == pause) return;
            masterBus.setPaused(pause);
        }
        #endregion

        #region Getters

        /// <summary>
        /// Creates and returns an audio ticket that provides direct access to modifiers <br/>
        /// Returns null if audio scriptable is null <br/>
        /// 
        /// Recommended for frame updates
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public FMODAudioTicket GetAudioTicket(FMODAudioScriptable audioScriptable)
        {
            if (!audioScriptable)
            {
                AudioScriptableLogger(audioScriptable);
                return null;
            }

            if (!fmodAudioInstances.ContainsKey(audioScriptable.runtimeID))
            {
                RegisterAudio(audioScriptable);
            }
            var fmodAudioInstance = fmodAudioInstances[audioScriptable.runtimeID];

            return new FMODAudioTicket(fmodAudioInstance);
        }

        /// <summary>
        /// ⚠️ EXPERT USE ONLY!, USE <see cref="GetAudioTicket(FMODAudioScriptable)"></see> INSTEAD ⚠️ <br/>
        /// Retrieves the raw FMOD EventInstance. <br/> 
        /// Conflicts may arise using this with the Audio Manager and responsiblility for its entire 
        /// lifecycle will no longer be controlled by the manager if 'forget' is true. 
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public EventInstance? GetRawEventInstance(FMODAudioScriptable audioScriptable, bool forget = false)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return null;
            }

            EventInstance eventInstance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;

            if (forget) fmodAudioInstances.Remove(audioScriptable.runtimeID);

            return eventInstance;
        }

        /// <summary>
        /// Gets the event parameters of an audio
        /// </summary>
        /// <param name="audioInstance"></param>
        /// <returns></returns>
        public Dictionary<string, PARAMETER_ID> GetEventParametersRaw(EventInstance audioInstance)
        {
            Dictionary<string, PARAMETER_ID> paramDict = new();

            audioInstance.getDescription(out EventDescription description);
            description.getParameterDescriptionCount(out int count);

            for (int i = 0; i < count; i++)
            {
                description.getParameterDescriptionByIndex(i, out PARAMETER_DESCRIPTION paramDesc);
                paramDict[paramDesc.name] = paramDesc.id;
            }

            return paramDict;
        }

        /// <summary>
        /// Gets the event parameters of a registered audio
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public IReadOnlyDictionary<string, PARAMETER_ID> GetEventParameters(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return null;
            }

            return fmodAudioInstances[audioScriptable.runtimeID].paramsDict;
        }

        /// <summary>
        /// Gets only the names of the event parameters
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public IReadOnlyCollection<string> GetEventParamteters_Name(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return null;
            }

            return fmodAudioInstances[audioScriptable.runtimeID].paramsNames;
        }

        /// <summary>
        /// Gets only the ids of the event paramters
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public IReadOnlyCollection<PARAMETER_ID> GetEventParameters_Id(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return null;
            }

            return fmodAudioInstances[audioScriptable.runtimeID].paramsIDs;
        }

        /// <summary>
        /// Returns the current volume of a registered audio
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public float GetVolume(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return NOVALUE;
            }

            var instance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;
            instance.getVolume(out float volume);
            return volume;
        }

        /// <summary>
        /// Returns the current pitch of a registered audio
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public float GetPitch(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return NOVALUE;
            }
            var instance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;
            instance.getPitch(out float pitch);

            return pitch;
        }

        public (float attenuationMin, float attenuationMax) GetAttenuation(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return (NOVALUE, NOVALUE);
            }

            var instance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;
            instance.getMinMaxDistance(out float minDistance, out float maxDistance);
            return (minDistance, maxDistance);
        }

        /// <summary>
        /// Gets playback completion percentage of a registered audio instance.
        /// Behaves like <see cref="GetCompletionByPercentageRaw"/>:
        /// <para> - Timeline events: returns 0.0–1.0, can exceed 1 for looping audio </para>
        /// <para> - Non-timeline events: 0 while playing, 1 when stopped </para>
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public float GetCompletionByPercentage(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return NOVALUE;
            }

            var instance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;

            return GetCompletionByPercentageRaw(instance);
        }

        /// <summary>
        /// Gets playback completion percentage of an unregistered or detached audio instance.
        /// <para> - Timeline events: returns 0.0–1.0, can exceed 1 for looping audio </para>
        /// <para> - Non-timeline events: 0 while playing, 1 when stopped </para>
        /// </summary>
        /// <param name="audioInstance"></param>
        /// <returns></returns>
        public float GetCompletionByPercentageRaw(EventInstance audioInstance)
        {
            if (!audioInstance.isValid()) return NOVALUE;

            audioInstance.getTimelinePosition(out int position);
            audioInstance.getDescription(out var desc);
            desc.getLength(out int length);

            if (length > 0)
            {
                return (float)position / length;
            }

            return !IsPlayingRaw(audioInstance) ? 1f : 0f;
        }

        /// <summary>
        /// Gets the length of a registered audio instance.
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public float GetLength(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return NOVALUE;
            }

            var instance = fmodAudioInstances[audioScriptable.runtimeID].eventInstance;

            return GetLengthRaw(instance);
        }

        /// <summary>
        /// Gets the length of an unregistered or detached audio instance.
        /// </summary>
        /// <param name="audioInstance"></param>
        /// <returns></returns>
        public float GetLengthRaw(EventInstance audioInstance)
        {
            if (!audioInstance.isValid()) return NOVALUE;

            audioInstance.getDescription(out var desc);
            desc.getLength(out int length);

            return length;
        }

        /// <summary>
        /// Gets the meter info which contains the peak and rms levels
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <returns></returns>
        public FMOD.DSP_METERING_INFO? GetMeterInfo(FMODAudioScriptable audioScriptable)
        {
            if (!IsRegistered(audioScriptable))
            {
                AudioScriptableLogger(audioScriptable);
                return null;
            }

            var fmodAudioInstance = fmodAudioInstances[audioScriptable.runtimeID];

            if (fmodAudioInstance.dsp.hasHandle())
            {
                fmodAudioInstance.dsp.getMeteringInfo(System.IntPtr.Zero, out var meterInfo);

                return meterInfo;
            }
            return null;
        }

        /// <summary>
        /// Calculates and returns the mean rms level from the passed meter info
        /// </summary>
        /// <param name="meterInfo"></param>
        /// <returns></returns>
        public float GetMeanRMS(FMOD.DSP_METERING_INFO meterInfo)
        {
            float meanRMS = 0f;

            if (meterInfo.numsamples > 0)
            {
                float rmsSum = 0f;
                int rmsLength = meterInfo.rmslevel.Length;
                for (int i = 0; i < rmsLength; i++)
                {
                    rmsSum += meterInfo.rmslevel[i] * meterInfo.rmslevel[i];
                }

                meanRMS = Mathf.Sqrt(rmsSum / (float)rmsLength);
            }

            return meanRMS;
        }

        /// <summary>
        /// Returns a list of all bus paths
        /// </summary>
        /// <returns></returns>
        public List<string> GetBusPaths()
        {
            RuntimeManager.StudioSystem.getBankList(out Bank[] banks);

            List<string> paths = new List<string>();

            foreach (var bank in banks)
            {
                bank.getBusList(out Bus[] buses);
                foreach (var bus in buses)
                {
                    bus.getPath(out string path);
                    paths.Add(path);
                }
            }

            return paths;
        }
        #endregion

        #region Release
        /// <summary>
        /// Releases an unregistered or detached audio from memory
        /// <para> Audio still plays until it ends if deferred is true </para>
        /// </summary>
        /// <param name="audioInstance"></param>
        public void ReleaseRaw(EventInstance audioInstance, bool deferred = false)
        {
            if (!audioInstance.isValid()) return;

            if (deferred) audioInstance.release();
            else
            {
                audioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                audioInstance.release();
            }
        }

        /// <summary>
        /// Erases registered audio from memory
        /// <para> Audio still plays until it ends if deferred is true </para>
        /// </summary>
        /// <param name="audioScriptable"></param>
        /// <param name="deferred"></param>
        public void Release(FMODAudioScriptable audioScriptable, bool deferred = false)
        {
            if (!IsRegistered(audioScriptable))
            {
                return;
            }

            var fmodAudioInstance = fmodAudioInstances[audioScriptable.runtimeID];

            if (deferred) fmodAudioInstance.eventInstance.release();
            else
            {
                if (fmodAudioInstance.fadeRoutine != null)
                {
                    StopCoroutine(fmodAudioInstance.fadeRoutine);
                    fmodAudioInstance.fadeRoutine = null;
                }

                fmodAudioInstance.eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                fmodAudioInstance.eventInstance.release();
            }

            fmodAudioInstances.Remove(audioScriptable.runtimeID);
        }

        /// <summary>
        /// Erases all registered audio from memory and releases all instances
        /// <para> Audio still plays until it ends if deferred is true </para>
        /// </summary>
        /// <param name="deferred"></param>
        public void ReleaseAll(bool deferred = false)
        {
            foreach (var fmodAudioInstance in fmodAudioInstances.Values)
            {

                if (deferred) fmodAudioInstance.eventInstance.release();
                else
                {
                    if (fmodAudioInstance.fadeRoutine != null)
                    {
                        StopCoroutine(fmodAudioInstance.fadeRoutine);
                        fmodAudioInstance.fadeRoutine = null;
                    }

                    fmodAudioInstance.eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    fmodAudioInstance.eventInstance.release();
                }
            }

            fmodAudioInstances.Clear();
        }
        #endregion

        #region Enumerators

        private IEnumerator Fade(FMODAudioInstance fmodAudioInstance, AnimationCurve animationCurve,
            Action onFadeEnd, bool stop = false)
        {
            if (animationCurve.length == 0)
            {
                onFadeEnd?.Invoke();
                fmodAudioInstance.fadeRoutine = null;
                fmodAudioInstance.fadeMemory.ResetValues();
                yield break;
            }

            float duration = animationCurve[animationCurve.length - 1].time;
            float elapsed = 0f;

            float initialVolume = fmodAudioInstance.audioModifiers.managerVolume;
            fmodAudioInstance.fadeMemory.animationCurve = animationCurve;

            while (elapsed < duration)
            {
                fmodAudioInstance.audioModifiers.managerVolume = initialVolume * animationCurve.Evaluate(elapsed);

                fmodAudioInstance.eventInstance.setVolume(fmodAudioInstance.audioModifiers.finalVolume);

                elapsed += Time.unscaledDeltaTime;

                fmodAudioInstance.fadeMemory.durationElapsed = elapsed;

                yield return null;
            }

            fmodAudioInstance.audioModifiers.managerVolume = animationCurve.Evaluate(duration);

            fmodAudioInstance.eventInstance.setVolume(fmodAudioInstance.audioModifiers.finalVolume);

            if (stop)
            {
                fmodAudioInstance.eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }

            onFadeEnd?.Invoke();

            fmodAudioInstance.fadeRoutine = null;
            fmodAudioInstance.fadeMemory.ResetValues();
        }

        private IEnumerator ReverseFade(FMODAudioInstance fmodAudioInstance,
            Action onFadeEnd, bool flip, bool stop = false)
        {
            AnimationCurve animationCurve = fmodAudioInstance.fadeMemory.animationCurve;

            if (animationCurve == null || animationCurve.length == 0)
            {
                onFadeEnd?.Invoke();
                fmodAudioInstance.fadeRoutine = null;
                fmodAudioInstance.fadeMemory.ResetValues();
                yield break;
            }

            float duration = animationCurve[animationCurve.length - 1].time;
            float elapsed = 0f;

            float initialVolume = fmodAudioInstance.audioModifiers.managerVolume;

            if (!flip)
            {
                while (elapsed < duration)
                {
                    fmodAudioInstance.audioModifiers.managerVolume = initialVolume * animationCurve.Evaluate(elapsed);

                    fmodAudioInstance.eventInstance.setVolume(fmodAudioInstance.audioModifiers.finalVolume);

                    elapsed += Time.unscaledDeltaTime;

                    fmodAudioInstance.fadeMemory.durationElapsed = elapsed;

                    yield return null;
                }

                fmodAudioInstance.audioModifiers.managerVolume = animationCurve.Evaluate(duration);
            }
            else
            {
                elapsed = fmodAudioInstance.fadeMemory.durationElapsed;

                while (elapsed > 0)
                {
                    fmodAudioInstance.audioModifiers.managerVolume = initialVolume * animationCurve.Evaluate(elapsed);

                    fmodAudioInstance.eventInstance.setVolume(fmodAudioInstance.audioModifiers.finalVolume);

                    elapsed -= Time.unscaledDeltaTime;

                    fmodAudioInstance.fadeMemory.durationElapsed = elapsed;

                    yield return null;
                }

                fmodAudioInstance.audioModifiers.managerVolume = animationCurve.Evaluate(0);
            }

            fmodAudioInstance.eventInstance.setVolume(fmodAudioInstance.audioModifiers.finalVolume);

            if (stop)
            {
                fmodAudioInstance.eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }

            onFadeEnd?.Invoke();

            fmodAudioInstance.fadeRoutine = null;
            fmodAudioInstance.fadeMemory.ResetValues();
        }

        private IEnumerator PlayOnceRoutine(EventInstance audioInstance, bool detach)
        {
            audioInstance.start();

            if (detach) audioInstance.release();

            PLAYBACK_STATE state;
            do
            {
                audioInstance.getPlaybackState(out state);
                yield return null;
            } while (state != PLAYBACK_STATE.STOPPED);
        }

        private IEnumerator LoadChannelGroup(FMODAudioInstance fmodAudioInstance)
        {
            EventInstance eventInstance = fmodAudioInstance.eventInstance;

            FMOD.ChannelGroup channelGroup;
            while (eventInstance.getChannelGroup(out channelGroup) != FMOD.RESULT.OK)
            {
                yield return null;
            }

            fmodAudioInstance.channelGroup = channelGroup;
            channelGroup.getDSP(0, out fmodAudioInstance.dsp);
            fmodAudioInstance.dsp.setMeteringEnabled(false, true);
        }


        #endregion

        private void OnDestroy()
        {
            ReleaseAll(false);
        }
    }

}