using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbyssWorks.FMODAudioManager;

namespace AbyssWorks.FMODAudioManager.Misc
{
    public class RMSReader : MonoBehaviour
    {
        public FMODAudioScriptable audioScriptable;
        public float scaleFac = 1f;
        public float lerpFac = 2f;

        private Vector3 originalScale;
        // Start is called before the first frame update
        void Start()
        {
            originalScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            if (FMODAudioManager.Instance && FMODAudioManager.Instance.IsPlaying(audioScriptable))
            {
                var meterInfo = FMODAudioManager.Instance.GetMeterInfo(audioScriptable);

                if (meterInfo.HasValue)
                {
                    float rms = FMODAudioManager.Instance.GetMeanRMS(meterInfo.Value);

                    transform.localScale = Vector3.Lerp(transform.localScale,
                        originalScale * (1 + rms * scaleFac), Time.deltaTime * lerpFac);
                }

            }
        }
    }

}
