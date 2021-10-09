using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assembler;
using Valve.VR.InteractionSystem;
using Valve.VR;

public class DeleterTool : MonoBehaviour
{
    public LineRenderer laserbeam;
    public LineRenderer aimer;

    //float brightness = 1;

    //[HideInInspector]
    public DeleterStatus status; 

    public AudioClip au_error;
    public AudioClip au_select;
    public AudioClip au_zap;

    public AudioSource au;

    public GameObject confirmer;

    public Transform ptr;
    
    public SteamVR_Action_Boolean actionSelect;
    public SteamVR_Action_Boolean actionDelete;


    private Rigidbody rb;
    private IEnumerator cycler;

    private AssemblerComponent hitAssemblerComponent;

    private Transform hitTr;
    private Interactable interactable;

    private Vector3 lockedPosition;


    private void Start()
    {
        interactable = GetComponent<Interactable>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (interactable.attachedToHand)
        {
            // Custom code:
            //transform.parent = null;
            // Custom code:
            
            rb.isKinematic = false;

            if (cycler == null)
            {
                cycler = CycleModes();
                StartCoroutine(cycler);
            }

            float dist = 200;
            bool didHit = Physics.Raycast(aimer.transform.position, aimer.transform.forward, 
                out RaycastHit hit);
            
            if (didHit)
            {
                dist = hit.distance;
            }

            confirmer.SetActive(status == DeleterStatus.Locked);
            
            switch (status)
            {
                case DeleterStatus.Aiming:
                    laserbeam.transform.rotation = aimer.transform.rotation;
                    hitAssemblerComponent = hit.collider.GetComponentInParent<AssemblerComponent>();
                
                    switch (didHit)
                    {
                        case true when (hitAssemblerComponent != null && hitAssemblerComponent.enabled):
                            hitTr = hit.transform;
                            lockedPosition = hit.transform.InverseTransformPoint(hit.point);
                            hitAssemblerComponent.PreviewDelete();
                            break;
                    
                        case false:
                            hitAssemblerComponent = null;
                            break;
                    }

                    laserbeam.gameObject.SetActive(false);
                    aimer.gameObject.SetActive(true);
                    aimer.SetPosition(1, Vector3.forward * dist / aimer.transform.lossyScale.z);
                    break;
                
                case DeleterStatus.Locked:
                {
                    Vector3 worldLock = hitTr.transform.TransformPoint(lockedPosition);
                    laserbeam.gameObject.SetActive(true);
                    laserbeam.transform.rotation = Quaternion.LookRotation(worldLock - laserbeam.transform.position);

                    laserbeam.SetPosition(1, laserbeam.transform.InverseTransformPoint(worldLock));
                    aimer.gameObject.SetActive(false);
                    hitAssemblerComponent.PreviewDelete();

                    if (Vector3.Angle(aimer.transform.forward, worldLock - laserbeam.transform.position) > 30) // angle to break lock
                    {
                        status = DeleterStatus.Aiming; //reset
                        au.PlayOneShot(au_error);
                    }

                    break;
                }
                
                case DeleterStatus.Zapping:
                    laserbeam.gameObject.SetActive(true);
                    laserbeam.SetPosition(1, Vector3.forward * dist / aimer.transform.lossyScale.z);
                    aimer.gameObject.SetActive(false);
                    break;
                
                case DeleterStatus.Cancelling:
                    laserbeam.gameObject.SetActive(true);
                    laserbeam.SetPosition(1, Vector3.forward * dist / aimer.transform.lossyScale.z);
                    aimer.gameObject.SetActive(false);
                    break;
            }
        }
        else
        {
            hitAssemblerComponent = null;
            if (cycler != null)
            {
                StopCoroutine(cycler);
                cycler = null;
            }

            status = DeleterStatus.Aiming;
            laserbeam.gameObject.SetActive(false);
            aimer.gameObject.SetActive(false);
            confirmer.SetActive(false);
        }
    }

    private IEnumerator CycleModes()
    {
        while (true)
        {
            while (status == DeleterStatus.Aiming)
            {
                if (interactable.attachedToHand)
                {
                    if (actionSelect.GetState(interactable.attachedToHand.handType))
                    {
                        if (hitAssemblerComponent != null)
                        {
                            status = DeleterStatus.Locked; // lock on
                            au.PlayOneShot(au_select);
                        }
                        else
                        {
                            status = DeleterStatus.Cancelling; // cancel
                            au.PlayOneShot(au_error);
                        }
                    }
                }
                yield return null;
            }

            while (status == DeleterStatus.Locked)
            {
                if (interactable.attachedToHand)
                {
                    if (actionDelete.GetState(interactable.attachedToHand.handType))
                    {
                        if (hitAssemblerComponent)
                            hitAssemblerComponent.Delete();
                        
                        hitAssemblerComponent = null;
                        status = 0; // reset
                        au.PlayOneShot(au_zap);
                    }
                }
                yield return null;
            }

            while (status == DeleterStatus.Zapping)
            {
                float zapTime = 2;
                while (zapTime > 0)
                {
                    zapTime -= Time.deltaTime * 4;
                    SetColor(zapTime);
                    yield return null;
                }
                
                status = DeleterStatus.Aiming; // reset
                yield return null;
                SetColor(1);
            }
            
            while (status == DeleterStatus.Cancelling)
            {
                float cancelTime = 1;
                while (cancelTime > 0)
                {
                    cancelTime -= Time.deltaTime * 4;
                    SetColor(cancelTime);
                    yield return null;
                }
                
                status = DeleterStatus.Aiming; //reset
                yield return null;
                SetColor(1);
            }

            while (interactable.attachedToHand != null && actionSelect.GetState(interactable.attachedToHand.handType))
            {
                yield return null;
            }
        }
    }

    private void SetColor(float f)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white * f, 0.0f), 
                new GradientColorKey(Color.white * f, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0.0f), 
                new GradientAlphaKey(1, 1.0f) }
        );
        laserbeam.colorGradient = gradient;
    }

    public enum DeleterStatus
    {
        Aiming,
        Locked,
        Zapping,
        Cancelling,
    }
}