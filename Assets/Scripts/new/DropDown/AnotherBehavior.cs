using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnotherBehavior : MonoBehaviour
{

    //using custom method to find filenames
    [DropDownList(typeof(CTDrawerHelper), "getFileNames")]
    public string TransChoice;
}
