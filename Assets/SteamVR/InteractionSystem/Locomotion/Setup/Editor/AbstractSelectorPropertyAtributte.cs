using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class AbstractSelectorPropertyAtributte : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty selectedItem, GUIContent label)
    {
        if (selectedItem.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, selectedItem);

            var attrib = this.attribute as AbstractSelectorAtributte;

            if (attrib != null && attrib.useDefaultFieldDrawer)
            {
                selectedItem.stringValue = EditorGUI.TagField(position, label, selectedItem.stringValue);
            }
            else
            {
                List<string> itemList = new List<string>();
                itemList.Add(getVoidSelectionValue());
                itemList.AddRange(getItemList());
                string selectedItemValue = selectedItem.stringValue;
                
                int index = 0;
                for (int i = 0; i < itemList.Count; i++)   // Check if there is an entry that matches the entry and get the index
                {
                    if (itemList[i] != selectedItemValue) continue;
                    
                    index = i;
                    break;
                }

                // Draw the popup box with the current selected index
                index = EditorGUI.Popup(position, label.text, index, itemList.ToArray());
                
                // Adjust the actual string value of the property based on the selection
                bool voidValueAvailable = getVoidSelectionValue() != null;
                selectedItem.stringValue = voidValueAvailable || index > 0 ? itemList[index] : "";
            }
            
            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, selectedItem, label);
        }
    }

    protected virtual string getVoidSelectionValue()
    {
        return null;
    }

    protected abstract string[] getItemList();
}
