using UnityEngine;
using TMPro;
using System;

public class TimeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private string timeFormat = "HH:mm";
    [SerializeField] private float updateInterval = 0.5f;

    private float timer;

    private void Start()
    {
        if (timeText == null)
        {
            timeText = GetComponent<TextMeshProUGUI>();
        }

        UpdateTimeDisplay();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            UpdateTimeDisplay();
            timer = 0f;
        }
    }

    private void UpdateTimeDisplay()
    {
        if (timeText == null)
        {
            return;
        }

        DateTime currentTime = DateTime.Now;
        timeText.text = currentTime.ToString(timeFormat);
    }
} 