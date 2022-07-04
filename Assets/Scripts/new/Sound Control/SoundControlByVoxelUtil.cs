using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System;

/// <summary>
/// This is a utility script, that makes the voxel colours available to any script. Meaning you can use it to help you figure out which voxel you are closest to
/// inside the dataset and get the information about that voxel. It was designed to be used to modify the pitch of a sound
/// </summary>
public class SoundControlByVoxelUtil : MonoBehaviour
{
    private int[] voxelDims;

    private GenericLoader loader;
    private MeshFilter meshFil;
    private string rawFileName;

    private int[][][] arr_3d;

    public int MinGrayValue = 0;
    [HideInInspector]
    public bool isVolumeUp = false;

    // uncomment all "color_arr_3d" if you want access to the color transform data, takes a lot of memory.
    //private Color[][][] color_arr_3d;

    [HideInInspector]
    public UnityEvent canControlSoundEvent;

    private bool ScaleIsInverted = false;

    // will finish before the scene starts
    private void Awake()
    {
        loader = gameObject.GetComponent<GenericLoader>();
        loader.ScaleNegativeEvent.AddListener(InvertScaleEvent);

        rawFileName = Path.Combine(loader.imageDir, loader.RawFileName);

        loader.DoneFillingTextureEvent.AddListener(instantiate3DArrays);
        Debug.Log("here");
    }

    // Start is called before the first frame update
    void Start()
    {
        meshFil = gameObject.GetComponent<MeshFilter>();
    }

    public bool getVolumeIsUp()
    {
        return isVolumeUp;
    }

    // For when the scale is inverted
    private void InvertScaleEvent()
    {
        ScaleIsInverted = true;
        loader.ScaleNegativeEvent.RemoveListener(InvertScaleEvent);
    }

    /// <summary>
    /// For any world position, it checks whether it is inside this object
    /// </summary>
    /// <param name="worldPosition">The query world space </param>
    /// <returns>true if the position is inside the bounds, otherwise false</returns>
    public bool checkIsInside(Vector3 worldPosition)
    {
        return meshFil.mesh.bounds.Contains(gameObject.transform.InverseTransformPoint(worldPosition));
    }

    public void playFileRandomStart(AudioSource LocalAudioSource, bool _on)
    {
        if (_on)
        {
            LocalAudioSource.time = UnityEngine.Random.Range(0.0f, LocalAudioSource.clip.length);
            LocalAudioSource.volume = .5f;
            LocalAudioSource.Play();
        }
        else LocalAudioSource.Stop();
    }

    /// <summary>
    /// This file controls the volume of the audiosource between max and min
    /// </summary>
    /// <param name="worldSpacePosition">query position, checks where the closest voxel is</param>
    /// <param name="LocalAudioSource">audio source to modify</param>
    /// <param name="min">minimum volume</param>
    /// <param name="max">maximum volume</param>
    public void controlMusic(Vector3 worldSpacePosition, AudioSource LocalAudioSource, float min = 1.0f, float max = 3.0f)
    {
        //move position of audiosource to position of wand head
        //LocalAudioSource.transform.position = worldSpacePosition;

        Vector3 CurrLocation = gameObject.transform.InverseTransformPoint(worldSpacePosition);

        // get the voxel positions in the 3d array
        Vector3Int voxelPos = getIndexFromLocal(CurrLocation);

        // get grayscale value from 0 to 255 normalized between 0 and 1
        float grayValue = arr_3d[voxelPos.x][voxelPos.y][voxelPos.z] / 255f;

        // I have not done anything with color yet, but this gives you the rgb value for the given pixel
        //Color colorValue = color_arr_3d[voxelPos.x][voxelPos.y][voxelPos.z];

        //// this gives you access to hsv, 
        //// h is hue
        //// s is saturation
        //// v is value
        //Color.RGBToHSV(colorValue, out float h, out float s, out float v);
        //// use like you would any other variable
        //// like this works:
        //float hsv = h * s * v; //commented bc not using
        //// EASY UNCOMMENT: highlight and hit CTRL + K + U

        // I found that in unity, you can't really hear below a pitch value of 1 and they don't let you go past 3. But feel free to play around with min and max
        // as unity allows from -3 to 3

        // make the hue fluctuate between 1.0 and 3.0f
        float pitchValue = min + grayValue * 2f;

        // this is just to keep the values from going too high or too low
        LocalAudioSource.pitch = Mathf.Clamp(pitchValue, min, max);

        // Just logs the grascale value in the console
        //Debug.Log("test x, y, z: " + voxelPos.x + ", " + voxelPos.y + ", " + voxelPos.z + " gives value: " + grayValue);
    }

    /// <summary>
    /// this function gets the indices of the 3d Arrays using worldspace position
    /// </summary>
    /// <param name="localPos">position inside the body</param>
    /// <returns>the closest voxel to that position</returns>
    public Vector3Int getIndexFromLocal(Vector3 localPos)
    {
        // already normalized between 0 and 1 because bounds are -0.5 to 0.5 always
        Vector3 OffsetPos = localPos - meshFil.mesh.bounds.min;

        // OffsetPos.x needs to go from right to left, not left to right. So, subtract 1

        if (!ScaleIsInverted)
        {
            OffsetPos.x = 1.0f - OffsetPos.x;
        }

        //Debug.Log("Offset Position: " + OffsetPos);

        // x describes left to right, y describes anterior to posterior (chest to bum), z describes inferior to superior (toes to head)
        

        Vector3 voxelFloatPositions = new Vector3(OffsetPos[0] * voxelDims[0], OffsetPos[1] * voxelDims[1], OffsetPos[2] * voxelDims[2]);
        Vector3Int voxelIndices = Vector3Int.FloorToInt(voxelFloatPositions);
        return voxelIndices;
    }

    /// <summary>
    /// create the 3d grayscale array, can be used to create a 3d array of colors if you uncomment //color_arr_3d
    /// </summary>
    private void instantiate3DArrays()
    {
        
        voxelDims = loader.getDim();
        
        // initiate the 3d arrays
        arr_3d = new int[loader.dim[0]][][];
        //color_arr_3d = new Color[loader.dim[0]][][];
        for (int i = 0; i < loader.dim[0]; i++)
        {
            arr_3d[i] = new int[loader.dim[1]][];
            //color_arr_3d[i] = new Color[loader.dim[1]][];
            for (int j = 0; j < loader.dim[1]; j++)
            {
                arr_3d[i][j] = new int[loader.dim[2]];
                //color_arr_3d[i][j] = new Color[loader.dim[2]];
            }
        }

        // Read the file information
        FileStream file = new FileStream(rawFileName, FileMode.Open);

        BinaryReader reader = new BinaryReader(file);
        byte[] buffer = new byte[loader.dim[0] * loader.dim[1] * loader.dim[2]];
        reader.Read(buffer, 0, sizeof(byte) * buffer.Length);
        reader.Close();
        Color[] colors = new Color[buffer.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            colors[i] = loader.findColor(buffer[i]);
        }

        // Turns the 1d array to 3d
        for (int x = 0; x < loader.dim[0]; x++)
        {
            for (int y = 0; y < loader.dim[1]; y++)
            {
                for (int z = 0; z < loader.dim[2]; z++)
                {
                    arr_3d[x][y][z] = buffer[(z * loader.dim[0] * loader.dim[1]) + (y * loader.dim[0]) + x];
                    //color_arr_3d[x][y][z] = colors[(z * loader.dim[0] * loader.dim[1]) + (y * loader.dim[0]) + x];
                }
            }
        }
        loader.DoneFillingTextureEvent.RemoveListener(instantiate3DArrays);

        canControlSoundEvent.Invoke();
    }
}
