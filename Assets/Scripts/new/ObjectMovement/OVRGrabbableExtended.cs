using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GrabEndEvent : UnityEvent<Vector3, Vector3>
{
}

// Extends OVRGrabbable to send unityevents every time the object it is attached to is grabbed or released
public class OVRGrabbableExtended : OVRGrabbable
{
    [HideInInspector] public UnityEvent OnGrabBegin;
    [HideInInspector] public GrabEndEvent OnGrabEnd;

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        OnGrabBegin.Invoke();

        base.GrabBegin(hand, grabPoint);
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        base.GrabEnd(linearVelocity, angularVelocity);
        OnGrabEnd.Invoke(linearVelocity, angularVelocity);
    }

    
}
