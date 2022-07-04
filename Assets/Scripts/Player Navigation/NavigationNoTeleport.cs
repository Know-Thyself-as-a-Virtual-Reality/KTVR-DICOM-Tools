using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationNoTeleport : MonoBehaviour
{
    [SerializeField]
    private OVRInput.Controller m_controller;

    public Rigidbody NaviBase;
    public Vector3 ThrustDirection;
    public float ThrustForce;
    private Vector3 MoveThrottle = Vector3.zero;

    [SerializeField]
    private OVRPlayerController player;
    private CharacterController character;
    private GameObject playerController;

    public GameObject startPoint;
    private GameObject mainCamera;

    GameObject cameraRig;
    GameObject TrackingSpace;
    Vector3 tempVector;
    Transform[] acs;
    Vector3[] scaling = new[] {
        new Vector3(0f, 0f, 0f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(1f, 1f, 1f),
        new Vector3(3f, 3f, 3f),
        new Vector3(6f, 6f, 6f),
        new Vector3(9f, 9f, 9f),
    };

    public float minScale = 0.5f;
    public float maxScale = 9f;
    private float scaleDiff;

    public float scalingFactor = 10f;
    
    private int index = 2;
    private int length;

    void Awake()
    {
        character = player.GetComponent<CharacterController>();
        playerController = GameObject.Find("OVRPlayerController");
    }

    void Start()
    {
        // get difference between min and max scale
        scaleDiff = maxScale - minScale;



        length = scaling.Length;
        cameraRig = GameObject.Find("OVRCameraRig");
        TrackingSpace = GameObject.Find("TrackingSpace");
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        // Transform[] ts = TrackingSpace.GetComponentsInChildren<Transform>();
        acs = new Transform[TrackingSpace.transform.childCount];

        for (int i = 0; i < TrackingSpace.transform.childCount; ++i)
        {
            acs[i] = TrackingSpace.transform.GetChild(i);
        }
    }

    void FixedUpdate()
    {

        // add force
        Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);


        if(primaryAxis.x != 0 || primaryAxis.y != 0)
        {
            // Debug.Log(primaryAxis);
            Vector3 tmp = Vector3.ProjectOnPlane(transform.rotation * new Vector3(primaryAxis.x, 0.0f, primaryAxis.y), Vector3.up);

            NaviBase.AddForce(tmp * ThrustForce);
        }
        else if (secondaryAxis.x != 0 || secondaryAxis.y != 0)
        {
            Vector3 tmp = Vector3.Project(new Vector3(secondaryAxis.x, secondaryAxis.y, 0.0f), Vector3.up);
            NaviBase.AddForce(tmp * ThrustForce);
            NaviBase.maxAngularVelocity = 2f;
        }
        else
        {
            NaviBase.velocity = Vector3.zero;
            NaviBase.angularVelocity = Vector3.zero;
            MoveThrottle = Vector3.zero;
        }

        // Increases the scale
        if (OVRInput.Get(OVRInput.RawButton.Y))
        {
            //cameraRig.gameObject.transform.localScale = Mathf.Min(cameraRig.gameObject.transform.localScale, );
        }
        // Decreases the scale
        else if (OVRInput.Get(OVRInput.RawButton.X))
        {

        }
    }

    void Update()
    {
        


        // Vector3 originPos = CenterCam.transform.position;
        if (OVRInput.GetDown(OVRInput.RawButton.X, m_controller))
        {
            Debug.Log("debug before: " + cameraRig.transform);
            if (index >= 0 && index < length - 1)
            {
                //startPoint.transform.SetPositionAndRotation(CameraRig.transform.position, CameraRig.transform.rotation);
                index = index + 1;
                cameraRig.gameObject.transform.localScale = scaling[index];
                recenter(startPoint);
            }
            Debug.Log("debug after: " + cameraRig.transform);

            // CameraRig.gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            // CenterCam.transform.position = originPos;

            //Debug.Log(acs[0].position.ToString());

            //for (int i = 0; i < acs.Length; ++i)
            //{
            //     acs[i].position = pos[i];
            //}
        }
        else if (OVRInput.GetDown(OVRInput.RawButton.Y, m_controller))
        {
            Debug.Log("debug before: " + cameraRig.transform);
            if (index > 0 && index <= length)
            {
                //startPoint.transform.SetPositionAndRotation(cameraRig.transform.position, cameraRig.transform.rotation);
                index = index - 1;
                cameraRig.gameObject.transform.localScale = scaling[index];
                recenter(startPoint);
            }
            Debug.Log("debug after: " + cameraRig.transform);
        }
        


    }

    private void fixPos()
    {
        cameraRig.transform.SetParent(null);
        playerController.transform.SetPositionAndRotation(mainCamera.transform.position, playerController.transform.rotation);
        cameraRig.transform.SetParent(playerController.transform);
    }

    public void recenter(GameObject startPoint)
    {
        startPoint = startPoint ? startPoint : GameObject.Find("StartPoint");
        if (!startPoint) return;
        fixPos();
        // OVRManager.display.RecenterPose();
        character.enabled = false;
        playerController.transform.SetPositionAndRotation(startPoint.transform.position, startPoint.transform.rotation);
        character.enabled = true;
    }

}


