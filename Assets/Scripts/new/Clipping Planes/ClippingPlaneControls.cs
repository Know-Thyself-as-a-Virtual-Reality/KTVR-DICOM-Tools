using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// Uses the VR controller inputs to control the clipping planes
public class ClippingPlaneControls : MonoBehaviour
{
    private GenericLoader loader;

    private bool isLeftIndexPressed = false;
    private bool isRightIndexPressed = false;

    private Vector3 leftIndexPressLocation;
    private Vector3 rightIndexPressLocation;

    public float MaxClipPerFrame = 0.05f;
    public float ScaleOfClipping = 1f;
    public float threshold = 0f;

    public GameObject OVRTrackingSpace;

    private float dim1min;
    private float dim1max;
    private float dim2min;
    private float dim2max;
    private float dim3min;
    private float dim3max;

    private bool ScaleIsInverted = false;

    private void Awake()
    {
        loader = gameObject.GetComponent<GenericLoader>();
        loader.ScaleNegativeEvent.AddListener(InvertedScale);
    }

    // If Scale is inverted, some of the controls are also inverted
    private void InvertedScale()
    {
        ScaleIsInverted = true;
    }

    // Update is called once per frame
    void Update()
    {
        // index buttons control when you can modify clipping planes
        CheckIndexPresses();

        if (isLeftIndexPressed)
        {
            // Grabs the which direction the palm is facing
            Quaternion tempQua = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);

            // Left hand, so palm would be facing right
            Vector3 localPalmDir = tempQua * Vector3.right;

            // Chang the direction so that it is in refference to world space
            Vector3 wSpaceDir = OVRTrackingSpace.transform.TransformDirection(localPalmDir);

            controlClipDimsWithPalmDirection(wSpaceDir, true);
        }
        if (isRightIndexPressed)
        {
            // All the same as above, but inverted (tempQua * -Vector3.right) vs (tempQua * Vector3.right)
            Quaternion tempQua = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            Vector3 localPalmDir = tempQua * -Vector3.right;
            Vector3 wSpaceDir = OVRTrackingSpace.transform.TransformDirection(localPalmDir);
            controlClipDimsWithPalmDirection(wSpaceDir, false);
        }
    }

    /// <summary>
    /// Takes the palm direction, then grabs the distance from initial index press. It then modifies the clipping plane corresponding to the direction the palm is facing 
    /// proportionally to the distance in the palm facing direction that the hand has moved
    /// 
    /// It basically clips away the body in the direction your palm is facing. Funky math and stuff.
    /// </summary>
    /// <param name="wSpaceDirection">Direction the palm is facing</param>
    /// <param name="leftHand">whether or no the hand is left</param>
    private void controlClipDimsWithPalmDirection(Vector3 wSpaceDirection, bool leftHand)
    {
        Vector3 deltaLocation;
        // Grabs the given hands distance from where the index was pressed
        if (leftHand)
        {
            Vector3 CurrLocation = transform.InverseTransformPoint(OVRTrackingSpace.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch)));
            deltaLocation = CurrLocation - leftIndexPressLocation;
        }
        else
        {
            Vector3 CurrLocation = transform.InverseTransformPoint(OVRTrackingSpace.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch)));
            deltaLocation = CurrLocation - rightIndexPressLocation;
        }

        // changes the spacial refference for direction to this gameobject's transform
        Vector3 LocalDirection = transform.InverseTransformDirection(wSpaceDirection);

        // if the scale is inverted, then the x direction needs to be inverted for the clipping to work properly
        if (ScaleIsInverted)
        {
            LocalDirection.x = -LocalDirection.x;
        }

        // absolute values to check max direction
        float x = Mathf.Abs(LocalDirection.x);
        float y = Mathf.Abs(LocalDirection.y);
        float z = Mathf.Abs(LocalDirection.z);
        float max = Mathf.Max(x, y, z);

        // basically sets the clipping plane proportionally to the distance moved in the direction your palm is facing
        if (x == max)
        {
            if (LocalDirection.x >= 0)
            {
                loader.setDim1Min(dim1min + deltaLocation.x * ScaleOfClipping);
            }
            else
            {
                loader.setDim1Max(dim1max + deltaLocation.x * ScaleOfClipping);
            }
        }
        else if (y == max)
        {
            if (LocalDirection.y >= 0)
            {
                loader.setDim2Min(dim2min + deltaLocation.y * ScaleOfClipping);
            }
            else
            {
                loader.setDim2Max(dim2max + deltaLocation.y * ScaleOfClipping);
            }
        }
        else
        {
            if (LocalDirection.z >= 0)
            {
                loader.setDim3Min(dim3min + deltaLocation.z * ScaleOfClipping);
            }
            else
            {
                loader.setDim3Max(dim3max + deltaLocation.z * ScaleOfClipping);
            }
        }
    }

    /// <summary>
    /// If the index trigger gets pressed, then the location of the index press is grabbed, for use in controlClipDimsWithPalmDirection()
    /// if the index gets released, the cliping is stopped
    /// </summary>
    private void CheckIndexPresses()
    {
        if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && !isLeftIndexPressed)
        {
            isLeftIndexPressed = true;
            leftIndexPressLocation = transform.InverseTransformPoint(OVRTrackingSpace.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch)));
            getDims();
        }
        else if (!OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && isLeftIndexPressed)
        {
            isLeftIndexPressed = false;
        }

        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && !isRightIndexPressed)
        {
            isRightIndexPressed = true;
            rightIndexPressLocation = transform.InverseTransformPoint(OVRTrackingSpace.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch)));
            getDims();
        }
        else if (!OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && isRightIndexPressed)
        {
            isRightIndexPressed = false;
        }
    }

    /// <summary>
    /// gets the dimensions of the clipping planes, so they can be used in controlClipDimsWithPalmDirection()
    /// </summary>
    private void getDims()
    {
        dim1min = loader.getDim1Min();
        dim2min = loader.getDim2Min();
        dim3min = loader.getDim3Min();

        dim1max = loader.getDim1Max();
        dim2max = loader.getDim2Max();
        dim3max = loader.getDim3Max();
    }
}
