#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

/// <summary>
/// This script will teleport the camera to the center eye of The OVRCamerarig and take a photo
/// </summary>
public class TeleportSnapPhotos : MonoBehaviour
{
    public GameObject[] LayerList;
    private bool CanTakeScreenShot = false;
    private int index = 0;

    public int width = 8192;
    public int height = 4096;
    private int counter;
    private bool wasBPressed = false;
    private bool wasYPressed = false;

    private GameObject teleportTo;

    public string outputPhotoDirectory;
    public void OpenExplorer()
    {
        outputPhotoDirectory = EditorUtility.OpenFolderPanel("Choose Directory", "", "");
        SetCounterVar();
    }

    private void Awake()
    {
        if (LayerList.Length > 0)
        {
            CanTakeScreenShot = true;
        }  

        SetCounterVar();
        teleportTo = GameObject.Find("CenterEyeAnchor");
    }

    private void Update()
    {
        // gets the button pressed
        getButtonPresses();
    }

    /// <summary>
    /// if b on the controller is pressed, it teleports the camera to the current position of the headset
    /// </summary>
    private void OnBPressed()
    {
        transform.SetPositionAndRotation(teleportTo.transform.position, teleportTo.transform.rotation);
        Debug.Log("Transform Position: " + teleportTo.transform.position);
        Debug.Log("Transform Rotation: " + teleportTo.transform.rotation);
    }

    /// <summary>
    /// Takes a screenshot of all the layers when y is pressed
    /// </summary>
    private void OnYPressed()
    {
        StartCoroutine(MultiLevelScreenshot());
    }

    /// <summary>
    /// it goes through and takes pictures of all the layers individually as seperate screenshots
    /// </summary>
    /// <returns></returns>
    private IEnumerator MultiLevelScreenshot()
    {
        yield return new WaitForEndOfFrame();
        // make everything go bye bye
        for (int i = 1; i < LayerList.Length; i++)
        {
            if (i < LayerList.Length)
            {
                LayerList[i].SetActive(false);
            }   
        }

        for (int i = 0; i < LayerList.Length; i++)
        {
            if (i > 0)
            {
                LayerList[i].SetActive(true);
            }
            string filename = fileName(width: width, height: height, i+1);
            FuncTakeScreenShot(width: width, height: height, filename:filename);
            //StartCoroutine(TakeScreenShot(width: width, height: height, filename:filename));
            LayerList[i].SetActive(false);
        }
        counter++;

        for (int i = 0; i < LayerList.Length; i++)
        {
            GameObject gameObject = LayerList[i];
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Invokes OnBPressed() and OnYPressed() if their corresponding button was pressed and released
    /// </summary>
    private void getButtonPresses()
    {
        if (!wasBPressed && OVRInput.Get(OVRInput.RawButton.B))
        {
            wasBPressed = true;
        }

        else if (wasBPressed && !OVRInput.Get(OVRInput.RawButton.B))
        {
            wasBPressed = false;
            OnBPressed();
        }

        if (!wasYPressed && OVRInput.Get(OVRInput.RawButton.A))
        {
            wasYPressed = true;
        }

        else if (wasYPressed && !OVRInput.Get(OVRInput.RawButton.A))
        {
            wasYPressed = false;
            OnYPressed();
        }
    }

    // Counts how many photos are in the directory, sets counter to be that number
    private void SetCounterVar()
    {
        if (outputPhotoDirectory == null || outputPhotoDirectory == "")
        {
            outputPhotoDirectory = Application.dataPath + "/Resources";
        }

        Debug.Log(outputPhotoDirectory);

        DirectoryInfo dir = new DirectoryInfo(outputPhotoDirectory);
        FileInfo[] info = dir.GetFiles("*.png");
        counter = info.Length%(LayerList.Length+1);

        Debug.Log("Screenshot Count: " + counter);
    }

    /// <summary>
    /// Gets the name of the file to save
    /// </summary>
    /// <param name="width">width in pixels of the image</param>
    /// <param name="height">height in pixels of the image</param>
    /// <param name="num">the layer number</param>
    /// <returns></returns>
    string fileName(int width, int height, int num)
    {
        string returnValue = string.Format("Image_{0}x{1}_{2}_lay_{3}.png", width, height, counter, num);
        return returnValue;
    }

    /// <summary>
    /// Takes the screenshot, saves it
    /// </summary>
    /// <param name="width">The width of the output image</param>
    /// <param name="height">The height of the output image</param>
    /// <param name="filename">The name of the filename</param>
    private void FuncTakeScreenShot(int width, int height, string filename)
    {
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one

        Camera camera = gameObject.GetComponent<Camera>();
        camera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;
        camera.Render();
        camera.fieldOfView = 60;
        Texture2D imageOverview = new Texture2D(camera.targetTexture.width, camera.targetTexture.height, TextureFormat.RGBAFloat, false);
        //Texture2D imageOverview = new Texture2D(camera.targetTexture.width, camera.targetTexture.height, TextureFormat.ASTC_HDR_12x12, false);
        imageOverview.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        imageOverview.Apply();
        RenderTexture.active = currentRT;

        // Encode texture into PNG
        byte[] bytes = imageOverview.EncodeToPNG();

        // save in memory
        //string filename = fileName(Convert.ToInt32(imageOverview.width), Convert.ToInt32(imageOverview.height));
        string path = outputPhotoDirectory + "/" + filename;
        //FileInfo file = new FileInfo(path);
        //file.Directory.Create();
        System.IO.File.WriteAllBytes(path, bytes);
    }
}
#endif  