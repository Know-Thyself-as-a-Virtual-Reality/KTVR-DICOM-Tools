using System;
using UnityEngine;
using UnityEditor;


// the editor script for the dropdownlists
[CustomPropertyDrawer(typeof(DropDownList))]
public class DropDownListDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var stringInList = attribute as DropDownList;
        var list = stringInList.List;
        if (property.propertyType == SerializedPropertyType.String)
        {
            if (list.Length != 0)
            {
                int index = Mathf.Max(0, Array.IndexOf(list, property.stringValue));
                index = EditorGUI.Popup(position, property.displayName, index, list);
                property.stringValue = list[index];
                //Debug.Log("if");
            }
            else
            {
                property.stringValue = "";
            }
        }
        else if (property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = EditorGUI.Popup(position, property.displayName, property.intValue, list);
            Debug.Log("else if");
        }
        else
        {
            base.OnGUI(position, property, label);
            Debug.Log("else");
        }
    }

    public void SetPropertyString(string inList)
    {
        var stringInList = attribute as DropDownList;
        var list = stringInList.List;
    }
}