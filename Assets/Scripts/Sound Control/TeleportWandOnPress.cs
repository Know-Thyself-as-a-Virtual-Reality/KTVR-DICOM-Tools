using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Created by Marilene Oliver, Scott Smallwood, Walter Ostrander
 * https://github.com/Know-Thyself-as-a-Virtual-Reality/KTVR-DICOM-Tools
 */

/// <summary>
/// It teleports this object to the location of the controller when a or x are pressed
/// </summary>
public class TeleportWandOnPress : MonoBehaviour
{

    public GameObject leftHandAnchor;
    public GameObject rightHandAnchor;

    private bool wasAPressed = false;
    private bool wasXPressed = false;

    private void Update()
    {
        getButtonPresses();
    }

    void teleportToRightHand()
    {
        gameObject.transform.position = rightHandAnchor.transform.position;
        gameObject.transform.rotation = rightHandAnchor.transform.rotation;
    }

    void teleportToLeftHand()
    {
        gameObject.transform.position = leftHandAnchor.transform.position;
        gameObject.transform.rotation = leftHandAnchor.transform.rotation;
    }

    /// <summary>
    /// On A or X press, the wand is teleported to the corresponding controllers position and rotation
    /// </summary>
    public void getButtonPresses()
    {

        if (!wasAPressed && OVRInput.Get(OVRInput.RawButton.A))
        {
            wasAPressed = true;
        }

        else if (wasAPressed && !OVRInput.Get(OVRInput.RawButton.A))
        {
            wasAPressed = false;
            teleportToRightHand();
        }

        if (!wasXPressed && OVRInput.Get(OVRInput.RawButton.X))
        {
            wasXPressed = true;
        }

        else if (wasXPressed && !OVRInput.Get(OVRInput.RawButton.X))
        {
            wasXPressed = false;
            teleportToLeftHand();
        }
    }

}
