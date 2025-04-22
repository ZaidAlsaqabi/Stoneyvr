using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerlook : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    
    [Header("Look Settings")]
    public float lookRadius = 5f;
    public float lookSpeed = 3f;
    
    [Header("Return Settings")]
    public float returnSpeed = 2f;
    
    private Quaternion originalRotation;
    
    void Start()
    {
        originalRotation = transform.rotation;
        
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    void Update()
    {
        if (player == null)
            return;
            
        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance <= lookRadius)
        {
            LookAtPlayer();
        }
        else
        {
            ReturnToOriginalRotation();
        }
    }
    
    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookSpeed);
        }
    }
    
    void ReturnToOriginalRotation()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * returnSpeed);
    }
}
