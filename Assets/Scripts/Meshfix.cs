using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meshfix : MonoBehaviour
{
    public Mesh originalMesh; 
    private MeshFilter meshFilter;


    private void Awake()
    {
  
        meshFilter = GetComponent<MeshFilter>();
        
    
        if (originalMesh == null && meshFilter != null && meshFilter.sharedMesh != null)
        {
            originalMesh = meshFilter.sharedMesh;
        }
    }


    void Start()
    {

        if (meshFilter != null && originalMesh != null)
        {
            meshFilter.mesh = originalMesh;
        }
    }
}