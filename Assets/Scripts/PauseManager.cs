using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private Dictionary<string, float> pauseDict = new();

    public static PauseManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void SetPause(string pauser, float value)
    {
        pauseDict[pauser] = value;

        PauseTime();
    }

    public float? GetPause(string pauser)
    {
        if (!pauseDict.ContainsKey(pauser)) return null;

        return pauseDict[pauser];
    }


    void PauseTime()
    {
        float resValue = 1;

        foreach (var (_, value) in pauseDict)
        {
            resValue *= value;
        }

        Time.timeScale = resValue;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
