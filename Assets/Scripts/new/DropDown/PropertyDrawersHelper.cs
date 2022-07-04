using System.Collections.Generic;
using UnityEngine;

public static class ExamplePropertyDrawersHelper
{
    public static string[] methodExample()
    {
        var temp = new List<string>();
        temp.Add("Example");
        temp.Add("Second");
        return temp.ToArray();
    }
}