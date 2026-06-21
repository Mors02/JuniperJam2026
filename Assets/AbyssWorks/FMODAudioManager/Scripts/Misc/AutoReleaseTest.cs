using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.FMODAudioManager.Misc
{
    public class AutoReleaseTest : MonoBehaviour
    {
        public FMODAudioScriptable audioScriptable;

        public bool removeOne = false;

        private List<FMODAudioScriptable> fMODAudioScriptables = new();
        // Start is called before the first frame update
        void Start()
        {
            if (FMODAudioManager.Instance)
            {
                FMODAudioManager.Instance.RegisterAudio(audioScriptable);
            }

            for (int i = 0; i < 5; i++)
            {
                var newAudio = Instantiate(audioScriptable);
                fMODAudioScriptables.Add(newAudio);
                newAudio.name += " " + i;
                if (FMODAudioManager.Instance)
                {
                    FMODAudioManager.Instance.RegisterAudio(newAudio);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (removeOne)
            {
                removeOne = false;
                if (fMODAudioScriptables.Count > 0)
                {
                    var someAudio = fMODAudioScriptables[fMODAudioScriptables.Count - 1];
                    fMODAudioScriptables.RemoveAt(fMODAudioScriptables.Count - 1);
                    Destroy(someAudio);
                }
            }
        }
    }

}

