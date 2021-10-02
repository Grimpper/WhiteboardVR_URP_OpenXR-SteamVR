using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEditor.UIElements;
using UnityEngine;
using Valve.VR.InteractionSystem;
using CustomProperties;

public class LayerSetter : MonoBehaviour
{
    [Space(10)]
    
    [Tooltip("Set parent and children to this layer")]
    [LayerSelectorAtributte] public string setBaseLayerTo;
    
    [Space(10)]

    [Tooltip("GameObjects with this tag will be set to alt layer")]
    [TagSelectorAtributte] public string alternativeTag;
    [Tooltip("Set parent and children with selected tag to this layer")]
    [LayerSelectorAtributte] public string setAlternativeLayerTo;
    
    [Space(10)]
    [Tooltip("Update layers every frame")]
    public bool updateDynamically = true;
    
    [Header("Developer Options")]
    [SerializeField] private bool showDebug = false;

    private int baseLayer;
    private int altLayer;
    private static string altLayerTag;

    void Start()
    {
        baseLayer = LayerMask.NameToLayer(setBaseLayerTo);
        altLayer =  LayerMask.NameToLayer(setAlternativeLayerTo);

        altLayerTag = alternativeTag;

        SetLayerRecursively(gameObject, baseLayer, altLayer);
        
        if (showDebug)
        {
            Debug.Log("Selected Base Layer: " + setBaseLayerTo + " (" + baseLayer + ")");
            Debug.Log("Selected Hand Layer: " + setAlternativeLayerTo + " (" + altLayer + ")");
            Debug.Log("Selected Tag: " + altLayerTag);
        }
    }

    private void Update()
    {
        if (!updateDynamically) return;
        
        baseLayer = LayerMask.NameToLayer(setBaseLayerTo);
        altLayer =  LayerMask.NameToLayer(setAlternativeLayerTo);

        altLayerTag = alternativeTag;
            
        SetLayerRecursively(gameObject, baseLayer, altLayer);
    }

    public static void SetLayerRecursively(GameObject obj, int desiredBaseLayer, int desiredAltLayer = -1)
    {
        if (altLayerTag != "" && obj.CompareTag(altLayerTag) && IsLayerValid(desiredAltLayer))
        {
            obj.layer = desiredAltLayer;
            
            foreach(Transform child in obj.transform)
            {
                // Set every child in hand to desired hand layer
                SetLayerRecursively(child.gameObject, desiredAltLayer, desiredAltLayer);
            }
        }
        else
        {
            if (IsLayerValid(desiredBaseLayer)) 
                obj.layer = desiredBaseLayer;
            
            foreach(Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, desiredBaseLayer, desiredAltLayer);
            }
        }
    }

    private static bool IsLayerValid(int layer) => layer >= 0 && layer <= 31;
}
