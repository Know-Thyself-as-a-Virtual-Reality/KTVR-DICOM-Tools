using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DatasetVarManager : MonoBehaviour
{
    // Start is called before the first frame update
    [HideInInspector] public UnityEvent OnDoneTimer;

    public float TimeAfterGrabEnd = 120f;
    private bool stopTimer = false;
    private float timer = 0f;

    [Range(0,1)]
    public float DistanceToDuplicate = 0.25f;

    [Range(0,2)]
    public float DistanceBeforeDeselect = 0.5f;

    public GameObject SelectionManager;
    public GameObject OVRTrackingSpace;

    // Update is called once per frame
    void Update()
    {
        if (!stopTimer)
        {
            timer += Time.deltaTime;

            if (timer > TimeAfterGrabEnd)
            {
                stopTimer = true;
                OnDoneTimer.Invoke();
                timer = 0f;
            }
        }
    }
}
