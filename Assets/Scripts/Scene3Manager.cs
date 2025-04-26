using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene3Manager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioClip voiceLine1;
    [SerializeField] private AudioClip voiceLine2;
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private AudioSource npcAudioSource;
    [SerializeField] private FadeScreen fadeScreen;
    
    [Header("Animation Parameters")]
    [SerializeField] private string thankYouAnimTrigger = "Thank You";
    [SerializeField] private string talkingAnimTrigger = "Talking";
    
    [Header("Settings")]
    [SerializeField] private float initialDelay = 2.5f;
    
    private void Start()
    {
        if (npcAudioSource == null)
        {
            npcAudioSource = GetComponent<AudioSource>();
            if (npcAudioSource == null)
            {
                npcAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        if (fadeScreen == null && SceneTransitionManager.singleton != null)
        {
            fadeScreen = SceneTransitionManager.singleton.fadeScreen;
        }
        
        if (npcAnimator == null)
        {
            npcAnimator = GetComponent<Animator>();
        }
        
        StartCoroutine(DelayedStartSequence());
    }
    
    private IEnumerator DelayedStartSequence()
    {
        yield return new WaitForSeconds(initialDelay);
        
        StartCoroutine(Scene3Sequence());
    }
    
    private IEnumerator Scene3Sequence()
    {
        if (voiceLine1 != null)
        {
            if (npcAnimator != null && HasAnimatorParameter(thankYouAnimTrigger))
            {
                npcAnimator.SetTrigger(thankYouAnimTrigger);
            }
            
            npcAudioSource.clip = voiceLine1;
            npcAudioSource.Play();
            
            float animationTime = voiceLine1.length;
            yield return new WaitForSeconds(animationTime);
            
            yield return new WaitForSeconds(0.5f);
        }
        
        if (voiceLine2 != null)
        {
            if (npcAnimator != null && HasAnimatorParameter(talkingAnimTrigger))
            {
                npcAnimator.SetTrigger(talkingAnimTrigger);
            }
            
            npcAudioSource.clip = voiceLine2;
            npcAudioSource.Play();
            
            float animationTime = voiceLine2.length;
            yield return new WaitForSeconds(animationTime);
            
            yield return new WaitForSeconds(1f);
        }
        
        if (SceneTransitionManager.singleton != null)
        {
            SceneTransitionManager.singleton.GoToScene(0);
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
} 