using System.Collections;using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class CTDrawerHelper
{
    public static string[] getFileNames()
    {
        var temp = new List<string>();

        string myPath = "Assets/Resources/color-transfer-functions";
        DirectoryInfo dir = new DirectoryInfo(myPath);
        FileInfo[] info = dir.GetFiles("*.xml");

        foreach (FileInfo f in info)
        {temp.Add(f.Name);}
        return temp.ToArray();
    }
}
