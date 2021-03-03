using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

public class LayerSetter : MonoBehaviour
{
    [SerializeField] private LayerMask setLayerTo;

    void Start()
    { 
        int layer = (int) Math.Log(setLayerTo.value, 2);
        Debug.Log("Selected Layer: " + layer);
        SetLayerRecursively(gameObject, layer);
    }

    void SetLayerRecursively(GameObject obj, int desiredLayer) 
    {
        obj.layer = desiredLayer;

        foreach(Transform child in obj.transform )
        {
            SetLayerRecursively(child.gameObject, desiredLayer);
        }
    }

}
