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
                _instance.Init();
            }

            return _instance;
        }
    }

    public int CurrentLives;

    public WinCon CurrentWinCon;

    public int CollectedCoins;
    public int TotalCoins;
    public int KilledEnemies;
    public int TotalEnemies;

    public UIManager UIManager;

    public SurviveManager SurviveManager;

    public RingLeaderBehaviour RingLeader;

    public LavaFloorBehaviour LavaFloor;

    /// <summary>
    /// Change current wincondition
    /// </summary>
    /// <param name="type">The Wincon type</param>
    /// <param name="reward">The reward that generated this wincon</param>
    public void SetCurrentWinCondition(WinConType type, WheelReward reward)
    {
        switch (type)
        {
            case WinConType.CollectCoins:
                
                
                
                CurrentWinCon = new CollectCoinWinCon(TotalCoins, CollectedCoins, reward.Description);
                /*foreach(GameObject coin in coins)
                {
                    Coin coinComponent = coin.GetComponentInChildren<Coin>();
                    coinComponent.Spawn();
                }*/
                break;
            case WinConType.KillEnemies:
                
                CurrentWinCon = new KillEnemiesWinCon(TotalEnemies, KilledEnemies, reward.Description);
                break;
            case WinConType.FloorIsLava:
                CurrentWinCon = new LavaFloorWinCon(reward.Description);
                break;
            case WinConType.Stealth:
                //activate stealth mode for every enemy which visualizes the line sight cone
                break;
            case WinConType.RingLeader:
                CurrentWinCon = new RingLeaderWinCon(reward.Description);
                break;
            case WinConType.Survive:

                CurrentWinCon = new SurviveWinCon(60f, reward.Description);
                
                //activate the gameobject that shoots projectiles
                //the gameobject should contain a update function that calls WinconUpdate every frame
                break;
            
        }

        CurrentWinCon.OnWinConUpdate.AddListener(_instance.UIManager.UpdateWinConUI);

        _instance.UIManager.UpdateWinConDescriptionUI(CurrentWinCon);
    }

    /// <summary>
    /// Checks if the player has won
    /// </summary>
    /// <returns></returns>
    public bool Won()
    {
        if (CurrentWinCon == null)
            return false;

        return CurrentWinCon.CheckWinCon();
    }

    /// <summary>
    /// Initialize variables related to the scene
    /// </summary>
    public void Init()
    {
        _instance.CurrentWinCon = null;
        _instance.CollectedCoins = 0;
        _instance.KilledEnemies = 0;
        _instance.TotalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;
        _instance.TotalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        _instance.UIManager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIManager>();
        _instance.SurviveManager = GameObject.FindGameObjectWithTag("Survive").GetComponent<SurviveManager>();
        _instance.RingLeader = GameObject.FindGameObjectWithTag("RingLeader").GetComponent<RingLeaderBehaviour>();
        _instance.LavaFloor = GameObject.FindGameObjectWithTag("LavaFloor").GetComponentInChildren<LavaFloorBehaviour>();
    }

    /// <summary>
    /// If the WinCon has an activation, activate it
    /// </summary>
    public void ActivateWinConAnimation()
    {
        Debug.Log("Current" + CurrentWinCon.Type);
        switch(CurrentWinCon.Type)
        {
            case WinConType.RingLeader:
                RingLeader.Activate();
                break;
            case WinConType.Survive:
                SurviveManager.Activate();
                break;
            case WinConType.FloorIsLava:
                LavaFloor.Activate();
                break;
            default:
                break;
        }
    }
}
