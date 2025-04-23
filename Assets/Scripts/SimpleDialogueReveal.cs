using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using System.Collections.Generic;

public class SimpleDialogueReveal : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public float detectionRadius = 3f;
    public AudioClip introDialogueAudio;
    public AudioClip endingDialogueAudio;
    
    [Header("Objects to Reveal")]
    public GameObject[] stoneFragments;
    public GameObject[] canvasUI;
    public GameObject[] vfxEffects;
    
    [Header("Collection Settings")]
    public int totalFragmentsNeeded = 5;
    public AudioClip collectSound;
    
    [Header("NPC Animation Settings")]
    [Tooltip("The Animator component on the NPC")]
    public Animator npcAnimator;
    [Tooltip("Animation trigger parameter to use when all fragments are collected")]
    public string fragmentsCollectedAnimTrigger = "AllFragmentsCollected";
    [Tooltip("Bool parameter to set when all fragments are collected")]
    public string fragmentsCollectedAnimBool = "HasAllFragments";
    [Tooltip("Float parameter to set with the number of collected fragments (optional)")]
    public string fragmentCountAnimFloat = "FragmentCount";
    [Tooltip("Animation trigger to play when player approaches with fragments")]
    public string approachWithFragmentsAnimTrigger = "PlayerApproaching";
    
    [Header("Fragment-Specific Voice Lines")]
    [Tooltip("Voice lines to play in order of collection (first, second, third, etc.)")]
    public AudioClip[] fragmentVoiceLines;
    [Tooltip("Delay before playing the voice line after collecting the fragment (seconds)")]
    public float voiceLineDelay = 0.5f;
    
    [Header("Fragment-VFX Mapping")]
    [Tooltip("Should the VFX at the same index as the stone fragment be turned off when collected?")]
    public bool disableMatchingVFX = true;
    
    [Header("Player Settings")]
    public Transform playerRig;
    public ActionBasedContinuousMoveProvider moveProvider; 
    public ActionBasedSnapTurnProvider turnProvider; 
    [Tooltip("Allow the player to drop objects during the ending dialogue")]
    public bool allowDroppingDuringDialogue = true;
    
    [Header("Transition Settings")]
    public FadeScreen fadeScreen;
    public float fadeOutDuration = 1.0f;
    public float fadeInDuration = 1.0f;
    
    [Header("Audio Settings")]
    [Tooltip("Ambient crying sound to attract players")]
    public AudioClip cryingSound;
    [Tooltip("Radius at which crying sound can be heard (should be larger than detection radius)")]
    public float cryingSoundRadius = 15f;
    [Tooltip("Minimum volume for crying sound when player is far away")]
    [Range(0f, 1f)]
    public float minCryingVolume = 0.1f;
    [Tooltip("Maximum volume for crying sound when player is near")]
    [Range(0f, 1f)]
    public float maxCryingVolume = 0.7f;
    [Tooltip("How quickly the crying sound fades in/out based on distance")]
    public float cryingFadeSpeed = 2f;
    
    private AudioSource audioSource;
    private AudioSource voiceLineAudioSource;   
    private AudioSource cryingAudioSource; 
    private bool hasPlayedIntroDialogue = false;
    private bool hasPlayedEndingDialogue = false;
    private bool objectsRevealed = false;
    private int collectedFragments = 0;
    private bool hasSetFragmentsCollectedAnimation = false;
    
  
    private LocomotionProvider[] allLocomotionProviders;
    private TeleportationProvider teleportProvider;

    private List<XRBaseInteractable> grabbedObjects = new List<XRBaseInteractable>();
    

    private Dictionary<Rigidbody, bool> originalKinematicStates = new Dictionary<Rigidbody, bool>();
    private Dictionary<XRGrabInteractable, bool> originalThrowOnDetachStates = new Dictionary<XRGrabInteractable, bool>();
    
  
    private Dictionary<IXRSelectInteractor, XRBaseInteractable> interactorSelections = new Dictionary<IXRSelectInteractor, XRBaseInteractable>();
    
    private Dictionary<XRBaseInteractable, Transform> originalParents = new Dictionary<XRBaseInteractable, Transform>();
    private Dictionary<XRBaseInteractable, Vector3> originalLocalPositions = new Dictionary<XRBaseInteractable, Vector3>();
    private Dictionary<XRBaseInteractable, Quaternion> originalLocalRotations = new Dictionary<XRBaseInteractable, Quaternion>();
    
    private Dictionary<XRGrabInteractable, XRBaseInteractable.MovementType> originalMovementTypes = 
        new Dictionary<XRGrabInteractable, XRBaseInteractable.MovementType>();
    
    private bool isInEndingDialogue = false;
    
    void Start()
    {
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("Added main AudioSource to " + gameObject.name);
        }
        else
        {
            Debug.Log("Found existing AudioSource on " + gameObject.name);
        }
        
        voiceLineAudioSource = gameObject.AddComponent<AudioSource>();
        voiceLineAudioSource.playOnAwake = false;
        voiceLineAudioSource.volume = audioSource.volume;
        Debug.Log("Created voice line AudioSource with volume: " + voiceLineAudioSource.volume);
        
        cryingAudioSource = gameObject.AddComponent<AudioSource>();
        cryingAudioSource.clip = cryingSound;
        cryingAudioSource.loop = true;
        cryingAudioSource.volume = minCryingVolume;
        cryingAudioSource.spatialBlend = 1f; 
        cryingAudioSource.minDistance = 1f;
        cryingAudioSource.maxDistance = cryingSoundRadius; 
        cryingAudioSource.rolloffMode = AudioRolloffMode.Linear; 
        cryingAudioSource.dopplerLevel = 0f; 
        
        if (cryingSound != null)
        {
            cryingAudioSource.Play();
            Debug.Log("Started crying sound with spatial blending");
        }
        else
        {
            Debug.LogWarning("No crying sound assigned to attract players");
        }
        
        if (fragmentVoiceLines != null)
        {
            Debug.Log("Fragment voice lines array length: " + fragmentVoiceLines.Length);
            for (int i = 0; i < fragmentVoiceLines.Length; i++)
            {
                if (fragmentVoiceLines[i] != null)
                    Debug.Log("Voice line " + i + ": " + fragmentVoiceLines[i].name);
                else
                    Debug.LogWarning("Voice line " + i + " is null!");
            }
        }
        else
        {
            Debug.LogWarning("Fragment voice lines array is null!");
        }
        
        if (moveProvider == null)
        {
            moveProvider = FindObjectOfType<ActionBasedContinuousMoveProvider>();
        }
        
        if (turnProvider == null)
        {
            turnProvider = FindObjectOfType<ActionBasedSnapTurnProvider>();
        }
        
        allLocomotionProviders = FindObjectsOfType<LocomotionProvider>();
        teleportProvider = FindObjectOfType<TeleportationProvider>();
        
        if (playerRig == null)
        {
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                playerRig = xrOrigin.transform;
            }
        }
        
        if (fadeScreen == null)
        {
            fadeScreen = FindObjectOfType<FadeScreen>();
        }
        
        if (npcAnimator == null)
        {
            npcAnimator = GetComponent<Animator>();
        }
        
        if (vfxEffects.Length != stoneFragments.Length && disableMatchingVFX)
        {
            Debug.LogWarning("Stone fragments and VFX effects arrays have different lengths. Make sure they match for proper VFX disabling.");
        }
        
        if (fragmentVoiceLines.Length != stoneFragments.Length)
        {
            Debug.LogWarning("Stone fragments and voice lines arrays have different lengths. Some fragments might not have voice lines.");
        }
        
        
        HideAllObjects();
        
        SetupStoneFragments();
        
        SetupGrabTracking();
        
        UpdateAnimationParameters();
    }
    
    private void UpdateAnimationParameters()
    {
        if (npcAnimator != null)
        {
            if (!string.IsNullOrEmpty(fragmentCountAnimFloat) && HasAnimatorParameter(fragmentCountAnimFloat))
            {
                npcAnimator.SetFloat(fragmentCountAnimFloat, collectedFragments);
            }
            
            if (AllFragmentsCollected() && !hasSetFragmentsCollectedAnimation)
            {
                if (!string.IsNullOrEmpty(fragmentsCollectedAnimBool) && HasAnimatorParameter(fragmentsCollectedAnimBool))
                {
                    npcAnimator.SetBool(fragmentsCollectedAnimBool, true);
                }
                
                if (!string.IsNullOrEmpty(fragmentsCollectedAnimTrigger) && HasAnimatorParameter(fragmentsCollectedAnimTrigger))
                {
                    npcAnimator.SetTrigger(fragmentsCollectedAnimTrigger);
                }
                
                hasSetFragmentsCollectedAnimation = true;
            }
        }
    }
    
    private bool HasAnimatorParameter(string paramName)
    {
        if (npcAnimator == null)
            return false;
            
        foreach (AnimatorControllerParameter param in npcAnimator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    
    void SetupGrabTracking()
    {
        XRBaseInteractor[] interactors = FindObjectsOfType<XRBaseInteractor>();
        foreach (var interactor in interactors)
        {
            if (interactor != null)
            {
                IXRSelectInteractor selectInteractor = interactor as IXRSelectInteractor;
                if (selectInteractor != null)
                {
                    interactorSelections[selectInteractor] = null;
                
                    interactor.selectEntered.AddListener((SelectEnterEventArgs args) => {
                        XRBaseInteractable interactable = args.interactableObject as XRBaseInteractable;
                        IXRSelectInteractor interactorInterface = args.interactorObject as IXRSelectInteractor;
                        
                        if (interactable != null && interactorInterface != null && !grabbedObjects.Contains(interactable))
                        {
                            grabbedObjects.Add(interactable);
                            
                            interactorSelections[interactorInterface] = interactable;
                            
                            Rigidbody rb = interactable.GetComponent<Rigidbody>();
                            if (rb != null && !originalKinematicStates.ContainsKey(rb))
                            {
                                originalKinematicStates[rb] = rb.isKinematic;
                            }
                            
                            XRGrabInteractable grabInteractable = interactable as XRGrabInteractable;
                            if (grabInteractable != null && !originalThrowOnDetachStates.ContainsKey(grabInteractable))
                            {
                                originalThrowOnDetachStates[grabInteractable] = grabInteractable.throwOnDetach;
                            }
                            
                            if (!originalParents.ContainsKey(interactable))
                            {
                                originalParents[interactable] = interactable.transform.parent;
                                originalLocalPositions[interactable] = interactable.transform.localPosition;
                                originalLocalRotations[interactable] = interactable.transform.localRotation;
                            }
                        }
                    });
                    
                    interactor.selectExited.AddListener((SelectExitEventArgs args) => {
                        XRBaseInteractable interactable = args.interactableObject as XRBaseInteractable;
                        IXRSelectInteractor interactorInterface = args.interactorObject as IXRSelectInteractor;
                        
                        if (interactable != null && interactorInterface != null)
                        {
                            if (isInEndingDialogue && allowDroppingDuringDialogue)
                            {
                                Debug.Log("Player dropped object during dialogue: " + interactable.name);
                            }
                            
                            grabbedObjects.Remove(interactable);
                            
                            if (interactorSelections.ContainsKey(interactorInterface))
                            {
                                interactorSelections[interactorInterface] = null;
                            }
                        }
                    });
                }
            }
        }
    }
    
    void Update()
    {
        UpdateCryingSoundVolume();
        
        bool playerIsNear = IsPlayerNearby();
        
        if (playerIsNear)
        {
            if (!hasPlayedIntroDialogue)
            {
                StartIntroDialogue();
            }
            else if (AllFragmentsCollected() && !hasPlayedEndingDialogue && objectsRevealed)
            {
                if (npcAnimator != null && !string.IsNullOrEmpty(approachWithFragmentsAnimTrigger) 
                    && HasAnimatorParameter(approachWithFragmentsAnimTrigger))
                {
                    npcAnimator.SetTrigger(approachWithFragmentsAnimTrigger);
                }
                
                StartEndingDialogue();
            }
        }
    }
    
    void SetupStoneFragments()
    {
        foreach (GameObject fragment in stoneFragments)
        {
            if (fragment != null)
            {
                if (fragment.GetComponent<Collider>() == null)
                {
                    fragment.AddComponent<BoxCollider>();
                }
                
                Collider collider = fragment.GetComponent<Collider>();
                collider.isTrigger = false;
                
                if (fragment.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = fragment.AddComponent<Rigidbody>();
                    rb.isKinematic = false; 
                    rb.useGravity = true;
                    rb.mass = 1f; 
                    rb.drag = 0.5f; 
                }
                
                XRGrabInteractable grabInteractable = fragment.GetComponent<XRGrabInteractable>();
                if (grabInteractable == null)
                {
                    grabInteractable = fragment.AddComponent<XRGrabInteractable>();
                    grabInteractable.throwOnDetach = true;
                    grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                    grabInteractable.trackPosition = true;
                    grabInteractable.trackRotation = true;
                    grabInteractable.throwSmoothingDuration = 0.2f;
                    grabInteractable.throwSmoothingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                }
                
                if (fragment.GetComponent<AudioSource>() == null && collectSound != null)
                {
                    AudioSource fragmentAudio = fragment.AddComponent<AudioSource>();
                    fragmentAudio.playOnAwake = false;
                    fragmentAudio.clip = collectSound;
                }
                
                grabInteractable.selectEntered.AddListener((SelectEnterEventArgs args) => {
                    FragmentCollected(fragment);
                });
                
                RemoveComponentByName(fragment, "Outline");
                
                RemoveComponentsByPartialName(fragment, new string[] { "outline", "hover", "highlight", "glow" });
            }
        }
    }
    
    private void RemoveComponentByName(GameObject obj, string componentName)
    {
        Component[] components = obj.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (component != null && component.GetType().Name.Equals(componentName, StringComparison.OrdinalIgnoreCase))
            {
                Destroy(component);
            }
        }
    }
    
    private void RemoveComponentsByPartialName(GameObject obj, string[] namesToCheck)
    {
        Component[] components = obj.GetComponents<MonoBehaviour>();
        foreach (Component component in components)
        {
            if (component == null) continue;
            
            string typeName = component.GetType().Name.ToLower();
            foreach (string nameToCheck in namesToCheck)
            {
                if (typeName.Contains(nameToCheck.ToLower()))
                {
                    Destroy(component);
                    break;
                }
            }
        }
    }
    
    bool IsPlayerNearby()
    {
        if (Camera.main != null)
        {
            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            return distance <= detectionRadius;
        }
        return false;
    }
    
    void StartIntroDialogue()
    {
        if (introDialogueAudio != null && !hasPlayedIntroDialogue)
        {
            hasPlayedIntroDialogue = true;
            StartCoroutine(PlayIntroDialogueAndReveal());
        }
    }
    
    void StartEndingDialogue()
    {
        if (endingDialogueAudio != null && !hasPlayedEndingDialogue)
        {
            hasPlayedEndingDialogue = true;
            
            isInEndingDialogue = true;
            
            StabilizeHeldItems();
            
            StartCoroutine(PlayEndingDialogueAndTransition());
        }
    }
    
    void StabilizeHeldItems()
    {
        try
        {
            foreach (var pair in interactorSelections)
            {
                IXRSelectInteractor interactor = pair.Key;
                XRBaseInteractable interactable = pair.Value;
                
                if (interactor != null && interactable != null)
                {
                    bool isFlashlight = interactable.gameObject.name.ToLower().Contains("flash") || 
                                        interactable.gameObject.name.ToLower().Contains("torch");
                    
                    Rigidbody rb = interactable.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.drag = 5f;
                        rb.angularDrag = 5f;
                        
                        if (isFlashlight)
                        {
                            Debug.Log("Stabilizing flashlight: " + interactable.name);
                            rb.drag = 10f;
                            rb.angularDrag = 10f;
                        }
                    }
                    
                    XRGrabInteractable grabInteractable = interactable as XRGrabInteractable;
                    if (grabInteractable != null)
                    {
                        if (!originalMovementTypes.ContainsKey(grabInteractable))
                        {
                            originalMovementTypes[grabInteractable] = grabInteractable.movementType;
                        }
                        
                        grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
                        
                        grabInteractable.trackPosition = true;
                        grabInteractable.trackRotation = true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error stabilizing objects: " + ex.Message);
        }
    }
    
    IEnumerator PlayIntroDialogueAndReveal()
    {
        DisablePlayerMovement();
        
        if (playerRig != null)
        {
            Vector3 dirToPlayer = playerRig.position - transform.position;
            dirToPlayer.y = 0;
            
            if (dirToPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(-dirToPlayer); 
                playerRig.rotation = Quaternion.Slerp(playerRig.rotation, lookRotation, 0.9f);
            }
        }
        
        if (cryingAudioSource != null && cryingAudioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(cryingAudioSource, 1.0f));
        }
        
        
        audioSource.clip = introDialogueAudio;
        audioSource.Play();
        
        yield return new WaitForSeconds(introDialogueAudio.length);
        
        EnablePlayerMovement();
        
        RevealAllObjects();
    }
    
    IEnumerator PlayEndingDialogueAndTransition()
    {
        DisablePlayerMovement();
        
        if (playerRig != null)
        {
            Vector3 dirToPlayer = playerRig.position - transform.position;
            dirToPlayer.y = 0;
            
            if (dirToPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(-dirToPlayer); // Negative to face towards NPC
                playerRig.rotation = Quaternion.Slerp(playerRig.rotation, lookRotation, 0.9f);
            }
        }
        
        audioSource.clip = endingDialogueAudio;
        audioSource.Play();
        
        yield return new WaitForSeconds(endingDialogueAudio.length);
        
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
            yield return new WaitForSeconds(fadeOutDuration);
        }
        
        // Transition to starting scene (scene 0)
        SceneManager.LoadScene(0);
    }
    
    private void PlayFragmentVoiceLine(int collectionOrder)
    {
        Debug.Log("PlayFragmentVoiceLine called with index: " + collectionOrder);
        StartCoroutine(PlayVoiceLineWithDelay(collectionOrder));
    }
    
    private IEnumerator PlayVoiceLineWithDelay(int collectionOrder)
    {
        Debug.Log("Waiting " + voiceLineDelay + " seconds before playing voice line");
        yield return new WaitForSeconds(voiceLineDelay);
        
        if (fragmentVoiceLines != null && collectionOrder < fragmentVoiceLines.Length && collectionOrder >= 0)
        {
            if (fragmentVoiceLines[collectionOrder] != null)
            {
                Debug.Log("Playing voice line: " + fragmentVoiceLines[collectionOrder].name);
                
                if (voiceLineAudioSource.isPlaying)
                {
                    voiceLineAudioSource.Stop();
                    Debug.Log("Stopped previous voice line audio");
                }
                
                Debug.Log("Voice line audio source settings - Volume: " + voiceLineAudioSource.volume + 
                          ", Mute: " + voiceLineAudioSource.mute + 
                          ", Pitch: " + voiceLineAudioSource.pitch);
                
                voiceLineAudioSource.clip = fragmentVoiceLines[collectionOrder];
                voiceLineAudioSource.Play();
                
                yield return new WaitForSeconds(0.5f);
                if (voiceLineAudioSource.isPlaying)
                {
                    Debug.Log("Voice line is playing successfully");
                    
                    float clipLength = fragmentVoiceLines[collectionOrder].length;
                    Debug.Log("Voice line duration: " + clipLength + " seconds");
                    yield return new WaitForSeconds(clipLength);
                }
                else
                {
                    Debug.LogWarning("Voice line failed to play! Clip assigned: " + 
                                    (voiceLineAudioSource.clip != null ? voiceLineAudioSource.clip.name : "null"));
                }
            }
            else
            {
                Debug.LogWarning("Voice line at index " + collectionOrder + " is null!");
            }
        }
        else
        {
            string details = "Array Length: " + (fragmentVoiceLines != null ? fragmentVoiceLines.Length.ToString() : "null array") +
                             ", Requested Index: " + collectionOrder;
            Debug.LogWarning("No valid voice line found for index " + collectionOrder + ". " + details);
        }
    }
    
    void DisablePlayerMovement()
    {
        try
        {
            
            
            foreach (var provider in allLocomotionProviders)
            {
                if (provider != null)
                {
                    provider.enabled = false;
                }
            }
            
            
            if (teleportProvider != null)
            {
                teleportProvider.enabled = false;
            }
            
            
            if (moveProvider != null)
            {
                
                if (moveProvider.leftHandMoveAction.action != null && moveProvider.leftHandMoveAction.action.enabled)
                    moveProvider.leftHandMoveAction.action.Disable();
                
                if (moveProvider.rightHandMoveAction.action != null && moveProvider.rightHandMoveAction.action.enabled)
                    moveProvider.rightHandMoveAction.action.Disable();
            }
            
            
            if (turnProvider != null)
            {
                if (turnProvider.leftHandSnapTurnAction.action != null && turnProvider.leftHandSnapTurnAction.action.enabled)
                    turnProvider.leftHandSnapTurnAction.action.Disable();
                
                if (turnProvider.rightHandSnapTurnAction.action != null && turnProvider.rightHandSnapTurnAction.action.enabled)
                    turnProvider.rightHandSnapTurnAction.action.Disable();
            }
            
            
            TeleportationAnchor[] teleportAnchors = FindObjectsOfType<TeleportationAnchor>();
            foreach (var anchor in teleportAnchors)
            {
                if (anchor != null)
                    anchor.enabled = false;
            }
            
            XRRayInteractor[] rayInteractors = FindObjectsOfType<XRRayInteractor>();
            foreach (var interactor in rayInteractors)
            {
                var teleportInteractor = interactor.GetComponent<TeleportationAnchor>();
                var teleportationRay = interactor.GetComponent<XRRayInteractor>();
                
                if (teleportInteractor != null || teleportationRay != null)
                {
                    interactor.enabled = false;
                }
            }
            
            
            if (!allowDroppingDuringDialogue)
            {
                ActionBasedController[] controllers = FindObjectsOfType<ActionBasedController>();
                foreach (var controller in controllers)
                {
                    if (controller != null && controller.selectAction != null && controller.selectAction.action != null)
                    {
                        if (controller.selectAction.action.enabled)
                        {
                            controller.selectAction.action.Disable();
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Error disabling player movement: " + ex.Message);
        }
    }
    
    void EnablePlayerMovement()
    {
        try
        {
            foreach (var provider in allLocomotionProviders)
            {
                if (provider != null)
                {
                    provider.enabled = true;
                }
            }
            
            if (teleportProvider != null)
            {
                teleportProvider.enabled = true;
            }
            
            if (moveProvider != null)
            {
                if (moveProvider.leftHandMoveAction.action != null)
                    moveProvider.leftHandMoveAction.action.Enable();
                
                if (moveProvider.rightHandMoveAction.action != null)
                    moveProvider.rightHandMoveAction.action.Enable();
            }
            
            if (turnProvider != null)
            {
                if (turnProvider.leftHandSnapTurnAction.action != null)
                    turnProvider.leftHandSnapTurnAction.action.Enable();
                
                if (turnProvider.rightHandSnapTurnAction.action != null)
                    turnProvider.rightHandSnapTurnAction.action.Enable();
            }
            
            TeleportationAnchor[] teleportAnchors = FindObjectsOfType<TeleportationAnchor>();
            foreach (var anchor in teleportAnchors)
            {
                if (anchor != null)
                    anchor.enabled = true;
            }
            
            XRRayInteractor[] rayInteractors = FindObjectsOfType<XRRayInteractor>();
            foreach (var interactor in rayInteractors)
            {
                var teleportInteractor = interactor.GetComponent<TeleportationAnchor>();
                var teleportationRay = interactor.GetComponent<XRRayInteractor>();
                
                if (teleportInteractor != null || teleportationRay != null)
                {
                    interactor.enabled = true;
                }
            }
            
            
            ActionBasedController[] controllers = FindObjectsOfType<ActionBasedController>();
            foreach (var controller in controllers)
            {
                if (controller != null && controller.selectAction != null && controller.selectAction.action != null)
                {
                    controller.selectAction.action.Enable();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Error enabling player movement: " + ex.Message);
        }
    }
    
    void RevealAllObjects()
    {
        if (objectsRevealed) return;
        
        objectsRevealed = true;
        
        
        foreach (GameObject fragment in stoneFragments)
        {
            if (fragment != null)
            {
                fragment.SetActive(true);
            }
        }
        
        
        foreach (GameObject canvas in canvasUI)
        {
            if (canvas != null)
            {
                canvas.SetActive(true);
            }
        }
        
        
        foreach (GameObject effect in vfxEffects)
        {
            if (effect != null)
            {
                effect.SetActive(true);
            }
        }
    }
    
    void HideAllObjects()
    {
        foreach (GameObject fragment in stoneFragments)
        {
            if (fragment != null)
            {
                fragment.SetActive(false);
            }
        }
        
        
        foreach (GameObject canvas in canvasUI)
        {
            if (canvas != null)
            {
                canvas.SetActive(false);
            }
        }
        
        
        foreach (GameObject effect in vfxEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
    }
    
    public void FragmentCollected(GameObject fragment)
    {
        if (fragment == null) return;
        
        
        AudioSource fragmentAudio = fragment.GetComponent<AudioSource>();
        if (fragmentAudio != null && fragmentAudio.clip != null)
        {
            fragmentAudio.Play();
        }
        
        
        int fragmentIndex = Array.IndexOf(stoneFragments, fragment);
        
        
        if (disableMatchingVFX && fragmentIndex >= 0 && fragmentIndex < vfxEffects.Length)
        {
            if (vfxEffects[fragmentIndex] != null)
            {
                
                StartCoroutine(FadeOutVFX(vfxEffects[fragmentIndex]));
            }
        }
        
        
        PlayFragmentVoiceLine(collectedFragments);
        
        collectedFragments++;
        
        
        StartCoroutine(DisableAfterSound(fragment));
        
        Debug.Log("Fragment collected! " + collectedFragments + " / " + totalFragmentsNeeded);
        
        
        UpdateAnimationParameters();
    }
    
    private IEnumerator FadeOutVFX(GameObject vfxObject)
    {
        
        ParticleSystem ps = vfxObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            
            var main = ps.main;
            main.loop = false; 
            ps.Stop(true); 
            
            
            yield return new WaitForSeconds(main.startLifetime.constantMax);
        }
        
        
        Renderer[] renderers = vfxObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            float elapsedTime = 0;
            float fadeDuration = 0.5f;
            
            
            Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material.HasProperty("_Color"))
                {
                    originalColors[renderer] = renderer.material.color;
                }
            }
            
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float fadeAmount = 1 - (elapsedTime / fadeDuration);
                
                foreach (Renderer renderer in renderers)
                {
                    if (originalColors.ContainsKey(renderer))
                    {
                        Color color = originalColors[renderer];
                        color.a = fadeAmount;
                        renderer.material.color = color;
                    }
                }
                
                yield return null;
            }
        }
        
        
        vfxObject.SetActive(false);
    }
    
    private IEnumerator DisableAfterSound(GameObject fragment)
    {
        
        AudioSource fragmentAudio = fragment.GetComponent<AudioSource>();
        if (fragmentAudio != null && fragmentAudio.isPlaying)
        {
            yield return new WaitForSeconds(fragmentAudio.clip.length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        
        fragment.SetActive(false);
    }
    
    public bool AllFragmentsCollected()
    {
        return collectedFragments >= totalFragmentsNeeded;
    }
    
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        
        Gizmos.color = new Color(0.8f, 0.4f, 0.8f, 0.6f); 
        Gizmos.DrawWireSphere(transform.position, cryingSoundRadius);
    }
    
    
    void OnValidate()
    {
        
        if (Application.isPlaying)
        {
            RemoveAllOutlineComponents();
        }
    }
    
    
    void RemoveAllOutlineComponents()
    {
        try
        {
            
            MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>();
            foreach (MonoBehaviour script in allScripts)
            {
                if (script == null) continue;
                
                string typeName = script.GetType().Name.ToLower();
                if (typeName.Contains("outline") || typeName.Contains("hover") || 
                    typeName.Contains("highlight") || typeName.Contains("glow"))
                {
                    Destroy(script);
                }
            }
            
            XRRayInteractor[] rayInteractors = FindObjectsOfType<XRRayInteractor>();
            foreach (var interactor in rayInteractors)
            {
                if (interactor != null)
                {
                    LineRenderer lineRenderer = interactor.GetComponent<LineRenderer>();
                    if (lineRenderer != null)
                    {
                        lineRenderer.enabled = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Error while removing outline components: " + ex.Message);
        }
    }
    
    
    private void UpdateCryingSoundVolume()
    {
        if (cryingAudioSource != null && cryingSound != null && !hasPlayedIntroDialogue)
        {
            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            float normalizedDistance = Mathf.Clamp01(distance / cryingSoundRadius);
            
            
            float targetVolume = Mathf.Lerp(maxCryingVolume, minCryingVolume, normalizedDistance);
            
            
            cryingAudioSource.volume = Mathf.Lerp(cryingAudioSource.volume, targetVolume, Time.deltaTime * cryingFadeSpeed);
        }
    }
    
    
    private IEnumerator FadeOutAudio(AudioSource source, float fadeDuration)
    {
        if (source == null) yield break;
        
        float startVolume = source.volume;
        float timer = 0;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
            yield return null;
        }
        
        source.Stop();
        source.volume = 0;
    }
} 