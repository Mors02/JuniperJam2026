using System;
using UnityEngine;

namespace AbyssWorks.Misc
{
    public class AnimationEnd : MonoBehaviour
    {
        private uint count = 0;
        public Action animationEnd;

        public void SetAnimationEnd(Action action)
        {
            animationEnd = action;
        }

        public void OnAnimationEnd()
        {
            count++;

            animationEnd?.Invoke();
        }

        public uint GetCount()
        {
            return count;
        }

        private void OnDestroy()
        {
            animationEnd = null;
        }
    }

}
