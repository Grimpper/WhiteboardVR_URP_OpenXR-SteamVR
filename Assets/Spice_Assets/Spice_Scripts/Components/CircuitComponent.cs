using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Random = UnityEngine.Random;

namespace Circuit
{
    public class CircuitComponent : MonoBehaviour
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

        public bool dismantle;

        private static CircuitComponent coreComp;

        public bool faker;
        public Material fakerSky;

        [Tooltip("Is this part the center that cannot be destroyed?")]
        public bool core;

        public SnapPoint[] points;

        [HideInInspector] public CircuitPoint[] pointColliders;
        
        [HideInInspector] public bool connected;

        public GameObject previewHolo;

        public int distanceFromCore;

        private Interactable interactable;
        private Rigidbody rb;
        private float radius = 0.3f;

        private bool wasGrab;

        private CircuitPoint mountPoint;
        private CircuitPoint attachPoint;

        private Hand holdingHand;
        private static readonly int Open = Animator.StringToHash("Open");

        public delegate void OnAttachedDelegate(Hand hand);

        public event OnAttachedDelegate onAttach;

        private void Start()
        {
            if (previewHolo)
                previewHolo.SetActive(true);

            rb = GetComponent<Rigidbody>();
            
            pointColliders = new CircuitPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                GameObject g = new GameObject("pt_" + i);
                pointColliders[i] = g.AddComponent<CircuitPoint>();
                pointColliders[i].radius = radius;
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
            else
                interactable = GetComponent<Interactable>();
        }

        private void Update()
        {
            if (faker)
                return;

            bool detached = true;
            for (int i = 0; i < points.Length; i++)
            {
                if (core == false)
                {
                    pointColliders[i].connectable = rb == null;

                    if (pointColliders[i].connected)
                        detached = false;
                }
                else
                {
                    detached = false;
                    pointColliders[i].connectable = true;
                }
            }

            if (core == false)
            {
                if (detached && connected) 
                    Detach();

                if (interactable.attachedToHand != null) // If being held
                {
                    holdingHand = interactable.attachedToHand;

                    if (connected && dismantle)
                        Detach();
                }

                rb.isKinematic = false;
                PointHitCheck[] hits = new PointHitCheck[points.Length];

                for (int i = 0; i < points.Length; i++)
                {
                    PointHitCheck hit;
                    if (pointColliders[i].CheckForPoints(out hit)) // Check if each point is touching another object's
                        hits[i] = hit;
                }

                mountPoint = FindNearest(hits);

                if (mountPoint != null)
                    RenderSnap();
            }
            else if (wasGrab && mountPoint != null)
            {
                Attach();
                mountPoint = null;
            }

            wasGrab = interactable.attachedToHand;
        }

        private void Attach()
        {
            onAttach?.Invoke(holdingHand);

            if (previewHolo)
                previewHolo.SetActive(false);
            
            GetComponent<Animator>()?.SetTrigger(Open);

            connected = true;

            Transform[] allTransforms = interactable.GetComponentsInChildren<Transform>(true);
            GameObject[] allGameObjects = new GameObject[allTransforms.Length];

            for (int i = 0; i < allTransforms.Length; i++)
                allGameObjects[i] = allTransforms[i].gameObject;

            interactable.hideHighlight = allGameObjects;

            transform.parent = coreComp.transform;
            distanceFromCore = mountPoint.comp.distanceFromCore + 1;

            SnapIn(transform);

            mountPoint.connected = true;
            mountPoint.connectedPoint = attachPoint;

            attachPoint.connected = true;
            attachPoint.connectedPoint = mountPoint;
        }

        public void Detach()
        {
            if (previewHolo)
                previewHolo.SetActive(true);

            if (core) return;
            
            rb.isKinematic = false;
            connected = false;
            
            for (int i = 0; i < points.Length; i++)
            {
                if (pointColliders[i].connected)
                {
                    if (pointColliders[i].connectedPoint.comp.distanceFromCore > distanceFromCore) 
                        // If another component will be detached from the core, start a chain reaction
                    {
                        pointColliders[i].connectedPoint.comp.Detach();
                    }
                }
                    
                pointColliders[i].DetachPoint();
            }

            const float rVel = 0.5f;
            rb.velocity = 
                new Vector3(Random.Range(-rVel, rVel), Random.Range(-rVel, rVel), Random.Range(-rVel, rVel));
            
            float rA = 20;
            rb.angularVelocity = new Vector3(Random.Range(-rA, rA), Random.Range(-rA, rA), Random.Range(-rA, rA));
        }

        private void SnapIn(Transform snapTransform, bool doSnap = true)
        {
            Transform tempObj = new GameObject().transform;
            tempObj.position = snapTransform.position;
            tempObj.rotation = snapTransform.rotation;
            tempObj.parent = mountPoint.transform;
            
            Vector3 hackyRounding = tempObj.localEulerAngles;
            hackyRounding.x = Mathf.Round(hackyRounding.x / 90) * 90;
            hackyRounding.y = Mathf.Round(hackyRounding.y / 90) * 90;
            hackyRounding.z = Mathf.Round(hackyRounding.z / 90) * 90;
            tempObj.localEulerAngles = hackyRounding;

            snapTransform.eulerAngles = tempObj.eulerAngles;
            
            Destroy(tempObj.gameObject);

            Vector3 posDiff = mountPoint.transform.position - attachPoint.transform.position;
            snapTransform.position += posDiff;

            if (!doSnap) return;
            
            DestroyImmediate(interactable.GetComponent<Throwable>());
            DestroyImmediate(interactable.GetComponent<VelocityEstimator>());
            DestroyImmediate(interactable.GetComponent<Rigidbody>());

            Rigidbody parentRigidbody = snapTransform.parent.GetComponentInParent<Rigidbody>();

            Joint[] joints = snapTransform.GetComponentsInChildren<Joint>();
            foreach (Joint joint in joints)
            {
                joint.GetComponent<Rigidbody>().rotation = snapTransform.rotation;
                joint.connectedBody = parentRigidbody;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector3.zero;
                joint.connectedAnchor = parentRigidbody.transform.InverseTransformPoint(joint.transform.position);

                StartCoroutine(DoSetBreakForce(joint));
            }
                
            Destroy(interactable);
        }

        private IEnumerator DoSetBreakForce(Joint joint)
        {
            yield return new WaitForSeconds(0.1f);

            if (joint != null)
                joint.breakForce = 2000000f;
        }

        public void Delete()
        {
            if (faker == false)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    if (!pointColliders[i].connected) continue;
                    
                    if (pointColliders[i].connectedPoint.comp.distanceFromCore > distanceFromCore) 
                        // If another component will be detached from the core, start a chain reaction
                    {
                        pointColliders[i].connectedPoint.comp.Delete();
                    }
                }

                if (core == false)
                    Destroy(gameObject);
            }
            else
            {
                if (fakerSky != null)
                {
                    RenderSettings.skybox = fakerSky;
                }
                
                Destroy(gameObject);
            }
        }

        private static Material previewDeleteSmall;

        public void PreviewDelete()
        {
            previewDeleteSmall ??= (Material)Resources.Load("PreviewDelete", typeof(Material));

            foreach (MeshFilter mesh in GetMeshesUp())
            {
                Matrix4x4 matrix = Matrix4x4.TRS(mesh.transform.position, mesh.transform.rotation,
                    mesh.transform.lossyScale);
                Graphics.DrawMesh(mesh.mesh, matrix, previewDeleteSmall, 0);
            }
        }

        public List<MeshFilter> GetMeshesUp()
        {
            List<MeshFilter> meshes = new List<MeshFilter>();

            if (core == false)
            {
                foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
                    meshes.Add(meshFilter);
            }

            for (int i = 0; i < points.Length; i++)
            {
                if (!pointColliders[i].connected) continue;
                
                if (pointColliders[i].connectedPoint.comp.distanceFromCore > distanceFromCore)// check if another component will be detached from the core. If so, start a chain reaction
                {
                    meshes.AddRange(pointColliders[i].connectedPoint.comp.GetMeshesUp());
                }
            }

            return meshes;
        }

        private CircuitPoint FindNearest(PointHitCheck[] hits)
        {
            CircuitPoint nearestPoint = null;
            float closestDistance = Mathf.Infinity;

            for (int i = 0; i < points.Length; i++)
            {
                if (hits[i] == null || hits[i].dist > closestDistance) continue;
                
                closestDistance = hits[i].dist;
                nearestPoint = hits[i].point;
                attachPoint = pointColliders[i];
            }

            return nearestPoint;
        }

        private static Material previewMaterial;

        private void RenderSnap()
        {
            Vector3 oldPos = transform.position;
            Quaternion oldRot = transform.rotation;
            
            SnapIn(transform, false);

            previewMaterial ??= (Material)Resources.Load("PreviewHologram", typeof(Material));

            Matrix4x4 matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Graphics.DrawMesh(GetComponent<MeshFilter>().mesh, matrix, previewMaterial, 0);

            // TODO: does this serve any purpose?
            transform.position = oldPos;
            transform.rotation = oldRot;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;

            foreach (SnapPoint snapPoint in points)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(snapPoint.point), radius);
                
                Gizmos.DrawLine(
                    transform.TransformPoint(snapPoint.point) + transform.TransformDirection(snapPoint.normal) * radius,
                    transform.TransformPoint(snapPoint.point) + transform.TransformDirection(snapPoint.normal) * radius * 1.8f
                );
            }
        }
    }

    public class PointHitCheck
    {
        public float dist;
        public CircuitPoint point;
        
        public PointHitCheck(float hitDistance, CircuitPoint hitPoint)
        {
            dist = hitDistance;
            point = hitPoint;
        }
    }
}
