using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Es.InkPainter;

public class BoardPointCollector : MonoBehaviour
{
    // Parameters
    [SerializeField] [TagSelectorAtributte] private string markerTag;
    [SerializeField] private float sampleRate = 1f;
    [SerializeField] private bool showDebug = false;
    
    // Raycast variables
    public RaycastHit raycastHit;
    private float raycastOffset = 0.1f;
    private int layerMask;
    
    // Intern variables
    private IEnumerator samplePoints;
    private BoardRenderer boardRenderer;
    private Collision markerCollision = new Collision();

    private void Start()
    {
        boardRenderer = GetComponent<BoardRenderer>();
        layerMask = (int)Mathf.Pow(2f, gameObject.layer);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag(markerTag)) return;
        
        if (samplePoints != null) StopCoroutine(samplePoints);

        samplePoints = SamplePointsRoutine();
        StartCoroutine(samplePoints);
    }

    private void OnCollisionStay(Collision other)
    {
        if (!other.gameObject.CompareTag(markerTag)) return;

        markerCollision = other;
    }


    private void OnCollisionExit(Collision other)
     {
         if (!other.gameObject.CompareTag(markerTag)) return;
         
         StopCoroutine(samplePoints);
         boardRenderer.StrokeCleared = true;
     }

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
    }
 
     private IEnumerator SamplePointsRoutine()
     {
         while (true)
         {
             Vector3 collisionAverage = GetAverageCollisionPoint(markerCollision);
             Vector3 collisionAverageNormal = GetAverageCollisionNormals(markerCollision);
             Vector3 raycastOrigin = collisionAverage - collisionAverageNormal * raycastOffset;

             float rayLength = Mathf.Infinity;
             Ray ray = new Ray(raycastOrigin, collisionAverageNormal);
             if (Physics.Raycast(ray, out raycastHit, rayLength, layerMask))
             {
                 boardRenderer.SetColor(markerCollision.gameObject.GetComponent<Marker>().Color);
                 boardRenderer.CollisionHit = raycastHit;
                 
                 if (showDebug)
                 {
                     Debug.Log("MARKER COLOR: " + markerCollision.gameObject.GetComponent<Marker>().Color);
                     
                     Debug.DrawRay(ray.origin,ray.direction, Color.green, duration: 10000, false);
                     Debug.Log("RAYCAST LAYER MASK: " + layerMask);
                     Debug.Log("RAYCAST POINT: " + raycastHit.point);
                     Debug.Log("RAYCAST TEXCOORD: " + raycastHit.textureCoord);
                 }
                 
             }

             yield return new WaitForSeconds(sampleRate);
         }
     }

}
