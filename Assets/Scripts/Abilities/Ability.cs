using AbyssWorks.ParasiteBehaviour;
using System;
using UnityEngine;

[System.Serializable]
public class Ability : ParasiteBehaviour
{
    public Action onExecutionComplete;
    public Action onExecutionCancel;
    public Action onExecutionFail;

    public virtual bool CanTrigger()
    {
        return true;
    }

    public virtual void Trigger() { }

    public void TryTrigger()
    {
        if (!CanTrigger())
            return;

        Trigger();
    }

    public virtual bool IsExecuting() => false;

    public virtual void CancelExecution(bool forceCancel = false) { }
}
