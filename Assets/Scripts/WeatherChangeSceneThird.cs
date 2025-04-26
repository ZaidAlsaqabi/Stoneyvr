using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enviro;
using Unity.XR.CoreUtils;

public class WeatherChangeSceneThird : MonoBehaviour
{
    private EnviroManager enviroManager;

    void Start()
    {
        enviroManager = EnviroManager.instance;
        
        // Set weather and time as soon as scene loads
        if (enviroManager != null)
        {
            try
            {
                enviroManager.Weather.ChangeWeather("Cloudy 1");
                enviroManager.Time.SetTimeOfDay(9f);
                Debug.Log("Weather set to Cloudy 1, time set to 9:00");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to set weather: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("EnviroManager instance not found");
        }
    }
} 