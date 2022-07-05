using UnityEditor;
using System.IO;
using UnityEngine;

/*
 * Created by Marilene Oliver, Walter Ostrander
 * https://github.com/Know-Thyself-as-a-Virtual-Reality/KTVR-DICOM-Tools
 */

/// <summary>
/// Creates a custom editor with a "Choose directory button". When pressed, it involed OpenExplorer (line 19)
/// </summary>
[CustomEditor(typeof(TeleportSnapPhotos))]
public class OpenFolderPanelExample : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TeleportSnapPhotos myScript = (TeleportSnapPhotos)target;

        if (GUILayout.Button("Choose Directory"))
        {
            myScript.OpenExplorer();
        }
    }
}