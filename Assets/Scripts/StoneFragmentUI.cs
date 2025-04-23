using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StoneFragmentUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoneFragmentManager fragmentManager;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private Image progressFill;
    [SerializeField] private GameObject completionPanel;
    
    [Header("Settings")]
    [SerializeField] private string counterFormat = "{0}/{1} Fragments";
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private float animationDuration = 0.5f;
    
    private int displayedCount = 0;
    private float displayedFillAmount = 0f;
    private float timer = 0f;
    
    void Start()
    {
        if (fragmentManager == null)
        {
            fragmentManager = FindObjectOfType<StoneFragmentManager>();
        }
        
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
            UpdateUI(true);
        

        if (fragmentManager != null)
        {
            fragmentManager.onAllFragmentsCollected.AddListener(OnAllFragmentsCollected);
        }
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateUI();
        }
    }
    
    private void UpdateUI(bool instantUpdate = false)
    {
        if (fragmentManager == null) return;
        
        int collectedCount = fragmentManager.GetCollectedCount();
        int totalCount = fragmentManager.GetTotalCount();
        
        if (counterText != null)
        {
            counterText.text = string.Format(counterFormat, collectedCount, totalCount);
        }
        
        if (progressFill != null)
        {
            float targetFill = totalCount > 0 ? (float)collectedCount / totalCount : 0f;
            
            if (instantUpdate)
            {
                progressFill.fillAmount = targetFill;
                displayedFillAmount = targetFill;
            }
            else if (displayedFillAmount != targetFill)
            {
                StopAllCoroutines();
                StartCoroutine(AnimateFillBar(displayedFillAmount, targetFill));
            }
        }
        

        displayedCount = collectedCount;
    }
    
    private IEnumerator AnimateFillBar(float startFill, float endFill)
    {
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / animationDuration;
            
            float currentFill = Mathf.Lerp(startFill, endFill, normalizedTime);
            progressFill.fillAmount = currentFill;
            displayedFillAmount = currentFill;
            
            yield return null;
        }
        
        progressFill.fillAmount = endFill;
        displayedFillAmount = endFill;
    }
    
    private void OnAllFragmentsCollected()
    {
        UpdateUI(true);
        
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
        }
    }
} 