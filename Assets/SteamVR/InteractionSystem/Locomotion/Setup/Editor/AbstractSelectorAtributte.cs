using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class AbstractSelectorAtributte : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty selectedItem, GUIContent label)
    {
        if (selectedItem.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, selectedItem);

            var attrib = this.attribute as LayerSelectorAtributte;

            if (attrib != null && attrib.useDefaultLayerFieldDrawer)
            {
                selectedItem.stringValue = EditorGUI.TagField(position, label, selectedItem.stringValue);
            }
            else
            {
                // Generate the layer list + custom layer
                List<string> layerList = new List<string>();
                layerList.Add("<No layer>");
                layerList.AddRange(UnityEditorInternal.InternalEditorUtility.layers);
                string selectedItemValue = selectedItem.stringValue;
                
                int index = 0;
                for (int i = 0; i < layerList.Count; i++)   // Check if there is an entry that matches the entry and get the index
                {
                    if (layerList[i] != selectedItemValue) continue;
                    
                    index = i;
                    break;
                }

                // Draw the popup box with the current selected index
                index = EditorGUI.Popup(position, label.text, index, layerList.ToArray());
                
                // Adjust the actual string value of the property based on the selection
                selectedItem.stringValue = layerList[index];
            }
            
            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, selectedItem, label);
        }
    }
}
