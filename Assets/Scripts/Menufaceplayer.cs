using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menufaceplayer : MonoBehaviour
{
    [Tooltip("If true, the menu will only rotate around the Y axis to face the player")]
    public bool billboardY = true;
    
    [Tooltip("How quickly the menu follows the player (higher = faster)")]
    public float followSpeed = 3.0f;
    
    [Tooltip("Distance at which the menu becomes visible")]
    public float visibilityDistance = 20.0f;
    
    [Tooltip("How quickly the menu fades in/out")]
    public float fadeSpeed = 3.0f;
    
    private Transform cameraTransform;
    
    private CanvasGroup canvasGroup;
    
    private float targetAlpha = 2.0f;
    
    private CanvasGroup[] allCanvasGroups;
    
    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            var anyCamera = FindObjectOfType<Camera>();
            if (anyCamera != null)
            {
                cameraTransform = anyCamera.transform;
            }
            else
            {
                Debug.LogWarning("No camera found in the scene!");
            }
        }
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        allCanvasGroups = GetComponentsInChildren<CanvasGroup>(true);
        
        if (allCanvasGroups.Length > 1)
        {
            foreach (CanvasGroup childGroup in allCanvasGroups)
            {
                if (childGroup != canvasGroup)
                {
                    childGroup.alpha = 1.0f;
                }
            }
        }
    }

    void Update()
    {
        if (cameraTransform == null)
            return;
        
        float distanceToCamera = Vector3.Distance(transform.position, cameraTransform.position);
        
        targetAlpha = distanceToCamera <= visibilityDistance ? 1.0f : 0.0f;
        
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        
        canvasGroup.interactable = canvasGroup.alpha > 0.5f;
        canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.5f;
        
        if (canvasGroup.alpha > 0.01f)
        {
            if (billboardY)
            {
                Vector3 directionToCamera = cameraTransform.position - transform.position;
                directionToCamera.y = 0;
                
                if (directionToCamera != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
                    
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
                }
            }
            else
            {
                Vector3 directionToCamera = cameraTransform.position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
                
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
            }
        }
    }
}