using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitNode
{
    public List<ComponentLeg> legs { get; private set; }

    public void AttachLeg(ComponentLeg legToAttach)
    {
        legs.Add(legToAttach);
    }
}
