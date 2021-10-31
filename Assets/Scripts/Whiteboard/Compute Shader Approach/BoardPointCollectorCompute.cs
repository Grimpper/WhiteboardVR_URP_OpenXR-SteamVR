using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CustomProperties;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(MeshCollider), typeof(BoardComputeRenderer))]
public class BoardPointCollectorCompute : MonoBehaviour
{
    // Parameters
    [SerializeField] [TagSelectorAtributte] private string markerTag;
    
    // Actions
    [Header("Input Actions")] [SerializeField]
    private SteamVR_Action_Boolean eraseButton;
    
    [Header("Sampling Parameters")]
    [SerializeField] [Min(0.0001f)] private float contactOffset = 0.001f;
    [SerializeField] private float sampleRate = 1f;
    
    [Header("Developer Options")]
    [SerializeField] private bool showDebug = false;

    // Raycast variables
    //private float raycastOffset = 0.1f;
    private int layerMask;
    
    // Intern variables
    private IEnumerator samplePoints;
    private BoardComputeRenderer boardComputeRenderer;
    private Collision markerCollision = new Collision();
    
    // Original collided object variables
    private float originalContactOffset;
    
    // Player
    private Player player = null;

    private Transform tipTransform = null;
    private Color markerColor = Color.black;
    
    private void Start()
    {
        MeshCollider boardCollider = GetComponent<MeshCollider>();
        boardCollider.contactOffset = contactOffset;
        boardComputeRenderer = GetComponent<BoardComputeRenderer>();
        layerMask = (int)Mathf.Pow(2f, gameObject.layer);
        player = Player.instance;
    }

    private bool IsMarker(Collider target) => target.GetComponent<Collider>().gameObject.CompareTag(markerTag);
    private bool IsMarker(Collision target) => target.collider.gameObject.CompareTag(markerTag);

    private void OnTriggerEnter(Collider other)
    {
        if (!IsMarker(other)) return;
        
        originalContactOffset = other.contactOffset;
        other.contactOffset = contactOffset;

        markerColor = other.gameObject.GetComponentInParent<Marker>().Color;
        
        tipTransform = other.GetComponent<Collider>().transform;
        
        if(showDebug) Debug.Log("Triggered: changing contactOffset");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsMarker(other)) return;
        
        other.contactOffset = originalContactOffset;

        if(showDebug) Debug.Log("Exited trigger: restoring contactOffset");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!IsMarker(other)) return;

        if (samplePoints != null) 
            StopCoroutine(samplePoints);
        
        samplePoints = SamplePointsRoutine();
        StartCoroutine(samplePoints);
        
        foreach (var hand in player.hands)
        {
            if(showDebug) Debug.Log(hand.name);
            if (hand.currentAttachedObject != null && hand.currentAttachedObject.name == other.gameObject.name)
                hand.TriggerHapticPulse(1000);
        }
        
        if(showDebug) Debug.Log(other.gameObject.name + " collided: starting sampling coroutine");
    }

    private void OnCollisionStay(Collision other)
    {
        if (!other.collider.gameObject.CompareTag(markerTag)) return;
        
        markerCollision = other;
        
        if(showDebug) Debug.Log(other.gameObject.name + " colliding: updating collision");
    }


    private void OnCollisionExit(Collision other)
    {
         if (!IsMarker(other)) return;
         
         StopCoroutine(samplePoints);
         boardComputeRenderer.Painting = false;
         
         if(showDebug) Debug.Log(other.gameObject.name + " decollided: stopping sampling coroutine");
     }
    
    /*
    private Vector3 GetAverageCollisionPoint(Collision collision)
    {
        Vector3 collisionAverage = Vector3.zero;

        for (int i = 0; i < collision.contactCount; i++)
        {
            collisionAverage += collision.GetContact(i).point;
        }
        
        collisionAverage /= collision.contactCount;

        return collisionAverage;
    }
    
    private Vector3 GetAverageCollisionNormals(Collision collision)
    {
        Vector3 collisionAverageNormal = Vector3.zero;

        for (int i = 0; i < collision.contactCount; i++)
        {
            collisionAverageNormal += collision.GetContact(i).normal;
        }
        
        collisionAverageNormal /= collision.contactCount;

        return collisionAverageNormal.normalized;
    }*/
 
    // TODO: there is a bug where marker layer gets changed to an incorrect one
     private IEnumerator SamplePointsRoutine()
     {
         while (true)
         {
             /*Vector3 collisionAverage = GetAverageCollisionPoint(markerCollision);
             Vector3 collisionAverageNormal = GetAverageCollisionNormals(markerCollision);
             Vector3 raycastOrigin = collisionAverage - collisionAverageNormal * raycastOffset;*/

             Vector3 tipDirection = -tipTransform.up;
             Vector3 raycastOrigin = tipTransform.position;
             
             Debug.DrawLine(raycastOrigin, tipDirection * 100);

             float rayLength = Mathf.Infinity;
             Ray ray = new Ray(raycastOrigin, tipDirection);
             if (Physics.Raycast(ray, out var raycastHit, rayLength, layerMask, QueryTriggerInteraction.Ignore))
             {
                 boardComputeRenderer.Color = markerColor;
                 boardComputeRenderer.EraseMode = eraseButton.state;
                 
                 boardComputeRenderer.CollisionHit = raycastHit; // Dispatches shader
                 boardComputeRenderer.Painting = true;
                 
                 if (showDebug)
                 {
                     Debug.Log("Marker Color: " + boardComputeRenderer.Color);
                     
                     Debug.DrawRay(ray.origin,ray.direction, Color.green, duration: 10000, false);
                     Debug.Log("Raycast Layer Mask: " + layerMask);
                     Debug.Log("Raycast Hit Point: " + raycastHit.point);
                     Debug.Log("Raycast TextCoord: " + raycastHit.textureCoord);
                 }
                 
             }
             
             yield return new WaitForSeconds(sampleRate);
         }
     }

}
