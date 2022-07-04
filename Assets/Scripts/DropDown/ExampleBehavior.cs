using System;
using System.Collections.Generic;
using UnityEngine;

public class ExampleBehavior : MonoBehaviour
{
    [DropDownList("Cat", "Dog")] public string Animal;

    //using a method 
    [DropDownList(typeof(ExamplePropertyDrawersHelper), "methodExample")]
    public string methodExample;

    //using custom method to find filenames
    [DropDownList(typeof(CTDrawerHelper), "getFileNames")]
    public string TransChoice;
}

public class DropDownList : PropertyAttribute
{
    public delegate string[] GetStringList();
    public DropDownList(params string[] list)
    {
        List = list;
    }
    public DropDownList(Type type, string methodName)
    {
        var method = type.GetMethod(methodName);
        if (method != null)
        {
            List = method.Invoke(null, null) as string[];
        }
        else
        {
            Debug.LogError("NO SUCH METHOD " + methodName + " FOR " + type);
        }
    }
    public string[] List;
}