using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;

public class Respawnfall : MonoBehaviour
{
    [SerializeField] private float minimumHeight = -10f;
    
    [Header("References")]
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Camera mainCamera;
    
    [Header("Spawn Position")]
    [SerializeField] private bool useSceneStartPosition = true;
    [SerializeField] private Transform customSpawnPoint;
    
    [Header("Fade Transition")]
    [SerializeField] private bool useSceneTransitionManager = true;
    [SerializeField] private FadeScreen customFadeScreen;
    [SerializeField] private float respawnDelay = 0.2f;
    
    private bool isRespawning = false;
    private SceneTransitionManager transitionManager;
    private FadeScreen fadeScreen;
    private Vector3 initialXROriginPosition;
    private Quaternion initialXROriginRotation;
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private Vector3 initialCameraOffset;
    private Transform cameraTransform;
    
    void Awake()
    {
        InitializePositions();
    }
    
    private void InitializePositions()
    {
        if (xrOrigin == null)
        {
            xrOrigin = FindObjectOfType<XROrigin>()?.transform;
            
            if (xrOrigin == null)
            {
                return;
            }
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                return;
            }
        }
        
        cameraTransform = mainCamera.transform;
        initialXROriginPosition = xrOrigin.position;
        initialXROriginRotation = xrOrigin.rotation;
        initialCameraPosition = cameraTransform.position;
        initialCameraRotation = cameraTransform.rotation;
        initialCameraOffset = initialCameraPosition - initialXROriginPosition;
    }
    
    void Start()
    {
        if (useSceneTransitionManager)
        {
            transitionManager = SceneTransitionManager.singleton;
            if (transitionManager != null)
            {
                fadeScreen = transitionManager.fadeScreen;
            }
            else
            {
                useSceneTransitionManager = false;
            }
        }
        
        if (!useSceneTransitionManager && customFadeScreen != null)
        {
            fadeScreen = customFadeScreen;
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializePositions();
    }
    
    void Update()
    {
        if (mainCamera != null && cameraTransform.position.y < minimumHeight && !isRespawning)
        {
            StartCoroutine(RespawnPlayerWithFade());
        }
    }
    
    IEnumerator RespawnPlayerWithFade()
    {
        isRespawning = true;
        
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            yield return new WaitForSeconds(fadeScreen.fadeDuration + respawnDelay);
            TeleportPlayer();
            yield return new WaitForFixedUpdate();
            fadeScreen.FadeIn();
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }
        else
        {
            TeleportPlayer();
            yield return new WaitForFixedUpdate();
        }
        
        isRespawning = false;
    }
    
    void TeleportPlayer()
    {
        if (xrOrigin == null || mainCamera == null) return;
        
        Vector3 targetPosition;
        Quaternion targetRotation;
        
        if (useSceneStartPosition)
        {
            targetPosition = initialXROriginPosition;
            targetRotation = initialXROriginRotation;
        }
        else if (customSpawnPoint != null)
        {
            targetPosition = customSpawnPoint.position - initialCameraOffset;
            targetRotation = customSpawnPoint.rotation;
        }
        else
        {
            targetPosition = initialXROriginPosition;
            targetRotation = initialXROriginRotation;
        }
        
        xrOrigin.position = targetPosition;
        xrOrigin.rotation = targetRotation;
        
        if (useSceneStartPosition && cameraTransform != null)
        {
            try
            {
                cameraTransform.position = initialCameraPosition;
                cameraTransform.rotation = initialCameraRotation;
            }
            catch
            {
            }
        }
        
        Rigidbody rb = xrOrigin.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        foreach (Rigidbody childRb in xrOrigin.GetComponentsInChildren<Rigidbody>())
        {
            if (childRb != rb)
            {
                childRb.velocity = Vector3.zero;
                childRb.angularVelocity = Vector3.zero;
            }
        }
    }
}
