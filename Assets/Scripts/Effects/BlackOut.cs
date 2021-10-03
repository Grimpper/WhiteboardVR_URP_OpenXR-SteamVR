using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Valve.VR;

public class BlackOut : MonoBehaviour
{
    // OBJECTS
    [SerializeField] private PostProcessVolume postProcessVolume;

    private Collider colliderA;
    private Collider colliderB;

    // ATTRIBUTES
    [SerializeField] private float minFogDensity = 0;
    [SerializeField] private float maxFogDensity = 0.5f;

    private float headDiameter;
    
    public bool insideCollision;

    private float distance;

    [SerializeField] private LayerMask layerMask;
    
    [Space]
    [SerializeField] private bool debug = false;

    private void Start()
    {
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = minFogDensity;
        RenderSettings.fogColor = Color.black;

        colliderA = GetComponent<Collider>();
        headDiameter = GetComponent<SphereCollider>().radius * 2;
    }

    private void FixedUpdate()
    {
        if (!insideCollision) return;
        
        Physics.ComputePenetration(colliderA, transform.position, transform.rotation, colliderB,
            colliderB.transform.position, colliderB.transform.rotation, out _, out distance);

        if (distance >= headDiameter)
        {
            distance = headDiameter;
        }
            
        RenderSettings.fogDensity = distance * (maxFogDensity - minFogDensity) / headDiameter;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ContainsLayer(layerMask, other.gameObject.layer))
            return;
        
        colliderB = other;
        ToggleBlackOut(true);
        insideCollision = true;
        
        if (debug)
            Debug.Log("Head collided");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!ContainsLayer(layerMask, other.gameObject.layer))
            return;
        
        ToggleBlackOut(false);
        insideCollision = false;
        
        if (debug)
            Debug.Log("Head de-collided");
    }

    private void ToggleBlackOut(bool state)
    {
        postProcessVolume.profile.TryGetSettings(out ColorGrading blackOut);

        blackOut.active = state;
        RenderSettings.fog = state;
    }

    private bool ContainsLayer(LayerMask mask, int layer) => mask == (mask | (1 << layer));
}
