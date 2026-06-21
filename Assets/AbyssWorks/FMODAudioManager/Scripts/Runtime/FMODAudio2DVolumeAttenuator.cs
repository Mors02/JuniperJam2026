using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.FMODAudioManager
{
    public class FMODAudio2DVolumeAttenuator : MonoBehaviour
    {
        public void Attenuate(FMODAudioScriptable audioScriptable, Vector3 position, float volumeMultiplier = 1f)
        {
            if (FMODAudioManager.Instance && audioScriptable)
            {
                var (attenuationMin, attenuationMax) = FMODAudioManager.Instance.GetAttenuation(audioScriptable);
                var currentVolume = FMODAudioManager.Instance.GetVolume(audioScriptable);

                float distance = Vector3.Distance(transform.position, position);
                distance = Mathf.Clamp(distance, attenuationMin, attenuationMax);
                float volumeFactor = 1f - distance / (attenuationMax + Mathf.Epsilon);
                FMODAudioManager.Instance.SetVolume(audioScriptable, volumeFactor * currentVolume * volumeMultiplier);
            }
        }

        public void Attenuate(FMODAudioTicket audioTicket, Vector3 position, float volumeMultiplier = 1f)
        {
            if (audioTicket)
            {
                var (attenuationMin, attenuationMax) = audioTicket.Attenuation;
                var currentVolume = audioTicket.Volume;

                float distance = Vector3.Distance(transform.position, position);
                distance = Mathf.Clamp(distance, attenuationMin, attenuationMax);
                float volumeFactor = 1f - distance / (attenuationMax + Mathf.Epsilon);
                audioTicket.Volume = volumeFactor * currentVolume * volumeMultiplier;
            }
        }
    }
}
