using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectCoinWinCon : WinCon
{
    public CollectCoinWinCon(int coins, int alreadyCollected, string description) : base(WinConType.CollectCoins, description)
    {
        this._totalNumber = coins;
        this._numberLeft = alreadyCollected;
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
