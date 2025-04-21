using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Buttonscript : MonoBehaviour
{
    public GameObject Flashlight;
    private XRSimpleInteractable interactable;
    private bool canPress = true;
    private float cooldownTime = 1.0f; 
    
    private GameObject currentFlashlight;

    void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        
        if (interactable != null)
        {
            interactable.selectEntered.AddListener((SelectEnterEventArgs args) => {
                if (canPress)
                {
                    instantiateFlashlight();
                    StartCoroutine(Cooldown());
                }
            });
        }
    }

    private IEnumerator Cooldown()
    {
        canPress = false;
        yield return new WaitForSeconds(cooldownTime);
        canPress = true;
    }

    public void instantiateFlashlight()
    {
        if (currentFlashlight != null)
        {
            Destroy(currentFlashlight);
            currentFlashlight = null;
        }

        if (Flashlight != null)
        {
            Vector3 buttonPosition = transform.position;
            Vector3 spawnPosition = new Vector3(buttonPosition.x, buttonPosition.y - 0.5f, buttonPosition.z);
            
            Quaternion flatRotation = Quaternion.Euler(90f, 0f, 0f);
            currentFlashlight = Instantiate(Flashlight, spawnPosition, flatRotation);
        }
    }

    void Update()
    {
        if (currentFlashlight == null)
        {
            currentFlashlight = null;
        }
    }
}
