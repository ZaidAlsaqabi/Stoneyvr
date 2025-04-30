using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enviro;
using Unity.XR.CoreUtils;

public class WeatherChangeSceneRain : MonoBehaviour
{
    private EnviroManager enviroManager;

    void Start()
    {
        enviroManager = EnviroManager.instance;
        
   
        if (enviroManager != null)
        {
            try
            {
                enviroManager.Weather.ChangeWeather("Rain");
                enviroManager.Time.SetTimeOfDay(1f);
       
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