using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OutlineSelection : MonoBehaviour
{
    [Header("XR Ray Source")]
    [Tooltip("The XR controller that will cast the ray")]
    public XRRayInteractor rayInteractor;

    [Header("Outline Settings")]
    [Range(0.1f, 10f)]
    public float outlineWidth = 7.0f;
    public Color hoverOutlineColor = Color.magenta;
    
    [Header("Animation Settings")]
    [Range(1f, 10f)]
    public float outlineAnimationSpeed = 5f;
    [Range(0.01f, 0.5f)]
    public float animationThreshold = 0.05f;

    private Transform currentHoverObject;
    private RaycastHit raycastHit;
    private Dictionary<Transform, Outline> outlineComponents = new Dictionary<Transform, Outline>();
    private Dictionary<Transform, Coroutine> activeCoroutines = new Dictionary<Transform, Coroutine>();

    void Start()
    {
        
        if (rayInteractor == null)
        {
            rayInteractor = FindObjectOfType<XRRayInteractor>();
            if (rayInteractor == null)
            {
                Debug.LogError("not assign xray interactor");
            }
        }
    }

    void Update()
    {
        Transform hitObject = null;
        
        if (rayInteractor != null && rayInteractor.TryGetCurrent3DRaycastHit(out raycastHit))
        {
            hitObject = raycastHit.transform;
            
            if (hitObject != null && hitObject.CompareTag("Selectable"))
            {
                if (hitObject != currentHoverObject)
                {
                    if (currentHoverObject != null)
                    {
                        StartOutlineAnimation(currentHoverObject, false);
                    }
                    
                    currentHoverObject = hitObject;
                    
                    if (!outlineComponents.ContainsKey(currentHoverObject))
                    {
                        Outline outline = currentHoverObject.GetComponent<Outline>();
                        if (outline == null)
                        {
                            outline = currentHoverObject.gameObject.AddComponent<Outline>();
                            outline.OutlineWidth = 0;
                        }
                        outline.OutlineColor = hoverOutlineColor;
                        outlineComponents[currentHoverObject] = outline;
                    }
                    
                    StartOutlineAnimation(currentHoverObject, true);
                }
            }
        }
        
        if (hitObject == null && currentHoverObject != null)
        {
            StartOutlineAnimation(currentHoverObject, false);
            currentHoverObject = null;
        }
    }

    private void StartOutlineAnimation(Transform targetObject, bool showOutline)
    {
        if (activeCoroutines.ContainsKey(targetObject))
        {
            StopCoroutine(activeCoroutines[targetObject]);
            activeCoroutines.Remove(targetObject);
        }
        
        if (outlineComponents.ContainsKey(targetObject))
        {
            Coroutine animationCoroutine = StartCoroutine(SmoothOutlineAnimation(targetObject, showOutline));
            activeCoroutines[targetObject] = animationCoroutine;
        }
    }

    private IEnumerator SmoothOutlineAnimation(Transform targetObject, bool showOutline)
    {
        if (!outlineComponents.ContainsKey(targetObject))
            yield break;
            
        Outline outline = outlineComponents[targetObject];
        outline.enabled = true;
        
        float startWidth = outline.OutlineWidth;
        float targetWidth = showOutline ? outlineWidth : 0;
        
        if (Mathf.Abs(startWidth - targetWidth) < animationThreshold)
        {
            outline.OutlineWidth = targetWidth;
            
            if (!showOutline)
            {
                outline.enabled = false;
            }
            
            if (activeCoroutines.ContainsKey(targetObject))
            {
                activeCoroutines.Remove(targetObject);
            }
            
            yield break;
        }
        
        float time = 0;
        
        while (time < 1)
        {
            time += Time.deltaTime * outlineAnimationSpeed;
            outline.OutlineWidth = Mathf.Lerp(startWidth, targetWidth, Mathf.SmoothStep(0, 1, time));
            yield return null;
        }
        
        outline.OutlineWidth = targetWidth;
        
        if (!showOutline)
        {
            outline.enabled = false;
        }
        
        if (activeCoroutines.ContainsKey(targetObject))
        {
            activeCoroutines.Remove(targetObject);
        }
    }
}
