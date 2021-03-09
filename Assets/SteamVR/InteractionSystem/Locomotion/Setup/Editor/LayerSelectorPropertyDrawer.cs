using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LayerSelectorAtributte))]
public class LayerSelectorPropertyDrawer : AbstractSelectorPropertyAtributte
{
    protected override string[] getItemList()
    {
        return UnityEditorInternal.InternalEditorUtility.layers;
    }
}
