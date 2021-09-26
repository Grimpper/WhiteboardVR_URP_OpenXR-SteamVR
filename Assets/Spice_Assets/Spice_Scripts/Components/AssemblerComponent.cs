using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Assembler
{
    public class AssemblerComponent : MonoBehaviour
    {
        [Serializable]
        public class SnapPoint
        {
            public Vector3 point;
            public Vector3 normal;

            public int ID;

            public SnapPoint(Vector3 p, Vector3 n, int i)
            {
                point = p;
                normal = n.normalized;
                ID = i;
            }
        }

        public static AssemblerComponent coreComp;

        [Tooltip("Is this part the center that cannot be destroyed?")]
        public bool core;

        public SnapPoint[] points;

        [HideInInspector] public AssemblerPoint[] pointColliders;
        
        [HideInInspector] public bool connected;

        public GameObject previewHolo;

        //private Interactable interactable;
        private Rigidbody rb;
        private float rad = 0.3f;

        private void Start()
        {
            if (previewHolo)
                previewHolo.SetActive(true);

            rb = GetComponent<Rigidbody>();
            
            pointColliders = new AssemblerPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                GameObject g = new GameObject("pt_" + i);
                pointColliders[i] = g.AddComponent<AssemblerPoint>();
                pointColliders[i].radius = rad;
                pointColliders[i].transform.parent = transform;
                pointColliders[i].transform.position = transform.TransformPoint(points[i].point);
                pointColliders[i].transform.rotation =
                    Quaternion.LookRotation(transform.TransformDirection(points[i].normal), transform.up);
                pointColliders[i].comp = this;
                pointColliders[i].ID = points[i].ID;
                pointColliders[i].gameObject.layer = LayerMask.NameToLayer("TransparentFX"); // to avoid collisions
            }

            if (core)
                coreComp = this;
            // else
            //     interactable = GetComponent<Interactable>();
        }
    }

    public class PointHitCheck
    {
        public float dist;
        public AssemblerPoint point;
        
        public PointHitCheck(float hitDistance, AssemblerPoint hitPoint)
        {
            dist = hitDistance;
            point = hitPoint;
        }
    }
}
