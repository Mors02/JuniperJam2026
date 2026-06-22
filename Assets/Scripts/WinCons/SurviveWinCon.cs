using Unity.VisualScripting;
using UnityEngine;

public class SurviveWinCon : WinCon
{
    float _passedTime;
    public SurviveWinCon(float howManySeconds, string description) : base(WinConType.Survive, description)
    {
        _timeLeft = howManySeconds;
        _passedTime = 0;
    }

    public override bool CheckWinCon()
    {
        return _passedTime >= _timeLeft;
    }

    public override void UpdateUI()
    {
        string progressText = Mathf.RoundToInt(_passedTime) + "/" + _timeLeft;
        OnWinConUpdate.Invoke(progressText, CheckWinCon());
    }

    public override void UpdateWinCon()
    {
        _passedTime += Time.fixedDeltaTime;
        UpdateUI();
    }
}
