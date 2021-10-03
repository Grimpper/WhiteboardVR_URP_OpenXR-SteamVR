//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Flip Object to match which hand you pick it up in
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{

    public enum WhichHand
    {
        Left,
        Right
    }

    [RequireComponent(typeof(Throwable))]

    public class Equippable : MonoBehaviour
    {

        [Tooltip("Array of children you do not want to be mirrored. Text, logos, etc.")]
        public Transform[] antiFlip;

        public WhichHand defaultHand = WhichHand.Right;
        public VectorComponent axisToFlip = VectorComponent.X;

        private Interactable interactable;

        public enum VectorComponent
        {
            X,
            Y,
            Z
        }

        private bool isFlipped;
        
        [HideInInspector]
        public SteamVR_Input_Sources attachedHandType => interactable.attachedToHand
            ? interactable.attachedToHand.handType
            : SteamVR_Input_Sources.Any;

        private void Start()
        {
            // Custom code: Fix transform scaling
            //initialScale = transform.localScale;
            // Custom code: Fix transform scaling
            
            interactable = GetComponent<Interactable>();
        }

        /*private void Update()
        {
            if (!interactable.attachedToHand) return;

            Vector3 flipScale = transform.localScale;

            // Custom code: Fix transform scaling
            if (attachedHandType == SteamVR_Input_Sources.RightHand && defaultHand == WhichHand.Right ||
                attachedHandType == SteamVR_Input_Sources.LeftHand && defaultHand == WhichHand.Left)
            {
                flipScale.x *= 1;
            }
            else
            {
                flipScale.x *= -1;
                
                for (int i = 0; i < antiFlip.Length; i++)
                {
                    FlipTransform(ref antiFlip[i], VectorComponent.X);
                }
            }
            // Custom code: Fix transform scaling
            
            transform.localScale = flipScale;
        }*/

        public void FlipAxis()
        {
            switch (isFlipped)
            {
                case false when !interactable.attachedToHand:
                case false when (attachedHandType == SteamVR_Input_Sources.RightHand && defaultHand == WhichHand.Right ||
                                 attachedHandType == SteamVR_Input_Sources.LeftHand && defaultHand == WhichHand.Left):
                    return;
            }

            isFlipped = true;

            Vector3 flipScale = transform.localScale;
            
            flipScale.x *= -1;
            
            for (int i = 0; i < antiFlip.Length; i++)
                FlipTransform(ref antiFlip[i], axisToFlip);

            transform.localScale = flipScale;
        }

        private void FlipTransform(ref Transform trans, VectorComponent axisToFlip)
        {
            Vector3 transformScale = trans.localScale;

            switch (axisToFlip)
            {
                case VectorComponent.X:
                    transformScale.x *= -1;
                    break;
                case VectorComponent.Y:
                    transformScale.y *= -1;
                    break;
                case VectorComponent.Z:
                    transformScale.z *= -1;
                    break;
            }

            trans.localScale = transformScale;
        }
    }
}
