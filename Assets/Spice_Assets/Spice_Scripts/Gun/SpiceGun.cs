using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SpiceGun : MonoBehaviour
{
    [SerializeField] private SteamVR_ActionSet actionSet;
    [SerializeField] private SteamVR_Action_Single actionTrigger =
        SteamVR_Input.GetAction<SteamVR_Action_Single>("spice_gun", "Trigger");

    private Interactable interactable;

    [SerializeField] private Transform gunTip;
    
    // Line renderer variables
    public LineRenderer laserRenderer;
    [SerializeField] private float laserWidth = 0.1f;
    public float laserMaxLength = 5f;

    [Space] 
    [SerializeField] private bool debug = false;

    private void Start()
    {
        actionSet.Activate();
        
        interactable = GetComponent<Interactable>();
        
        Vector3[] initLaserPositions = {Vector3.zero, Vector3.zero};
        laserRenderer.SetPositions(initLaserPositions);
        laserRenderer.startWidth = laserRenderer.endWidth = laserWidth;
    }

    private void Update()
    {
        float trigger = 0;
        
        if (interactable.attachedToHand)
        {
            SteamVR_Input_Sources hand = interactable.attachedToHand.handType;

            trigger = actionTrigger.GetAxis(hand);
        }

        if (trigger > 0.7)
        {
            laserRenderer.enabled = true;
            FireRay(gunTip.position, gunTip.transform.right, laserMaxLength);
        }
        else
        {
            laserRenderer.enabled = false;
        }
    }
    
    private void FireRay(Vector3 startPos, Vector3 direction, float length )
    {
        Vector3 endPos = startPos + direction * laserMaxLength;
        
        RaycastHit hit;
        Ray ray = new Ray(startPos, direction);

        if (Physics.Raycast(ray, out hit, laserMaxLength))
        {
            endPos = hit.point;
            
            if (debug) 
                Debug.Log("Hit: " + hit.collider.gameObject.name);
        }
        
        laserRenderer.SetPosition(0, startPos );
        laserRenderer.SetPosition(1, endPos);

    }
}
