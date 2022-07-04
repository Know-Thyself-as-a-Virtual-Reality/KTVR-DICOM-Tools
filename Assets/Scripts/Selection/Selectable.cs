using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

/// <summary>
/// Marks the attached dataset as selectable, selection manager only allows one to be selected at a time
/// </summary>
public class Selectable : MonoBehaviour
{
    public SelectionManager selectionManager;


    [HideInInspector]
    public UnityEvent setSelectionFalseEvent;
    [HideInInspector]
    public UnityEvent onInsideEvent;
    private bool isInside = false;

    private bool wasIndexPressed = false;
    private bool wasAPressed = false;
    private bool wasBPressed = false;
    private bool isSelected = false;

    GenericLoader genericloader;

    private bool setBrightnessNormal = false;
    private static float normal_brightness = 1.0f;
    public float min_brightness = 0.8f;
    public float max_brightness = 1.5f;
    public float brightness_per_frame = 0.5f;
    private float brightness_multiplier = 1.0f;
    
    private int indexOf_currxml = 0;
    private List<string> fnames;
    private int maxFileNum = 0;

    // need to make private later, once it feels right
    public float threshold = 0.5f;
    public float scaleChangePerFrame = 0.015f;


    void Awake()
    {
        genericloader = gameObject.GetComponent<GenericLoader>();

        // xml files location is dependant on loader script
        fnames = getFileNames(genericloader.getxmlFolderPath());

        maxFileNum = fnames.Count;
        indexOf_currxml = fnames.IndexOf(genericloader.getCurrxml());
    }

    private void Update()
    {
        // If a hand is inside, it checks for a selection
        if (isInside)
        {
            CheckForSelect();
        }
        // once the object is selected, you can change the brightness and change the objects scale
        if (isSelected)
        {
            // state machine, object is selected
            changeBrightness();
            getButtonPresses();
            ThumbstickChangeScale();
        }
        else if (setBrightnessNormal)
        {
            // Once the object is not longer selected, the brightness is set to normal
            genericloader.brightness = normal_brightness;
            setBrightnessNormal = false;
        }
        
    }

    /// <summary>
    /// Changes the scale of the object using the thumbstick of an OVR controller
    /// </summary>
    private void ThumbstickChangeScale()
    {
        if (OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y > threshold)
        {
            genericloader.volScale += scaleChangePerFrame;
        }
        else if (OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y < -threshold)
        {
            genericloader.volScale = Mathf.Max(genericloader.volScale - scaleChangePerFrame, 0.01f);
        }
    }

    public void getButtonPresses()
    {
        if (!wasAPressed && OVRInput.Get(OVRInput.RawButton.A))
        {
            wasAPressed = true;
        }

        else if (wasAPressed && !OVRInput.Get(OVRInput.RawButton.A))
        {
            wasAPressed = false;

            // Decrease transform
            if (indexOf_currxml>0)
            {
                genericloader.setCurrxml(fnames[--indexOf_currxml]);
            }
        }

        if (!wasBPressed && OVRInput.Get(OVRInput.RawButton.B))
        {
            wasBPressed = true;
        }

        else if (wasBPressed && !OVRInput.Get(OVRInput.RawButton.B))
        {
            wasBPressed = false;
            if (indexOf_currxml < maxFileNum)
            {
                genericloader.setCurrxml(fnames[++indexOf_currxml]);
            }
        }
    }

    /// <summary>
    /// changes the brightness between 2 values until unselected
    /// </summary>
    public void changeBrightness()
    {
        genericloader.brightness += Time.deltaTime * brightness_multiplier * brightness_per_frame;

        if (genericloader.brightness > max_brightness)
        {
            brightness_multiplier = -1;
        }
        else if (genericloader.brightness < min_brightness)
        {
            brightness_multiplier = 1;
        }
    }

    /// <summary>
    /// Checks to see if the object gets selected
    /// selection is defined as when you place your right hand in an object and press and release the index trigger
    /// </summary>
    public void CheckForSelect()
    {
        if (!wasIndexPressed && OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            wasIndexPressed = true;
            Debug.Log("I got pressed");
        }
        else if (wasIndexPressed && !OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            wasIndexPressed = false;
            isSelected = !isSelected;
            setBrightnessNormal = !isSelected;

            // Tells the selection manager that this object has been selected
            if (isSelected)
            {
                onInsideEvent.Invoke();
                selectionManager.OnSelection(this);
            }
        }
    }

    // checks for hand entering 
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "hand")
        {
            isInside = true;
        }
    }

    // Checks for hand exiting
    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "hand")
        {
            isInside = false;
        }
    }

    /// <summary>
    /// Gets the filenames inside folderpath
    /// </summary>
    /// <param name="folderPath">the path to the folder in question</param>
    /// <returns>a list of files inside folder</returns>
    public static List<string> getFileNames(string folderPath)
    {
        var temp = new List<string>();
        DirectoryInfo dir = new DirectoryInfo(folderPath);
        FileInfo[] info = dir.GetFiles("*.xml");

        foreach (FileInfo f in info)
        { temp.Add(f.Name); }
        return temp;
    }

    /// <summary>
    /// for use from the selection manager, to deselect when a new object gets selected
    /// </summary>
    public void SetSelectionFalse()
    {
        isSelected = false;
        setSelectionFalseEvent.Invoke();
    }
}
