using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.AnimatorSignal
{
    [RequireComponent(typeof(Animator))]
    /// <summary>
    /// Support class for animator and event listener.
    /// </summary>
    public class AnimationSubscriber : MonoBehaviour
    {
        private uint endCount = 0;
        private List<Action> endActions = new();

        private uint startCount = 0;
        private List<Action> startActions = new();

        private class AnimAction
        {
            public List<Action> actions = new();
            public uint count = 0;
        }

        private Dictionary<string, AnimAction> actionDict = new();

        public void SubscribeEndAction(Action action) => endActions.Add(action);
        public void UnSubscribeEndAction(Action action) => endActions.Remove(action);
        public void ClearEndSubscriptions() => endActions.Clear();

        public void SubscribeStartAction(Action action) => startActions.Add(action);
        public void UnSubscribeStartAction(Action action) => startActions.Remove(action);
        public void ClearStartSubscriptions() => startActions.Clear();

        public void SubscribeAction(string actionName, Action action)
        {
            if (!actionDict.ContainsKey(actionName))
            {
                AnimAction animAction = new();

                actionDict[actionName] = animAction;
            }

            actionDict[actionName].actions.Add(action);
        }

        public void UnSubscribeAction(string actionName, Action action)
        {
            if (actionDict.TryGetValue(actionName, out AnimAction animAction))
            {
                animAction.actions.Remove(action);
            }
        }

        public void ClearActionSubscriptions(string actionName)
        {
            if (actionDict.TryGetValue(actionName, out AnimAction animAction))
            {
                animAction.actions.Clear();
            }
        }


        public void AnimationEnd()
        {
            endCount++;
            InvokeActions(endActions);
        }

        public void AnimationStart()
        {
            startCount++;
            InvokeActions(startActions);
        }

        public void TriggerAction(string actionName)
        {
            if (actionDict.TryGetValue(actionName, out AnimAction animAction))
            {
                animAction.count++;
                InvokeActions(animAction.actions);
            }
        }

        public uint GetEndCount()
        {
            return endCount;
        }

        public uint GetStartCount()
        {
            return startCount;
        }

        public uint GetCount(string actionName)
        {
            if (actionDict.TryGetValue(actionName, out AnimAction animAction))
            {
                return animAction.count;
            }
            return 0;
        }

        public void InvokeActions(List<Action> actions)
        {
            foreach (Action action in actions)
            {
                action?.Invoke();
            }
        }

        private void OnDestroy()
        {
            startActions.Clear();
            endActions.Clear();
            foreach (var (_, value) in actionDict)
            {
                value.actions.Clear();
            }
        }
    }

}

