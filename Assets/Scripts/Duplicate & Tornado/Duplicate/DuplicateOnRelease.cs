using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// After being grabbed, the object will duplicate and they will move away from one another
/// </summary>
public class DuplicateOnRelease : MonoBehaviour
{
    private OVRGrabbableExtended grabbableExt;

    public float MoveTime = 1f;

    private bool isCoRunning = false;
    private Coroutine co;

    private Transform parent;

    private bool wasXPressed = false;
    private bool CanDuplicate = false;

    private float DistanceToDuplicate;

    public bool RandomAngle;
    [DrawIf("RandomAngle", false)] public Vector3 ChosenAngle;


    [HideInInspector] public GrabEndEvent OnDoneDuplicating;

    // Start is called before the first frame update
    void Start()
    {
        grabbableExt = GetComponent<OVRGrabbableExtended>();
        grabbableExt.OnGrabBegin.AddListener(OnGrabBegin);
        parent = transform.parent;
        GetComponent<Selectable>().OnSelect.AddListener(OnSelect);
        GetComponent<Selectable>().OnDeSelect.AddListener(OnDeSelect);
        DistanceToDuplicate = GameObject.Find("Datasets").GetComponent<DatasetVarManager>().DistanceToDuplicate;
    }

    // If you press x and the object is selected, it will duplicate
    private void Update()
    {
        GetButtonPresses();
    }

    /// <summary>
    /// on grab begin, the object will stop what its doing (moving or whatever)
    /// </summary>
    void OnGrabBegin()
    {
        if (isCoRunning)
        {
            StopCoroutine(co);
            isCoRunning = false;
        }
    }

    /// <summary>
    /// On grab end, the object will start duplication process. The process involves moving away from eachother
    /// </summary>
    private void OnGrabEnd()
    {
    }

    private void Duplicate()
    {
        if (CanDuplicate)
        {
            // clone itself
            GameObject clone = Instantiate(gameObject);

            // makes its parent the same as this parent, so they all stay in the same directory
            clone.transform.parent = parent;

            Vector3 direction;
            if (RandomAngle)
            {
                direction = Random.onUnitSphere.normalized;
            }
            else
            {
                direction = Quaternion.Euler(ChosenAngle) * Vector3.forward;
            }
            // this is the distance to move, based on the scale of the object
            //float distance = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z) * 2f;

            // creates the position to move itself as well as the opposite direction for the clone
            Vector3 posToPosition = transform.position + direction * DistanceToDuplicate;
            Vector3 negToPosition = transform.position - direction * DistanceToDuplicate;

            // start the coroutine in the newly duplicated gameobject
            clone.GetComponent<DuplicateOnRelease>().co = StartCoroutine(clone.GetComponent<DuplicateOnRelease>().SnapBackHome(MoveTime, posToPosition));

            //move current object in opposite direction
            co = StartCoroutine(SnapBackHome(MoveTime, negToPosition));
        }
    }

    // This IEnumerator moves the game o
    /// <summary>
    /// Moves the gameobject to toPosition in Snaptime seconds
    /// </summary>
    /// <param name="SnapTime">time to move to position</param>
    /// <param name="toPosition">position to move</param>
    /// <returns>null hopefully</returns>
    private IEnumerator SnapBackHome(float SnapTime, Vector3 toPosition)
    {
        isCoRunning = true;

        //smaller variable to deal with, it will snap to position in a seconds
        float a = SnapTime;
        Vector3 fromPosition = gameObject.transform.position;
        Quaternion fromRotation = gameObject.transform.rotation;

        float x = 0f;
        for (float t = 0; t < a; t += Time.deltaTime)
        {
            x = (t / a) * (t / a) * (3 - 2 * t / a);
            gameObject.transform.position = Vector3.Lerp(fromPosition, toPosition, x);
            yield return null;
        }
        gameObject.transform.position = toPosition;
        yield return null;
        isCoRunning = false;

        OnDoneDuplicating.Invoke(Vector3.zero, Vector3.zero);
    }

    private void GetButtonPresses()
    {
        if (!wasXPressed && OVRInput.Get(OVRInput.RawButton.X))
        {
            wasXPressed = true;
        }

        else if (wasXPressed && !OVRInput.Get(OVRInput.RawButton.X))
        {
            wasXPressed = false;
            Debug.Log("We pressed it");
            Duplicate();
        }
    }

    private void OnSelect()
    {
        CanDuplicate = true;
    }

    private void OnDeSelect()
    {
        CanDuplicate = false;
    }
}
