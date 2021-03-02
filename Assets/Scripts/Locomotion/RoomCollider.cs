using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCollider : MonoBehaviour
{
    // OBJECTS
    public Transform head;
    public Transform player;
    
    private CapsuleCollider capsuleCollider;
    
    // ATTRIBUTES
    public Vector3 lastCollisionTransform;
    public bool insideCollider;

    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        SetHeight();
        SetTransform();
    }
    
    void SetHeight()
    {
        capsuleCollider.height = Mathf.Max(head.transform.localPosition.y, capsuleCollider.radius);
    }
    
    void SetTransform()
    {
        transform.position = Vector3.ProjectOnPlane(head.transform.position, Vector3.up) +
                             (capsuleCollider.height / 2 + player.transform.position.y) * Vector3.up;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ground"))
        {
            lastCollisionTransform = capsuleCollider.transform.position;
            insideCollider = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Ground"))
        {
            lastCollisionTransform = capsuleCollider.transform.position;
            insideCollider = false;
        }
    }
}
