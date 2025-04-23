using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class StoneFragment : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private bool isCollected = false;
    [SerializeField] private AudioClip collectionSound;
    [SerializeField] private GameObject collectionVFX;
    [SerializeField] private float destroyDelay = 1.0f;
    
    [Header("Optional")]
    [SerializeField] private GameObject visualModel;
    [SerializeField] private Collider fragmentCollider;
    
    public UnityEvent onCollect;
    
    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && collectionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }
        
        if (fragmentCollider == null)
        {
            fragmentCollider = GetComponent<Collider>();
        }
        
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }
    
    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (!isCollected)
        {
            Collect();
        }
    }
    
    public void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        if (collectionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectionSound);
        }
        
        if (collectionVFX != null)
        {
            Instantiate(collectionVFX, transform.position, Quaternion.identity);
        }
        
        onCollect.Invoke();
        
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }
        
        if (fragmentCollider != null)
        {
            fragmentCollider.enabled = false;
        }
        
        StartCoroutine(HandleCollectionFeedback());
    }
    
    private IEnumerator HandleCollectionFeedback()
    {
        if (visualModel != null)
        {
            float elapsed = 0f;
            float duration = destroyDelay;
            Vector3 startScale = visualModel.transform.localScale;
            Vector3 endScale = Vector3.zero;
            
            while (elapsed < duration)
            {
                visualModel.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            visualModel.transform.localScale = endScale;
        }
        else
        {
            yield return new WaitForSeconds(destroyDelay);
        }
        
        gameObject.SetActive(false);
    }
    
    public void ForceCollect()
    {
        if (!isCollected)
        {
            Collect();
        }
    }
} 