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
                _instance = new GameManager();
            
            

            return _instance;
        }
    }

    public int CurrentLives;
}
