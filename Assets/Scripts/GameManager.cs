using System.Linq;
using UnityEngine;

public class GameManager
{
    private static GameManager _instance;

    private GameManager()
    {
        CurrentLives = 3;
    }

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameManager();
                _instance.CurrentWinCon = null;
                
            }

            _instance.UIManager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIManager>();
            
           

            return _instance;
        }
    }

    public int CurrentLives;

    public WinCon CurrentWinCon;

    public UIManager UIManager;

    public void SetCurrentWinCondition(WinConType type, WheelReward reward)
    {
        switch (type)
        {
            case WinConType.CollectCoins:
                
                GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
                
                CurrentWinCon = new CollectCoinWinCon(coins.Length, reward.Description);
                foreach(GameObject coin in coins)
                {
                    Coin coinComponent = coin.GetComponentInChildren<Coin>();
                    coinComponent.Spawn();
                }
                break;
        }

        CurrentWinCon.OnWinConUpdate.AddListener(_instance.UIManager.UpdateWinConUI);

        _instance.UIManager.UpdateWinConDescriptionUI(CurrentWinCon);
    }

    public bool Won()
    {
        if (CurrentWinCon == null)
            return false;

        return CurrentWinCon.CheckWinCon();
    }
}
