using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enviro;
using Unity.XR.CoreUtils;

public class WeatherChangeScene : MonoBehaviour
{
    private BoxCollider triggerZone;
    private EnviroManager enviroManager;

    void Start()
    {
        triggerZone = GetComponent<BoxCollider>();
        if (triggerZone != null)
        {
            triggerZone.isTrigger = true;
        }
    
        enviroManager = EnviroManager.instance;
        
        // Set weather and time as soon as scene loads
        if (enviroManager != null)
        {
            try
            {
                enviroManager.Weather.ChangeWeather("Cloudy 1");
                enviroManager.Time.SetTimeOfDay(9f);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to set weather: " + e.Message);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //for future reference if I want to trigger something when the player enters the trigger zone
    }

    void Update()
    {
        
    }

    public void SetCloudyState(bool isCloudy)
    {
        
    }
}
