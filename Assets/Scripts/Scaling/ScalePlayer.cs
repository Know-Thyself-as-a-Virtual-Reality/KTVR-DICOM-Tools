using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * Created by Marilene Oliver, Walter Ostrander, Jiayi Yi
 * https://github.com/Know-Thyself-as-a-Virtual-Reality/KTVR-DICOM-Tools
 */

public class ScalePlayer : MonoBehaviour
{
    [SerializeField]
    private OVRInput.Controller mainController;

    // The parent of all the objects
    public GameObject cameraRig;
    public GameObject mainCamera;

    public CharacterController character;
    private GameObject playerController;

    // Object to pivot around when scaling
    public GameObject pivotObject;

    public float minScale = 0.5f;
    public float maxScale = 9f;
    private float scaleDiff;

    // The amount you scale per fixed update
    public float scalingFactor = 0.05f;
    
    private Vector3 position;
    

    // Start is called before the first frame update
    void Start()
    {
        scaleDiff = maxScale - minScale;

        if (cameraRig == null)
        {
            throw new Exception("You need to specify the object you want to scale");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ScalePlayerTest();
    }

    private void fixPos()
    {
        cameraRig.transform.SetParent(null);
        playerController.transform.SetPositionAndRotation(mainCamera.transform.position, playerController.transform.rotation);
        cameraRig.transform.SetParent(playerController.transform);
    }

    public void recenter(Vector3 position)
    {
        //fixPos();
        // OVRManager.display.RecenterPose();
        character.enabled = false;
        playerController.transform.position = position;
        character.enabled = true;
    }

    void ScalePlayerTest()
    {
        // Increases the scale
        if (OVRInput.Get(OVRInput.RawButton.Y, mainController))
        {
            float scale = cameraRig.transform.localScale.x;

            //scale = Mathf.Min(scale + scaleDiff * scalingFactor, maxScale);
            scale = Mathf.Clamp(scale * (scalingFactor + 1.0f), Mathf.Max(minScale, 0.01f), maxScale);

            
            //scalearound(objecttoscale, pivotobject.transform.position, new vector3(scale, scale, scale), true);

            Vector3 position = cameraRig.transform.position;

            //cameraRig.transform.localScale = new Vector3(scale, scale, scale);
            ScaleAround(cameraRig, mainCamera.transform.position, new Vector3(scale, scale, scale), false);

            recenter(position);
            cameraRig.transform.position = position;
        }
        // Decreases the scale
        else if (OVRInput.Get(OVRInput.RawButton.X, mainController))
        {
            //// The scaling
            float scale = cameraRig.transform.localScale.x;
            //scale = Mathf.Max(scale - scaleDiff * scalingFactor, minScale);
            scale = Mathf.Clamp(scale / (scalingFactor + 1.0f), Mathf.Max(minScale, 0.01f), maxScale);
            //ScaleAround(objectToScale, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(scale, scale, scale), true);
            //ScaleAround(objectToScale, pivotObject.transform.position, new Vector3(scale, scale, scale), true);
            Vector3 position = cameraRig.transform.position;

            //cameraRig.transform.localScale = new Vector3(scale, scale, scale);
            ScaleAround(cameraRig, mainCamera.transform.position, new Vector3(scale, scale, scale), false);

            recenter(position);
            cameraRig.transform.position = position;
            ////
        }
    }


    public static void ScaleAround(GameObject target, Vector3 pivotWorld, Vector3 newScale, bool globalSpace)
    {
        Vector3 A = target.transform.localPosition;
        Vector3 B;
        if (globalSpace)
        {
            B = pivotWorld;
        }
        else
        {
            Transform parent = target.transform.parent;
            if (parent == null)
            {
                Debug.Log("parent is nullll");
                B = pivotWorld;
            }
            else
            {
                B = parent.InverseTransformPoint(pivotWorld);
            }
            
        }

        Vector3 C = A - B; // diff from object pivot to desired pivot/origin

        float RS = newScale.x / target.transform.localScale.x; // relative scale factor

        // calc final position post-scale
        Vector3 FP = B + C * RS;

        // finally, actually perform the scale/translation
        target.transform.localScale = newScale;
        target.transform.localPosition = FP;
    }

}
