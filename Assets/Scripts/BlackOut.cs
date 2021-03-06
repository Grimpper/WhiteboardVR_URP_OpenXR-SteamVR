using System;
using System.Collections;
using System.Collections.Generic;
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
    private Vector3 direction;

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
        if (insideCollision)
        {
            Physics.ComputePenetration(colliderA, transform.position, transform.rotation, colliderB,
                colliderB.transform.position, colliderB.transform.rotation, out direction, out distance);

            if (distance >= headDiameter)
            {
                distance = headDiameter;
            }
            
            RenderSettings.fogDensity = distance * (maxFogDensity - minFogDensity) / headDiameter;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Head collided");
        colliderB = other;
        ToggleBlackOut(true);
        insideCollision = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Head decollided"); 
        ToggleBlackOut(false);
        insideCollision = false;
    }

    private void ToggleBlackOut(bool state)
    {
        ColorGrading blackOut;
        postProcessVolume.profile.TryGetSettings(out blackOut);

        blackOut.active = state;
        RenderSettings.fog = state;
    }
}
