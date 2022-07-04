using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// controls the sound based off the the location inside the body we are
/// </summary>
public class SendLocationControlSound : MonoBehaviour
{
    public bool canControlSound = false;

    public GameObject bodyObject;
    private AudioSource audioSource;

    public bool isPlaying = false;

    public float minPitch = 1.0f;
    public float maxPitch = 3.0f;

    public float normal_volume = 1.0f;

    private SoundControlByVoxelUtil musicScript;

    private void Awake()
    {
        // Gets the body from which we are trying to get the sound
        musicScript = bodyObject.GetComponent<SoundControlByVoxelUtil>();

        musicScript.canControlSoundEvent.AddListener(canControlSoundEvent);

        audioSource = this.GetComponent<AudioSource>();
    }


    // Update is called once per frame
    void Update()
    {
        // If it is inside the body, it plays a sound based on the closest voxel
        if (musicScript.checkIsInside(gameObject.transform.position))
        {
            if (!isPlaying)
            {
                musicScript.playFileRandomStart(audioSource, true);
                isPlaying = true;
            }
            musicScript.controlMusic(gameObject.transform.position, audioSource, minPitch, maxPitch);
        }
        // once it leaves the body, the sound stops
        else
        {
            musicScript.playFileRandomStart(audioSource, false);
            isPlaying = false;
        }
    }

    // waits until an event is invoked from SoundControlByVoxel
    private void canControlSoundEvent()
    {
        canControlSound = true;
    }

}
