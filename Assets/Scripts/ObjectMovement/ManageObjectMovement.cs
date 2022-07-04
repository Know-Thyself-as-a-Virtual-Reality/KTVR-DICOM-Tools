using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Events;
using System;


// Needs to be used with OVRGrabbableExtended
// after being released from a grab, an object will wait (StartFloatingAwayIn) seconds, then float away in a random direction for (StartComingBackIn - StartFloatingAwayIn) seconds
// It will float in a random direction, then come back to its original location in (TimeToReturn) seconds.
// However, if it is close enough to its orginal starting location when released (DistanceToSnap), it will start returning in (SnapBackTime) seconds.
public class ManageObjectMovement : MonoBehaviour
{
    //public float MaxFloatAwayAcceleration = 0.1f;
    //public float AngularMultiplier = 0.25f;

    public float TimeToReturn = 15f;

    //public float StartFloatingAwayIn = 30.0f;
    public float StartComingBackIn = 120.0f;

    private float myTimer = 0.0f;

    //private bool startFloatingAway = false;
    private bool startComingBack = false;

    private bool canStartTimers = false;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private Vector3 randomDirectionalAcceleration;
    private Vector3 randomAngularAcceleration;

    private Rigidbody rb;
    private OVRGrabbableExtended ovrExtended;
    public bool SnapInPlace = true;
    public float DistanceToSnap = 0.3f;
    public float SnapBackTime = 0.2f;

    private bool isGrabbed = false;
    private Coroutine co;
    private Coroutine snap;
    private bool isCoRunning = false;

    public bool canThrow = true;
    public float minSpeedToThrow = 1.0f; 

    [HideInInspector] public UnityEvent OnSnapBackStart;
    [HideInInspector] public UnityEvent OnSnapBackEnd;
    [HideInInspector] public UnityEvent OnFloatBackStart;
    [HideInInspector] public UnityEvent OnFloatBackEnd;


    

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;

        ovrExtended = gameObject.GetComponent<OVRGrabbableExtended>();
        ovrExtended.OnGrabBegin.AddListener(GrabBegin);
        ovrExtended.OnGrabEnd.AddListener(GrabEnd);
    }

    private void Update()
    {

        // Can start timers is true when an object is released
        if (canStartTimers)
        {
            myTimer += Time.deltaTime;


            if (!startComingBack && myTimer > StartComingBackIn)
            {
                // make come back
                startComingBack = true;
                InitFloatBack();
            }
            // starts floating away if enough time has passed
            //else if (!startFloatingAway && myTimer > StartFloatingAwayIn)
            //{
            //    // allow to float away
            //    InitFloatAway();
            //    startFloatingAway = true;
            //}

            // starts floating back if enough time has passed
            
        }

        // floats away in random direction with random spin
        //if (startFloatingAway)
        //{
        //    rb.velocity += randomDirectionalAcceleration;
        //    rb.angularVelocity += randomAngularAcceleration;
        //}

        // starts lerping back to initial location
        if (startComingBack && !isCoRunning)
        {
            startComingBack = false;
            co = StartCoroutine(SnapBackHome(TimeToReturn, false));
            OnFloatBackStart.Invoke();
        }

        // if it is currently grabbed, and close to starting location, scott can play a noise
        if (isGrabbed && isCloseToStartLocation())
        {
            ScottsIndicationNoise();
        }
    }

    // chooses a random vector with bounds (min, max)
    private Vector3 RandomVector(float min, float max)
    {
        var x = Random.Range(min, max);
        var y = Random.Range(min, max);
        var z = Random.Range(min, max);
        return new Vector3(x, y, z);
    }

    // Changes variables to initialize the floating back process
    public void InitFloatBack()
    {
        canStartTimers = false;
        //startFloatingAway = false;

        rb.isKinematic = true;
        setMomentumZero();
    }

    //// Changes variables to initialize floating away
    //public void InitFloatAway()
    //{
    //    rb.isKinematic = false;
    //    setMomentumZero();

    //    // Gather random vectors for float away
    //    randomDirectionalAcceleration = RandomVector(-MaxFloatAwayAcceleration, MaxFloatAwayAcceleration);
    //    randomAngularAcceleration = RandomVector(-MaxFloatAwayAcceleration * AngularMultiplier, MaxFloatAwayAcceleration * AngularMultiplier);
    //}

    // on grab end, checks whether the object is close enough to snap, else it starts the timers for float away process
    public void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        isGrabbed = false;

        if (SnapInPlace && isCloseToStartLocation())
        {
            co = StartCoroutine(SnapBackHome(SnapBackTime));
            OnSnapBackStart.Invoke();
        }
        else if (linearVelocity.magnitude > minSpeedToThrow)
        {
            startThrow(linearVelocity, angularVelocity);
        }
        else
        {
            // allow gameobjects to be thrown
            

            StartTimers();
        }
    }

    // used to allow a gameobject to use physics for until it comes to a stop
    private void startThrow(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        rb.isKinematic = false;
        rb.velocity = linearVelocity;
        rb.angularVelocity = angularVelocity;
        StartCoroutine(MonitorVelocity());
    }

    // checks if distance is less than Distance to snap
    private bool isCloseToStartLocation()
    {
        return Vector3.Distance(startPosition, gameObject.transform.position) < DistanceToSnap;
    }

    // starts the timers for floating away/floating back, changes variables to re-initiate the timers process thingy
    public void StartTimers()
    {
        myTimer = 0.0f;
        //startFloatingAway = false;
        startComingBack = false;
        canStartTimers = true;
    }

    // pretty self explanetary, sets velocity and angular velocity to zero
    private void setMomentumZero()
    {
        rb.velocity = new Vector3(0f, 0f, 0f);
        rb.angularVelocity = new Vector3(0f, 0f, 0f);
    }

    // If a grab is initiated, stops any snap coroutine that is currently in effect
    // makes sure all variables don't affect grabs
    public void GrabBegin()
    {
        if (isCoRunning)
        {
            StopCoroutine(co);
            isCoRunning = false;
        }
        isGrabbed = true;
        startComingBack = false;

        canStartTimers = false;
        //startFloatingAway = false;

        rb.isKinematic = true;
        setMomentumZero();
    }

    // Will Turn off physics once it comes to a stop
    private IEnumerator MonitorVelocity()
    {
        while (!Mathf.Approximately(rb.velocity.magnitude, 0.0f) && !Mathf.Approximately(rb.angularVelocity.magnitude, 0.0f))
        {
            yield return null;
        }
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        StartTimers();
    }

    // Linear movement in (time) seconds back to initial position/rotation grabbed in Awake()
    private IEnumerator LerpToStart(float time)
    {
        Vector3 fromPosition = gameObject.transform.position;
        Quaternion fromRotation = gameObject.transform.rotation;

        for (float t = 0f; t < 1f; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(fromPosition, startPosition, t / time);
            transform.rotation = Quaternion.Lerp(fromRotation, startRotation, t / time);
            yield return null;
        }
        transform.position = fromPosition;
        transform.rotation = fromRotation;
    }

    // basically the same as lerpToStart, however it moves more slowly at start and finish. (Moves with funky speed to look more fluid?)
    private IEnumerator SnapBackHome(float SnapTime, bool snap = true)
    {
        isCoRunning = true;

        //smaller variable to deal with, it will snap to position in a seconds
        float a = SnapTime;
        Vector3 fromPosition = gameObject.transform.position;
        Quaternion fromRotation = gameObject.transform.rotation;

        float x = 0f;
        for (float t = 0; t < a; t+=Time.deltaTime) 
        {
            x = (t / a) * (t / a) * (3 - 2 * t / a);
            gameObject.transform.position = Vector3.Lerp(fromPosition, startPosition, x);
            gameObject.transform.rotation = Quaternion.Lerp(fromRotation, startRotation, x);
            yield return null;
        }
        gameObject.transform.position = startPosition;
        gameObject.transform.rotation = startRotation;
        yield return null;
        isCoRunning = false;
        if (snap)
        {
            OnSnapBackEnd.Invoke();
        }
        else
        {
            OnFloatBackEnd.Invoke();
        }
    }

    // indicator for when the object is close to its starting location while being grabbed
    private void ScottsIndicationNoise()
    {
        Debug.Log("Close Enough To Snap");
    }

    private void returnEverthingBackTosStartLocation()
    {

    }
}
