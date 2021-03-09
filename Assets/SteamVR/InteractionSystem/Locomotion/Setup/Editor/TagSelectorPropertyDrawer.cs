using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TagSelectorAtributte))]
public class TagSelectorPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty selectedItem, GUIContent label)
    {
        if (selectedItem.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, selectedItem);

            var attrib = this.attribute as TagSelectorAtributte;

            if (attrib != null && attrib.useDefaultTagFieldDrawer)
            {
                selectedItem.stringValue = EditorGUI.TagField(position, label, selectedItem.stringValue);
            }
            else
            {
                // Generate the taglist + custom tags
                List<string> tagList = new List<string>();
                tagList.Add("<No tag>");
                tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);
                string selectedItemValue = selectedItem.stringValue;
                
                int index = 0;
                for (int i = 0; i < tagList.Count; i++) // Check if there is an entry that matches the entry and get the index
                {
                    if (tagList[i] != selectedItemValue) continue;
                    
                    index = i;
                    break;
                }

                // Draw the popup box with the current selected index
                index = EditorGUI.Popup(position, label.text, index, tagList.ToArray());
                
                // Adjust the actual string value of the property based on the selection
                selectedItem.stringValue = index > 0 ? tagList[index] : "";
            }
            
            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, selectedItem, label);
        }
    }
}
