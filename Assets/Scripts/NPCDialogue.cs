using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private Transform player;
    [SerializeField] private AudioClip[] introDialogueClips;
    [SerializeField] private AudioClip[] endingDialogueClips;
    [SerializeField] private GameObject visualCue;
    
    [Header("Stone Fragment Settings")]
    [SerializeField] private StoneFragmentManager fragmentManager;
    [SerializeField] private GameObject[] stoneFragmentsToReveal;
    [SerializeField] private GameObject[] canvasesToReveal;
    [SerializeField] private GameObject[] fireflyEffectsToReveal;
    
    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;
    public UnityEvent onAllFragmentsCollected;
    
    private AudioSource audioSource;
    private bool hasPlayedIntro = false;
    private bool hasPlayedEnding = false;
    private bool isInRange = false;
    private bool isPlayingDialogue = false;
    private bool canStartDialogue = true;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (visualCue != null)
        {
            visualCue.SetActive(false);
        }
        
        if (player == null)
        {
            player = Camera.main.transform;
        }
        
        
        HideAllObjects();
    }
    
    void Update()
    {
        CheckPlayerDistance();
        

        if (fragmentManager != null && fragmentManager.AllFragmentsCollected() && isInRange && !hasPlayedEnding && canStartDialogue)
        {
            StartEndingDialogue();
        }
    }
    
    private void CheckPlayerDistance()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        
 
        if (distance <= interactionRadius && !isInRange)
        {
            isInRange = true;
            if (visualCue != null)
            {
                visualCue.SetActive(true);
            }
            
       
            if (!hasPlayedIntro && canStartDialogue)
            {
                StartIntroDialogue();
            }
        }
    
        else if (distance > interactionRadius && isInRange)
        {
            isInRange = false;
            if (visualCue != null)
            {
                visualCue.SetActive(false);
            }
        }
    }
    
    private void StartIntroDialogue()
    {
        if (isPlayingDialogue || introDialogueClips.Length == 0) return;
        
        StartCoroutine(PlayDialogueSequence(introDialogueClips, true));
    }
    
    private void StartEndingDialogue()
    {
        if (isPlayingDialogue || endingDialogueClips.Length == 0) return;
        
        StartCoroutine(PlayDialogueSequence(endingDialogueClips, false));
    }
    
    private IEnumerator PlayDialogueSequence(AudioClip[] dialogueClips, bool isIntroSequence)
    {
        isPlayingDialogue = true;
        canStartDialogue = false;
        
        if (isIntroSequence)
        {
            onDialogueStart.Invoke();
            hasPlayedIntro = true;
        }
        else
        {
            onAllFragmentsCollected.Invoke();
            hasPlayedEnding = true;
        }
        
        foreach (AudioClip clip in dialogueClips)
        {
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
             
                yield return new WaitForSeconds(clip.length);
            }
        }
        
        isPlayingDialogue = false;
        
        if (isIntroSequence)
        {
            onDialogueEnd.Invoke();
            RevealAllObjects();
            canStartDialogue = true;
        }
        else
        {
            
            yield return new WaitForSeconds(1.0f);
            SceneTransitionManager.singleton.GoToScene(0);
        }
    }
    
    private void RevealAllObjects()
    {
        foreach (GameObject fragment in stoneFragmentsToReveal)
        {
            if (fragment != null)
            {
                fragment.SetActive(true);
            }
        }
        
        foreach (GameObject canvas in canvasesToReveal)
        {
            if (canvas != null)
            {
                canvas.SetActive(true);
            }
        }
        
        foreach (GameObject effect in fireflyEffectsToReveal)
        {
            if (effect != null)
            {
                effect.SetActive(true);
            }
        }
    }
    
    private void HideAllObjects()
    {
        foreach (GameObject fragment in stoneFragmentsToReveal)
        {
            if (fragment != null)
            {
                fragment.SetActive(false);
            }
        }
        
        foreach (GameObject canvas in canvasesToReveal)
        {
            if (canvas != null)
            {
                canvas.SetActive(false);
            }
        }
        
        foreach (GameObject effect in fireflyEffectsToReveal)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
    }
    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
} 