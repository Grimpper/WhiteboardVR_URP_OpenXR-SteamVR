using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class LayerSetter : MonoBehaviour
{
    [SerializeField] private LayerMask setLayerTo;
    [SerializeField] private bool updateDynamically = true;

    private int layer;

    void Start()
    {
        layer = (int) Math.Log(setLayerTo.value, 2);
        Debug.Log("Selected Layer: " + layer);
        SetLayerRecursively(gameObject, layer);
    }

    private void Update()
    {
        if (updateDynamically)
        {
            SetLayerRecursively(gameObject, layer);
        }
    }

    public static void SetLayerRecursively(GameObject obj, int desiredLayer) 
    {
        obj.layer = desiredLayer;

        foreach(Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, desiredLayer);
        }
    }

}
