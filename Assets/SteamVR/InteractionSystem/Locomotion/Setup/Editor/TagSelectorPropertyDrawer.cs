using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TagSelectorAtributte : AbstractSelectorAtributte
{
    
}

[CustomPropertyDrawer(typeof(TagSelectorAtributte))]
public class TagSelectorPropertyDrawer : AbstractSelectorPropertyAtributte
{
    protected override string getVoidSelectionValue()
    {
        return "<No tag>";
    }

    protected override string[] getItemList()
    {
        return UnityEditorInternal.InternalEditorUtility.tags;
    }
}
