using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEditor.UIElements;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class LayerSetter : MonoBehaviour
{
    [Tooltip("Set parent and children to this layer")]
    [SerializeField] private LayerMask setBaseLayerTo;

    [Header("Exception layer")] 
    private new static string tag = "Hand";
    [Tooltip("Set parent and children with selected tag to this layer")]
    [SerializeField] private LayerMask setAlternativeLayerTo;
    
    [Space(10)]
    [Tooltip("Update layers every frame")]
    [SerializeField] private bool updateDynamically = true;

    private int baseLayer;
    private int altLayer;

    void Start()
    {
        baseLayer = (int) Math.Log(setBaseLayerTo.value, 2);
        altLayer = (int) Math.Log(setAlternativeLayerTo.value, 2);
        
        Debug.Log("Selected Base Layer: " + baseLayer);
        Debug.Log("Selected Hand Layer: " + altLayer);

        SetLayerRecursively(gameObject, baseLayer, altLayer);
    }

    private void Update()
    {
        if (updateDynamically)
        {
            SetLayerRecursively(gameObject, baseLayer, altLayer);
        }
    }

    public static void SetLayerRecursively(GameObject obj, int desiredBaseLayer, int desiredHandLayer = -1)
    {
        if (obj.CompareTag(tag) && (desiredHandLayer >= 0 && desiredHandLayer <= 31))
        {
            obj.layer = desiredHandLayer;
            
            foreach(Transform child in obj.transform)
            {
                // Set every child in hand to desired hand layer
                SetLayerRecursively(child.gameObject, desiredHandLayer, desiredHandLayer);
            }
        }
        else
        {
            if (desiredBaseLayer >= 0 && desiredBaseLayer <= 31) obj.layer = desiredBaseLayer;
            
            foreach(Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, desiredBaseLayer, desiredHandLayer);
            }
        }
    }

}
