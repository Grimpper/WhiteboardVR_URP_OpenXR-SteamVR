using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using CustomProperties;

public class RoomCollider : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform player;
    
    [Tooltip("Ignore gameObject with this tag when checking player's transform inside a collision")]
    [TagSelectorAtributte] public string ignoreTag;

    [Header("Data")]
    [CustomProperties.ReadOnly] public bool insideCollider;
    [CustomProperties.ReadOnly] public Vector3 lastCollisionTransform;

    private CapsuleCollider capsuleCollider;
    
    
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
        if (other.CompareTag(ignoreTag)) return;
        
        lastCollisionTransform = capsuleCollider.transform.position;
        insideCollider = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(ignoreTag)) return;
        
        lastCollisionTransform = capsuleCollider.transform.position;
        insideCollider = false;
    }
}
