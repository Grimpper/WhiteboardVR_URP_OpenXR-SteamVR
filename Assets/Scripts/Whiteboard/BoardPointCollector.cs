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

    private IEnumerator samplePoints;
    private BoardRenderer boardRenderer;
    private Collision markerCollision = new Collision();

    public RaycastHit raycastHit;

    private void Start()
    {
        boardRenderer = GetComponent<BoardRenderer>();
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

        return collisionAverageNormal;
    }
 
     private IEnumerator SamplePointsRoutine()
     {
         while (true)
         {
             Vector3 collisionAverage = GetAverageCollisionPoint(markerCollision);
             Vector3 collisionAverageNormal = GetAverageCollisionNormals(markerCollision);

             int layerMask = gameObject.layer;
             float rayLength = 100;
             Ray ray = new Ray(collisionAverage, collisionAverageNormal);

             if (Physics.Raycast(ray, out raycastHit, rayLength, ~layerMask))
             {
                 boardRenderer.CollisionHit = raycastHit;
                 boardRenderer.SetColor(markerCollision.gameObject.GetComponent<Marker>().Color);
             }

             if (showDebug)
             {
                 Debug.DrawRay(ray.origin,ray.direction, Color.green, duration: 10000, false);
                 Debug.Log("RAYCAST POINT: " + raycastHit.point);
                 Debug.Log("RAYCAST TEXCOORD: " + raycastHit.textureCoord);
             }

             yield return new WaitForSeconds(sampleRate);
         }
     }

}
