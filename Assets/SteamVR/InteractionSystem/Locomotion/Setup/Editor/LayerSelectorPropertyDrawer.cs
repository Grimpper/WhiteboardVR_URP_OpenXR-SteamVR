using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LayerSelectorAtributte : AbstractSelectorAtributte
{
    
}

[CustomPropertyDrawer(typeof(LayerSelectorAtributte))]
public class LayerSelectorPropertyDrawer : AbstractSelectorPropertyAtributte
{
    protected override string getVoidSelectionValue()
    {
        return "<No change>";
    }
    protected override string[] getItemList()
    {
        return UnityEditorInternal.InternalEditorUtility.layers;
    }
}
