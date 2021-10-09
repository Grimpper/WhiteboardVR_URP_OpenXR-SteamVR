using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assembler 
{
    public class AssemblerPoint : MonoBehaviour
    {
        public float radius;
        public AssemblerPoint connectedPoint;
        public bool connected;
        public AssemblerComponent comp;
        public int ID;
        public bool connectable;

        private void Start()
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = radius;
        }

        private void Update()
        {
            if (!connected) return;
            
            if (connectedPoint.connectedPoint != this)
            {
                DetachPoint();
            }

            if (connectedPoint == null)
            {
                DetachPoint();
            }
        }
        
        public void DetachPoint()
        {
            connectedPoint = null;
            connected = false;
        }

        public bool CheckForPoints(out PointHitCheck hit)
        {
            hit = null;

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius * 2);
            foreach (Collider col in colliders)
            {
                AssemblerPoint point = col.GetComponent<AssemblerPoint>();
                if (point == null || point.connected || !CheckAttachConditions(point)) continue;
                
                float hitDistance = Vector3.Distance(transform.position, point.transform.position);
                hit = new PointHitCheck(hitDistance, point);

                return true;
            }

            return false;
        }

        private bool CheckAttachConditions(in AssemblerPoint point)
            => point.ID == ID && point.comp != comp && (point.comp.connected || point.comp.core);
        // same ID          not itself        is a connected snapPoint or the coreComponent
    }
}