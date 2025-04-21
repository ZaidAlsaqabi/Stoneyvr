using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Footsteps : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ActionBasedContinuousMoveProvider moveProvider;

    [Header("Footstep Settings")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float minTimeBetweenSteps = 0.3f;
    [SerializeField] private float moveThreshold = 0.1f;

    private float timeSinceLastStep;
    private bool isMoving;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (moveProvider == null)
        {
            moveProvider = GetComponentInParent<ActionBasedContinuousMoveProvider>();
        }
    }

    void Update()
    {
        if (moveProvider == null) return;

        Vector2 leftHandMovement = moveProvider.leftHandMoveAction.action.ReadValue<Vector2>();
        Vector2 rightHandMovement = moveProvider.rightHandMoveAction.action.ReadValue<Vector2>();
        
        isMoving = leftHandMovement.magnitude > moveThreshold || 
                   rightHandMovement.magnitude > moveThreshold;

        if (isMoving)
        {
            timeSinceLastStep += Time.deltaTime;

            if (timeSinceLastStep >= minTimeBetweenSteps)
            {
                PlayFootstepSound();
                timeSinceLastStep = 0f;
            }
        }
    }

    private void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

       
        AudioClip randomStep = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.PlayOneShot(randomStep);
    }
}
