using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectCoinWinCon : WinCon
{
    public CollectCoinWinCon(int coins, string description) : base(WinConType.CollectCoins, description)
    {
        this._totalNumber = coins;
        this._numberLeft = 0;
    }

    public override bool CheckWinCon()
    {
        Debug.Log(_numberLeft >= _totalNumber);
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
