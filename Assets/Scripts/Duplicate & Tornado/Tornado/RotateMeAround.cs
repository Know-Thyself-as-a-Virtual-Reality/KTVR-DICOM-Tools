using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates the given gameobject around OVRAvatar
/// </summary>
public class RotateMeAround : MonoBehaviour
{
    [Range(0f, 360f)]
    public float MaxAngularVelocity = 20f;
    public float TimeToReachMaxAngularVelocity = 5f;
    private float TimeAfterGrabEnd;

    private float CurrentVelocity = 0f;
    private float timer = 0f;
    private bool CanChangeSpeed = false;

    private bool hasBeenActivated = false;

    private bool PausedAfterGrabEnd = false;
    private float TimerToStartMoving = 0f;

    private bool CanRotate = true;

    private GameObject avatar;
    private Vector3 target;
    private Vector3 axis;

    public bool RandomAxis;

    [Range(0, 180)]
    public float MaxRandomAngle;

    OVRGrabbableExtended ovrExtended;
    DuplicateOnRelease duplicateOnRelease;
    private GameObject SelectionManager;

    void Awake()
    {
        ovrExtended = GetComponent<OVRGrabbableExtended>();
        ovrExtended.OnGrabBegin.AddListener(OnGrabBegin);
        ovrExtended.OnGrabEnd.AddListener(OnGrabEnd);
        // TODO: Why is this happening here?
        duplicateOnRelease = GetComponent<DuplicateOnRelease>();
        duplicateOnRelease.OnDoneDuplicating.AddListener(OnGrabEnd);
        avatar = GameObject.Find("CenterEyeAnchor");

        SelectionManager = GameObject.Find("Datasets").GetComponent<DatasetVarManager>().SelectionManager;

        // If uncommented, then all objects will start rotating one melanix is deselected
        // SelectionManager.GetComponent<SelectionManagerTest>().ResumeAll.AddListener(ResumeAll);
        TimeAfterGrabEnd = GameObject.Find("Datasets").GetComponent<DatasetVarManager>().TimeAfterGrabEnd;

        // Figures out the axis to rotate around
        if (!RandomAxis)
        {
            axis = Vector3.up;
        }
        else
        {
            //axis = Random.insideUnitSphere.normalized;
            axis = CreateRandomAxis();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (PausedAfterGrabEnd)
        {
            TimerToStartTornado();
        }
        
        // Slowly increase speed over time
        if (CanChangeSpeed && CurrentVelocity < MaxAngularVelocity)
        {
            timer += Time.deltaTime;
            CurrentVelocity = Mathf.SmoothStep(0f, MaxAngularVelocity, timer / TimeToReachMaxAngularVelocity);
        }
        else
        {
            CanChangeSpeed = false;
        }

        if (CanRotate)
        {
            rotateNonstop();
        }
    }

    /// <summary>
    /// Timer for when the tornado is not allowed to occur
    /// </summary>
    private void TimerToStartTornado()
    {
        TimerToStartMoving += Time.deltaTime;

        if (TimerToStartMoving > TimeAfterGrabEnd)
        {
            PausedAfterGrabEnd = false;
            CanChangeSpeed = true;
            CanRotate = true;
            TimerToStartMoving = 0f;

            timer = 0f;
        }
    }

    /// <summary>
    /// When grab begins, nothing can happen
    /// </summary>
    void OnGrabBegin()
    {
        CanRotate = false;
        CanChangeSpeed = false;
    }

    /// <summary>
    /// on grab end, can start timers to move
    /// </summary>
    void OnGrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        CurrentVelocity = 0f;
        PausedAfterGrabEnd = true;
        // add duplicate
    }

    /// <summary>
    /// using angular velocity, rotate around target nonstop
    /// </summary>
    void rotateNonstop()
    {
        target = avatar.transform.position;
        transform.RotateAround(target, axis, CurrentVelocity * Time.deltaTime);
    }

    /// <summary>
    /// finds a random axis within a cone of the y axis
    /// </summary>
    /// <returns>the axis</returns>
    public Vector3 CreateRandomAxis()
    {
        float azimuth = Random.Range(0f, Mathf.Clamp(MaxRandomAngle, 0f, 180f));
        float polar = Random.Range(0f, 360f);

        azimuth *= Mathf.PI / 180f;
        polar *= Mathf.PI / 180f;

        Vector3 direction = new Vector3();

        direction.x = Mathf.Sin(azimuth) * Mathf.Cos(polar);
        direction.z = Mathf.Sin(azimuth) * Mathf.Sin(polar);
        direction.y = Mathf.Cos(azimuth);

        return direction;
    }
}
