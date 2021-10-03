using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroGravity : MonoBehaviour
{
    private BoxCollider zeroGravityTrigger;

    private void Start()
    {
        zeroGravityTrigger = GetComponent<BoxCollider>();
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb == null || rb.CompareTag("Player") || rb.CompareTag("Environment"))
            return;
        
        rb.useGravity = false;
        rb.drag = 0;
    }
    
    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        
        if (rb == null || rb.CompareTag("Player") || rb.CompareTag("Environment"))
            return;

        rb.useGravity = true;
        rb.drag = 1;
    }
}
