using UnityEngine;

public class TorchController : MonoBehaviour
{
    private Light torchLight;

    private void Start()
    {
        torchLight = GetComponentInChildren<Light>();
    }

    public void ToggleLight()
    {
        torchLight.enabled = !torchLight.enabled;
    }
} 