using UnityEngine;

public class KillEnemiesWinCon : WinCon
{
    public KillEnemiesWinCon(int enemies, int alreadyKilled, string description) : base(WinConType.KillEnemies, description)
    {
        _numberLeft = alreadyKilled;
        _totalNumber = enemies;
    }

    public override bool CheckWinCon()
    {
        if (_numberLeft >= _totalNumber)
            return true;
        return false;
    }

    public override void UpdateWinCon()
    {
        this._numberLeft++;
        UpdateUI();
    }

    public override void UpdateUI()
    {
        string progressText = _numberLeft + "/" + _totalNumber;
        OnWinConUpdate.Invoke(progressText, CheckWinCon());
    }
}
