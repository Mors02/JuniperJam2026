using FMOD.Studio;
using UnityEngine;

namespace AbyssWorks.FMODAudioManager
{
    public class FMODAudioTester : MonoBehaviour
    {
        public enum FadeMode { Instant, Fade }

        [Header("Global Parameter Settings")]
        public string globalParameterName;
        public float globalParameterInfluence;

        [Header("Audio Settings")]
        public FMODAudioScriptable audioScriptable;
        public string parameterName;
        public float parameterInfluence;
        public FMODAudio2DVolumeAttenuator audio2DAttenuation;
        public bool attenuate2D = false;

        [Header("Audio Play Controls")]
        public bool loopOneShot = false;
        public bool allowReverse = false;
        public FadeMode playMode = FadeMode.Instant;
        public FadeMode stopMode = FadeMode.Instant;
        [Range(0, 10f)] public float fadeDuration = 1f;

        public float rms;

        [Header("Detached Audio Settings")]
        public FMODAudioScriptable sfxScriptable;
        public Transform callPosition;
        [Tooltip("Sets volume of currently playing detached audio")]
        [Range(0, 1)] public float volumeScale = 1f;
        [Tooltip("Sets pitch of currently playing detached audio")]
        [Range(0, 2)] public float pitchScale = 1f;

        private EventInstance sfxInstance;
        private FMODAudioScriptable prevAudioScriptable;
        private FMODAudioTicket audioTicket = null;

        private bool validateChangeTracker = false;

        private void OnValidate()
        {
            validateChangeTracker = true;
        }

        private void UpdateValuesOnValidate()
        {
            if (!validateChangeTracker) return;

            if (FMODAudioManager.Instance)
            {
                if (globalParameterName != "")
                {
                    FMODAudioManager.Instance.SetGlobalParameter(globalParameterName, globalParameterInfluence);
                }

                if ((bool)audioTicket && parameterName != "")
                {
                    audioTicket.SetParameterByName(parameterName, parameterInfluence);
                }
            }

            if ((bool)audioTicket && parameterName != "")
            {
                FMODAudioManager.Instance.SetParameterByName(audioScriptable, parameterName, parameterInfluence);
            }

            validateChangeTracker = false;
        }

        private void Awake()
        {
            if (FMODAudioManager.Instance && audioScriptable)
            {
                //if(playMode == FadeMode.Fade) FMODAudioManager.Instance.PlayAudioLinear(audioScriptable, fadeDuration);
                //else FMODAudioManager.Instance.PlayAudio(audioScriptable);
                FMODAudioManager.Instance.RegisterAudio(audioScriptable);

                audioTicket = FMODAudioManager.Instance.GetAudioTicket(audioScriptable);

                if (parameterName != "")
                {
                    FMODAudioManager.Instance.SetParameterByName(audioScriptable, parameterName, parameterInfluence);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdateValuesOnValidate();

            if (prevAudioScriptable != audioScriptable)
            {
                if (FMODAudioManager.Instance && audioScriptable)
                    audioTicket = FMODAudioManager.Instance.GetAudioTicket(audioScriptable);
                else
                    audioTicket = null;
            }
            prevAudioScriptable = audioScriptable;


            if (audioTicket)
            {
                audioTicket.Volume = audioScriptable.volumeScale;
                audioTicket.Pitch = audioScriptable.pitchScale;

                if (loopOneShot && !audioTicket.IsPlaying)
                {
                    FMODAudioManager.Instance.PlayAudio(audioScriptable);
                }

                if (attenuate2D)
                {
                    FMODAudioManager.Instance.SetAttenuation(audioScriptable, audioScriptable.attenuationMin, audioScriptable.attenuationMax);
                    if (audio2DAttenuation) audio2DAttenuation.Attenuate(audioTicket, transform.position);
                }
                else audioTicket.Position = transform.position;

                if (audioTicket.MeterInfo.HasValue)
                {
                    rms = FMODAudioManager.Instance.GetMeanRMS(audioTicket.MeterInfo.Value);
                }
            }

            if (FMODAudioManager.Instance && FMODAudioManager.Instance.IsPlayingRaw(sfxInstance))
            {
                sfxInstance.setVolume(sfxScriptable.volumeScale * volumeScale);
                sfxInstance.setPitch(sfxScriptable.pitchScale * pitchScale);
            }
        }

        public void PlaySFX()
        {
            if (FMODAudioManager.Instance && sfxScriptable)
            {
                Vector3 playPosition = callPosition ? callPosition.position : transform.position;
                var audioInstance = FMODAudioManager.Instance.PlayOnce(sfxScriptable, playPosition, true);
                if (audioInstance.HasValue) sfxInstance = audioInstance.Value;
            }
        }


    }

}

