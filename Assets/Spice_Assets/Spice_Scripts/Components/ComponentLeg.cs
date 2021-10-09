using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComponentLeg : MonoBehaviour
{
    public CircuitComponent component { get; private set; }
    public CircuitNode node { get; private set; }

    private void Start()
    {
        component = GetComponentInParent<CircuitComponent>();
    }

    public void AttachNode(CircuitNode nodeToAttach)
    {
        node = nodeToAttach;
    }
}
