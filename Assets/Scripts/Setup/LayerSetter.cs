using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSetter : MonoBehaviour
{
    [SerializeField] private LayerMask setLayerTo;
    //private int layer = (int) Math.Log(setLayerTo.value, 2);
    void Awake()
    {
        Debug.Log("Layer: " + setLayerTo.value);
        SetLayerRecursively(this.gameObject, 6);
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
