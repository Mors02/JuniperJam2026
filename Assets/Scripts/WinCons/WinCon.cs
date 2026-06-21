using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class WinCon
{   

    public UnityEvent<string, bool> OnWinConUpdate;
    public WinConType WinConType;

    /// <summary>
    /// Check if the wincon is completed
    /// </summary>
    /// <returns>true if completed, false otherwise</returns>
    public abstract bool CheckWinCon();

    public abstract void UpdateWinCon();

    public abstract void UpdateUI();

    /// <summary>
    /// Time left for the wincon to satisfy
    /// </summary>
    protected float _timeLeft;
    public float TimeLeft => _timeLeft;

    /// <summary>
    /// Number of things left to satisfy the wincon
    /// </summary>
    protected int _numberLeft;
    public int NumberLeft => _numberLeft;

    /// <summary>
    /// Total number of things to do to complete the wincon
    /// </summary>
    protected int _totalNumber;
    public int TotalNumber;

    protected string _description;
    public string Description => _description;


    public WinCon(WinConType type, string description)
    {
        this.WinConType = type;
        this._description = description;
        OnWinConUpdate = new UnityEvent<string, bool>();
    }
}

public enum WinConType
{
    CollectCoins,
    KillEnemies,
    Stealth,
    Survive,
    RingLeader,
    FloorIsLava
}
