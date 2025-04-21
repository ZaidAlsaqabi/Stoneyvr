using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
	private Light torchLight;

	void Start()
	{
		torchLight = GetComponentInChildren<Light>();
		torchLight.enabled = false;  // Start with the light off
	}

	public void ToggleLight()
	{
		if (torchLight.isActiveAndEnabled)
		{
			torchLight.enabled = false;
		}
		else
		{
			torchLight.enabled = true;
		}
	}
}