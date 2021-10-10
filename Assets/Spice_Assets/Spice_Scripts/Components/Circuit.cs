using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{
    public List<CircuitComponent> components;

    public void CreateConnection(ComponentLeg legA, ComponentLeg legB)
    {
        CircuitNode nodeA = legA.node;
        CircuitNode nodeB = legB.node;

        switch (nodeA, nodeB)
        {
            case (null, null):
                CircuitNode node = new CircuitNode();
            
                legA.AttachNode(node);
                legB.AttachNode(node); 
                break;
            
            case (_, _) when nodeA == nodeB:
                return;
            
            case (null, _):
                legA.AttachNode(nodeB);
                break;
            
            case (_, null):
                legB.AttachNode(nodeA);
                break;
            
            case (_, _):
                MergeNodes(nodeA, nodeB);
                break;
        }
        
    }

    private void MergeNodes(CircuitNode nodeA, CircuitNode nodeB)
    {
        List<ComponentLeg> legsNodeB = nodeB.legs;

        foreach (ComponentLeg leg in legsNodeB)
        {
            leg.AttachNode(nodeA);
            nodeA.AttachLeg(leg);
        }
        
        nodeB.legs.Clear();
    }
}
