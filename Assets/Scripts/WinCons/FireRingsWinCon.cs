using UnityEngine;

public class FireRingsWinCon : WinCon
{
    public FireRingsWinCon(int rings, string description) : base(WinConType.FireRings, description)
    {
        _numberLeft = 0;
        _totalNumber = rings;
    }

    public override bool CheckWinCon()
    {
         if (_numberLeft >= _totalNumber)
            return true;
        return false;
    }

    public override void UpdateUI()
    {
        string progressText = _numberLeft + "/" + _totalNumber;
        OnWinConUpdate.Invoke(progressText, CheckWinCon());
    }

    public override void UpdateWinCon()
    {
        this._numberLeft++;
        UpdateUI();
    }
}
