using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SpiceWand : MonoBehaviour
{
    [SerializeField] public SteamVR_Action_Boolean actionConnect;

    private Interactable interactable;

    private void Update()
    {
        if (actionConnect.GetLastStateDown(interactable.attachedToHand.handType))
        {
            Physics.Raycast(transform.position, transform.forward, out RaycastHit hit);

            
        }
    }
}
