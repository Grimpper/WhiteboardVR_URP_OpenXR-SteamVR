using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Random = UnityEngine.Random;

public class CircuitComponent : MonoBehaviour
{
    public string type;
    public int ID;
    public List<ComponentLeg> legs;
}
