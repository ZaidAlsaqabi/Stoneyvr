using UnityEngine;
using TMPro;
using System;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;

public class StonehengeTimeWeather : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI weatherText;
    [SerializeField] private TextMeshProUGUI temperatureText;
    
    [Header("Settings")]
    [SerializeField] private string timeFormat = "h:mm tt";
    [SerializeField] private float updateInterval = 30f;
    
    [Header("API Settings")]
    [SerializeField] private string apiKey = "dbda820a29ec419186a142047252204";
    
    private const float STONEHENGE_LAT = 51.1789f;
    private const float STONEHENGE_LON = -1.8262f;
    
    private const string LONDON_TIME_ZONE = "GMT Standard Time";
    
    private float timer;
    private TimeZoneInfo ukTimeZone;
    private bool useSystemTimeAsBackup = false;
    
    private void Start()
    {
        if (timeText == null || weatherText == null || temperatureText == null)
        {
        }
        
        try 
        {
            ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById(LONDON_TIME_ZONE);
        }
        catch (Exception e)
        {
            try
            {
                ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
            }
            catch
            {
                useSystemTimeAsBackup = true;
            }
        }
        
        UpdateTimeDisplay();
        
        StartCoroutine(UpdateWeatherDisplay());
    }
    
    private void Update()
    {
        UpdateTimeDisplay();
        
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            StartCoroutine(UpdateWeatherDisplay());
            timer = 0f;
        }
    }
    
    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            DateTime stonehengeTime;
            
            if (!useSystemTimeAsBackup && ukTimeZone != null)
            {
                stonehengeTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ukTimeZone);
            }
            else
            {
                stonehengeTime = DateTime.Now;
            }
            
            timeText.text = $"Stonehenge Time: {stonehengeTime.ToString(timeFormat)}";
        }
    }
    
    private IEnumerator UpdateWeatherDisplay()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            if (weatherText != null) weatherText.text = "Weather: API Key Required";
            if (temperatureText != null) temperatureText.text = "";
            yield break;
        }
        
        string url = $"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={STONEHENGE_LAT},{STONEHENGE_LON}&aqi=no";
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                if (weatherText != null) weatherText.text = "Weather: Connection Error";
                if (temperatureText != null) temperatureText.text = "";
            }
            else
            {
                try
                {
                    JSONNode weatherData = JSON.Parse(webRequest.downloadHandler.text);
                    
                    if (weatherData != null)
                    {
                        string weatherDescription = weatherData["current"]["condition"]["text"].Value;
                        float temperature = weatherData["current"]["temp_c"].AsFloat;
                        
                        if (weatherText != null) weatherText.text = $"Stonehenge Weather: {weatherDescription}";
                        if (temperatureText != null) temperatureText.text = $"Temperature: {temperature:F1}°C";
                    }
                }
                catch (Exception e)
                {
                    if (weatherText != null) weatherText.text = "Weather: Data Error";
                    if (temperatureText != null) temperatureText.text = "";
                }
            }
        }
    }
} 