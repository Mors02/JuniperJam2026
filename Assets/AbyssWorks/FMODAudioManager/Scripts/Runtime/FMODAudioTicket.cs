using FMOD.Studio;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.FMODAudioManager
{
    public class FMODAudioTicket
    {
        private FMODAudioManager.FMODAudioInstance fmodAudioInstance;

        public FMODAudioTicket(FMODAudioManager.FMODAudioInstance fmodAudioInstance)
        {
            this.fmodAudioInstance = fmodAudioInstance;
        }

        public float Volume
        {
            get
            {
                if (!IsEventValid) return FMODAudioManager.NOVALUE;

                fmodAudioInstance.eventInstance.getVolume(out float volume); return volume;
            }
            set
            {
                if (!IsEventValid) return;

                fmodAudioInstance.audioModifiers.baseVolume = Mathf.Max(value, 0f);
                fmodAudioInstance.eventInstance.setVolume(fmodAudioInstance.audioModifiers.finalVolume);
            }
        }

        public float Pitch
        {
            get
            {
                if (!IsEventValid) return FMODAudioManager.NOVALUE;

                fmodAudioInstance.eventInstance.getPitch(out float pitch); return pitch;
            }
            set
            {
                if (!IsEventValid) return;

                fmodAudioInstance.eventInstance.setPitch(Mathf.Max(value, 0f));
            }
        }

        public Vector3 Position
        {
            get
            {
                if (!IsEventValid) return Vector3.zero;

                fmodAudioInstance.eventInstance.get3DAttributes(out FMOD.ATTRIBUTES_3D attributes);
                return new Vector3(attributes.position.x, attributes.position.y, attributes.position.z);
            }
            set
            {
                if (!IsEventValid) return;

                fmodAudioInstance.eventInstance.get3DAttributes(out FMOD.ATTRIBUTES_3D attributes);

                attributes.position = new FMOD.VECTOR
                {
                    x = value.x,
                    y = value.y,
                    z = value.z
                };

                fmodAudioInstance.eventInstance.set3DAttributes(attributes);
            }
        }

        public (float attenuationMin, float attenuationMax) Attenuation
        {
            get
            {
                if (!IsEventValid) return (FMODAudioManager.NOVALUE, FMODAudioManager.NOVALUE);

                fmodAudioInstance.eventInstance.getMinMaxDistance(out float minDistance, out float maxDistance);
                return (minDistance, maxDistance);
            }
            set
            {
                if (!IsEventValid) return;

                fmodAudioInstance.eventInstance.setProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, Mathf.Max(value.attenuationMin, 0f));
                fmodAudioInstance.eventInstance.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, Mathf.Max(value.attenuationMax, 0f));
            }
        }

        public int TimelinePosition
        {
            get
            {
                if (!IsEventValid) return FMODAudioManager.NOVALUE;

                fmodAudioInstance.eventInstance.getTimelinePosition(out int timelinePos); return timelinePos;
            }
            set
            {
                if (!IsEventValid) return;

                fmodAudioInstance.eventInstance.setTimelinePosition(Mathf.Max(value, 0));
            }
        }

        public float Length
        {
            get
            {
                if (!IsEventValid) return FMODAudioManager.NOVALUE;

                fmodAudioInstance.eventInstance.getDescription(out var desc);
                desc.getLength(out int length);

                return length;
            }
        }

        public float CompletionByPercentage
        {
            get
            {
                if (!IsEventValid) return FMODAudioManager.NOVALUE;

                fmodAudioInstance.eventInstance.getTimelinePosition(out int position);
                fmodAudioInstance.eventInstance.getDescription(out var desc);
                desc.getLength(out int length);

                if (length > 0)
                {
                    return (float)position / length;
                }

                return !IsPlaying ? 1f : 0f;
            }
        }

        public FMOD.DSP_METERING_INFO? MeterInfo
        {
            get
            {
                if (fmodAudioInstance.dsp.hasHandle())
                {
                    fmodAudioInstance.dsp.getMeteringInfo(System.IntPtr.Zero, out var meterInfo);

                    return meterInfo;
                }
                return null;
            }
        }

        public bool Pause
        {
            get
            {
                if (!IsEventValid) return false;

                fmodAudioInstance.eventInstance.getPaused(out var paused);
                return paused;
            }
            set
            {
                if (!IsEventValid) return;
                fmodAudioInstance.eventInstance.setPaused(value);
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (!IsEventValid) return false;

                fmodAudioInstance.eventInstance.getPlaybackState(out PLAYBACK_STATE state);
                return state != PLAYBACK_STATE.STOPPED;
            }
        }

        public void SetParameterByName(string parameterName, float value)
        {
            if (!HasParameterName(parameterName))
            {
                return;
            }

            fmodAudioInstance.eventInstance.setParameterByName(parameterName, value);
        }

        public void SetParameterByID(PARAMETER_ID parameterId, float value)
        {
            if (!HasParameterID(parameterId))
            {
                return;
            }

            fmodAudioInstance.eventInstance.setParameterByID(parameterId, value);
        }

        public IReadOnlyDictionary<string, PARAMETER_ID> PARAMETERS => fmodAudioInstance.paramsDict;
        public IReadOnlyCollection<string> PARAMETERS_Name => fmodAudioInstance.paramsNames;
        public IReadOnlyCollection<PARAMETER_ID> PARAMETERS_ID => fmodAudioInstance.paramsIDs;

        public bool IsEventValid => fmodAudioInstance.eventInstance.isValid();

        public bool IsFading => fmodAudioInstance.fadeRoutine != null;

        public bool HasParameterName(string parameterName) => fmodAudioInstance.paramsNames.Contains(parameterName);

        public bool HasParameterID(PARAMETER_ID parameterId) => fmodAudioInstance.paramsIDs.Contains(parameterId);

        public static bool operator true(FMODAudioTicket audioTicket) => audioTicket != null;
        public static bool operator false(FMODAudioTicket audioTicket) => audioTicket == null;
        public static bool operator !(FMODAudioTicket audioTicket) => audioTicket == null;

        public static explicit operator bool(FMODAudioTicket audioTicket) => audioTicket != null;
    }

}

