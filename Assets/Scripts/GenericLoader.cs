using UnityEngine;
using UnityEngine.Events;
using System.IO; 
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

/*
 * Created by Kumaradevan Punithakumar, 
 * Modified by Marilene Oliver, Walter Ostrander
 * https://github.com/Know-Thyself-as-a-Virtual-Reality/KTVR-DICOM-Tools
 */

public struct MHDHeaderGeneric
{
	public int[] dims;
	public float[] spacing;
	public string datafile;
	public string datatype;
};


public class GenericLoader : MonoBehaviour {

    private string oldxml;
    private string currxml;

    [DropDownList(typeof(CTDrawerHelper), "getFileNames")]
    public string dropdownxml;

    public string xmlFolderPath;
    public string imageDir;
    public string RawFileName;

    public int[] dim = new int[3] { 0, 0, 0 };
    public bool mipmap;

    public MHDHeaderGeneric mhdheader;
    private float[,] cTable;

    public int NumberOfFrames = 1;

    List<Texture3D> _volumeBuffer = new List<Texture3D>();
    private int frameNumber = 0;

    public bool InvertScale;
    private float ScaleMultiplier = 1f;

    [SerializeField]
    [Range(0.01f, 5f)]
    public float volScale = 1f;

    [SerializeField]
    [Range(0.02f, 5f)]
    public float brightness = 1f;

    [SerializeField]
    [Range(0.01f, 2f)]
    public float timeInterval = 0.05f;
    private float timeMeas = 0f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim1Min = 0f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim1Max = 1f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim2Min = 0f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim2Max = 1f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim3Min = 0f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim3Max = 1f;

    bool clipForward = false;
    bool isPlay = true;

    [HideInInspector]
    public UnityEvent DoneFillingTextureEvent;

    [HideInInspector]
    public UnityEvent ScaleNegativeEvent;

    private void Awake()
    {
        // set dim to regex of whats in the mhd file. if exception, will set to {256,256,256}
        currxml = dropdownxml;
        oldxml = dropdownxml;
        reloadTexture();
        
    }

    private void Start()
    {
        DoneFillingTextureEvent.Invoke();
        if (InvertScale)
        {
            ScaleMultiplier = -1f;
            ScaleNegativeEvent.Invoke();
        }
    }

    //void Start() {
    //    if (InvertScale)
    //    {
    //        ScaleMultiplier = -1f;
    //        ScaleNeganiteEvent.Invoke();
    //    }


    //    // set dim to regex of whats in the mhd file. if exception, will set to {256,256,256}
    //    currxml = dropdownxml;
    //    oldxml = dropdownxml;
    //    reloadTexture();
    //    DoneFillingTextureEvent.Invoke();
    //}

    private void Update() {

        if (!oldxml.Equals(dropdownxml))
        {
            oldxml = dropdownxml;
            currxml = dropdownxml;
            reloadTexture();
        }

        if (isPlay)
        {
            timeMeas += Time.deltaTime;

            if (timeMeas > timeInterval)
            {
                frameNumber++;
                timeMeas = 0f;
                if (frameNumber >= NumberOfFrames)
                {
                    frameNumber = 0;
                }
            }

            GetComponent<Renderer>().material.SetTexture("_Data", _volumeBuffer[frameNumber % NumberOfFrames]);
        }
        
        //this.transform.localScale = new Vector3 (mhdheader.spacing[0]*volScale, mhdheader.spacing[2]*volScale, mhdheader.spacing[1]*volScale);
        GetComponent<Renderer>().material.SetFloat("_Brightness", brightness);
        GetComponent<Renderer>().material.SetVector("_ClipDimMin", new Vector4(_ClipDim1Min, _ClipDim2Min, _ClipDim3Min, 1f));
        GetComponent<Renderer>().material.SetVector("_ClipDimMax", new Vector4(_ClipDim1Max, _ClipDim2Max, _ClipDim3Max, 1f));
        
        transform.localScale = new Vector3(mhdheader.spacing[0] * volScale * ScaleMultiplier, mhdheader.spacing[1] * volScale * dim[1] / dim[0], mhdheader.spacing[2] * volScale * dim[2] / dim[0]);


        //GameObject clipPlane = GameObject.FindGameObjectWithTag("ClipPlane");
        //GameObject cubeParent = GameObject.FindGameObjectWithTag("CubeParent");

        //        clipPlane.transform.localScale = new Vector3(0.15f * this.transform.localScale[0], 0.15f * this.transform.localScale[1], 0.15f * this.transform.localScale[2]);
        //clipPlane.transform.localScale = new Vector3(1.0f * this.transform.localScale[0], 1.0f * this.transform.localScale[1], 1.0f * this.transform.localScale[2]);

        //var p1 = new Plane(cubeParent.transform.InverseTransformDirection(clipPlane.transform.up), cubeParent.transform.InverseTransformPoint(clipPlane.transform.position));
        //Plane p = new Plane(this.transform.InverseTransformDirection(cubeParent.transform.TransformDirection(clipPlane.transform.up)), this.transform.InverseTransformPoint(cubeParent.transform.TransformPoint(clipPlane.transform.position)));

        //  Plane p = new Plane(this.transform.InverseTransformDirection(clipPlane.transform.forward), this.transform.InverseTransformPoint(clipPlane.transform.position));

        //GetComponent<Renderer>().material.SetVector("_ClipPlane", new Vector4(p.normal.x, this.transform.localScale[1] / this.transform.localScale[0] * p.normal.y, this.transform.localScale[2] / this.transform.localScale[0] * p.normal.z, p.distance/Mathf.Sqrt(3f) ));    

        //GetComponent<Renderer>().material.SetVector("_ClipPlane", new Vector4(p.normal.x, p.normal.y, p.normal.z, 0.5f*p.distance));    


        //Plane p = new Plane(this.transform.InverseTransformDirection(clipPlane.transform.up), this.transform.InverseTransformPoint(clipPlane.transform.position));
        //GetComponent<Renderer>().material.SetVector("_ClipPlane", new Vector4(-p.normal.x, -p.normal.y, -p.normal.z, -p.distance));
    }
    

	private void OnDestroy ()
	{
		foreach(Texture3D _volbufferchild in _volumeBuffer){
			if (_volbufferchild != null) {
                if (Application.isEditor)
                {
                    DestroyImmediate(_volbufferchild);
                }
                else
                {
                    Destroy(_volbufferchild);
                }
			}
		}
	}

    public string getxmlFolderPath()
    {
        return xmlFolderPath;
    }

    public void setCurrxml(string xml)
    {
        currxml = xml;
        reloadTexture();
    }

    public string getCurrxml()
    {
        return currxml;
    }

    private void reloadTexture()
    {
        cTable = ParseSettingsXML(xmlFolderPath + currxml);
        Read_MHD_Header(Path.Combine(imageDir, RawFileName.Replace(".raw", ".mhd")));

        for (int k = 0; k < NumberOfFrames; k++)
        {
            Color[] colors = LoadData(Path.Combine(imageDir, RawFileName));
            _volumeBuffer.Add(new Texture3D(dim[0], dim[1], dim[2], TextureFormat.RGBAHalf, mipmap));

            _volumeBuffer[k].SetPixels(colors);
            _volumeBuffer[k].Apply();
        }

        GetComponent<Renderer>().material.SetTexture("_Data", _volumeBuffer[0]);
    }

	private Color[] LoadData(string fname)
	{
		Color[] colors;
		FileStream file = new FileStream(fname, FileMode.Open);

		BinaryReader reader = new BinaryReader(file);
		byte[] buffer = new byte[dim[0] * dim[1] * dim[2]]; 
		reader.Read(buffer, 0, sizeof(byte) * buffer.Length);
		reader.Close();

		colors = new Color[buffer.Length];
		for (int i = 0; i < buffer.Length; i++)
		{
			colors [i] = findColor(buffer[i]);
		}

		return colors;
	}

	public Color findColor (int pixel_value)
	{
		if (pixel_value <= cTable [0, 0])
			return new Color (cTable [0, 1], cTable [0, 2], cTable [0, 3], cTable [0, 4]);
		if (pixel_value >= cTable [cTable.GetLength (0) - 1, 0])
			return new Color (cTable [cTable.GetLength (0) - 1, 1], cTable [cTable.GetLength (0) - 1, 2], cTable [cTable.GetLength (0) - 1, 3], cTable [0, 4]);

		for (int i = 0; i < cTable.GetLength (0) - 1; i++) {
			if ((pixel_value >= cTable [i, 0]) & (pixel_value <= cTable [i + 1, 0])) {
				float npixel_val = (pixel_value - cTable [i, 0]) / (cTable [i + 1, 0] - cTable [i, 0]);
				return Color.Lerp (new Color (cTable [i, 1], cTable [i, 2], cTable [i, 3], cTable [i, 4]), new Color (cTable [i + 1, 1], cTable [i + 1, 2], cTable [i + 1, 3], cTable [i + 1, 4]), npixel_val);
			}
		}

		return Color.Lerp (new Color (1, 0, 0, 0), new Color (0, 1, 0, 1), pixel_value);
	}

		
	private bool Read_MHD_Header (string filename)
	{

		try {
			string line;
			StreamReader theReader = new StreamReader (filename, Encoding.Default);
			using (theReader) {
				do {
					line = theReader.ReadLine ();

					if (line != null) {
						string[] entries = line.Split (' ');
						if (entries.Length > 0) {
							if (entries [0] == "DimSize") {
								mhdheader.dims = new int[3];
								for (int i = 2; i < entries.Length; i++)
									mhdheader.dims [i - 2] = Int32.Parse (entries [i]);
							} else if (entries [0] == "ElementSpacing") {
								mhdheader.spacing = new float[3];
                                for (int i = 2; i < entries.Length; i++)
                                {
                                    mhdheader.spacing[i - 2] = float.Parse(entries[i]);
                                }
							} else if (entries [0] == "ElementType") {
								mhdheader.datatype = entries [2];
							} else if (entries [0] == "ElementDataFile") {
								mhdheader.datafile = Path.Combine (Path.GetDirectoryName (filename), entries [2]);
							}

						}
					}
				} while (line != null);
                // Done reading, close the reader and return true to broadcast success    
                dim = mhdheader.dims;
				theReader.Close ();
				return true;
			}
		} catch (Exception e) {
			Debug.Log (e.Message);
			return false;
		}
	}


	float[,] ParseSettingsXML (string xmlname)
	{
		try {
			StreamReader theReader = new StreamReader (xmlname, Encoding.Default);
			XmlDocument xmlSettings = new XmlDocument ();
			xmlSettings.Load (theReader);

			XmlNodeList rgbsettings = xmlSettings.GetElementsByTagName ("rgbfuncsettings");
			var npoints_rgb = Int32.Parse (rgbsettings [0].Attributes ["NumberOfPoints"].Value);

			float[,] rgb = new float[npoints_rgb, 4]; 

			for (int i = 0; i < npoints_rgb; i++) {
				string[] ptvalues = rgbsettings [0].Attributes ["pt" + i].Value.Split (',');
				if (ptvalues.Length > 3) {
					for (int j = 0; j < 4; j++) {
						rgb [i, j] = float.Parse (ptvalues [j]);
					}
				}
			}

			XmlNodeList opacitysettings = xmlSettings.GetElementsByTagName ("scalarfuncsettings");
			var npoints_alpha = Int32.Parse (opacitysettings [0].Attributes ["NumberOfPoints"].Value);

			float[,] alpha = new float[npoints_alpha, 2]; 

			for (int i = 0; i < npoints_alpha; i++) {
				string[] ptvalues = opacitysettings [0].Attributes ["pt" + i].Value.Split (',');
				if (ptvalues.Length > 2) {
					for (int j = 0; j < 2; j++) {
						alpha [i, j] = float.Parse (ptvalues [j]);
					}
				}
			}

			float[] intensityvals = merge_arrays (rgb, alpha);
			float[,] pixelTable = new float[intensityvals.Length, 5];


			for (int i = 0; i < intensityvals.Length; i++) {
				pixelTable [i, 0] = intensityvals [i];

				int k = getClosestK (rgb, pixelTable [i, 0]);
				float npixel_val = (pixelTable [i, 0] - rgb [k, 0]) / (rgb [k + 1, 0] - rgb [k, 0]);
				Color rgbtemp = Color.Lerp (new Color (rgb [k, 1], rgb [k, 2], rgb [k, 3], 1f), new Color (rgb [k + 1, 1], rgb [k + 1, 2], rgb [k + 1, 3], 1f), npixel_val);
				for (int j = 0; j < 3; j++)
					pixelTable [i, j + 1] = rgbtemp [j];

				k = getClosestK (alpha, pixelTable [i, 0]);
				npixel_val = (pixelTable [i, 0] - alpha [k, 0]) / (alpha [k + 1, 0] - alpha [k, 0]);
				pixelTable [i, 4] = Mathf.Lerp (alpha [k, 1], alpha [k + 1, 1], npixel_val);


			}



			return pixelTable;

		} catch (Exception e) {
			Debug.Log (e.Message);
			return null;
		}

	}

	// Concatenate unique intensity values from color and opacity tables
	float[] merge_arrays (float[,] rgb, float[,] alpha)
	{
		float[] res = new float[rgb.GetLength (0) + alpha.GetLength (0)];
		int i = 0, j = 0, k = 0;

		while (i < rgb.GetLength (0) && j < alpha.GetLength (0)) {
			if (rgb [i, 0] < alpha [j, 0]) {
				res [k] = rgb [i, 0];
				i++;
			} else if ((rgb [i, 0] > alpha [j, 0])) {
				res [k] = alpha [j, 0];
				j++;
			} else {
				res [k] = rgb [i, 0];
				i++;
				j++;
			}
			k++;
		}

		while (i < rgb.GetLength (0)) {
			res [k] = rgb [i, 0];
			i++;
			k++;
		}

		while (j < alpha.GetLength (0)) {
			res [k] = alpha [j, 0];
			j++;
			k++;
		}

		float[] merged_array = new float[k];

		for (int ii = 0; ii < k; ii++)
			merged_array [ii] = res [ii];

		return merged_array;
	}

	int getClosestK (float[,] a, float x)
	{

		if (x <= a [0, 0])
			return 0;

		if (x >= a [a.GetLength (0) - 1, 0])
			return a.GetLength (0) - 2;

		int ind = 0;

		while (x > a [ind, 0] && (ind < a.GetLength (0) - 2))
			ind++;

		return ind;


	}

    public void changeScale(float scale)
    {
        volScale = scale;
    }

    public int[] getDim()
    {
        return dim;
    }

    public void increaseScale()
    {
        volScale = Mathf.Clamp(1.01f * volScale, 0.1f, 5.0f);
    }

    public void decreaseScale()
    {
        volScale = Mathf.Clamp(volScale/1.01f, 0.1f, 5.0f);
    }

    public void increaseBrightness()
    {
        brightness = Mathf.Clamp(1.01f * brightness, 0.002f, 5.0f);
    }

    public void decreaseBrightness()
    {
        brightness = Mathf.Clamp(brightness / 1.01f, 0.002f, 5.0f);
    }

    public float getDim1Min()
    {
        return _ClipDim1Min;
    }

    public void setDim1Min(float ClipDim1Min)
    {
        _ClipDim1Min = Mathf.Clamp(ClipDim1Min, 0f, 1f);
    }

    public float getDim1Max()
    {
        return _ClipDim1Max;
    }

    public void setDim1Max(float ClipDim1Max)
    {
        _ClipDim1Max = Mathf.Clamp(ClipDim1Max, 0f, 1f);
    }

    public float getDim2Min()
    {
        return _ClipDim2Min;
    }

    public void setDim2Min(float ClipDim2Min)
    {
        _ClipDim2Min = Mathf.Clamp(ClipDim2Min, 0f, 1f);
    }

    public float getDim2Max()
    {
        return _ClipDim2Max;
    }

    public void setDim2Max(float ClipDim2Max)
    {
        _ClipDim2Max = Mathf.Clamp(ClipDim2Max, 0f, 1f);
    }

    public float getDim3Min()
    {
        return _ClipDim3Min;
    }

    public void setDim3Min(float ClipDim3Min)
    {
        _ClipDim3Min = Mathf.Clamp(ClipDim3Min, 0f, 1f);
    }

    public float getDim3Max()
    {
        return _ClipDim3Max;
    }

    public void setDim3Max(float ClipDim3Max)
    {
        _ClipDim3Max = Mathf.Clamp(ClipDim3Max, 0f, 1f);
    }

    public void increaseClipDim1()
    {
        if (clipForward){
            _ClipDim1Min = Mathf.Clamp(_ClipDim1Min + 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim1Max = Mathf.Clamp(_ClipDim1Max - 0.01f, 0f, 1f);
        }
    }

    public void decreaseClipDim1()
    {
        if (clipForward)
        {
            _ClipDim1Min = Mathf.Clamp(_ClipDim1Min - 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim1Max = Mathf.Clamp(_ClipDim1Max + 0.01f, 0f, 1f);
        }
    }

    public void increaseClipDim2()
    {
        if (clipForward)
        {
            _ClipDim2Min = Mathf.Clamp(_ClipDim2Min + 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim2Max = Mathf.Clamp(_ClipDim2Max - 0.01f, 0f, 1f);
        }
    }

    public void decreaseClipDim2()
    {
        if (clipForward)
        {
            _ClipDim2Min = Mathf.Clamp(_ClipDim2Min - 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim2Max = Mathf.Clamp(_ClipDim2Max + 0.01f, 0f, 1f);
        }
    }

    public void increaseClipDim3()
    {
        if (clipForward)
        {
            _ClipDim3Min = Mathf.Clamp(_ClipDim3Min + 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim3Max = Mathf.Clamp(_ClipDim3Max - 0.01f, 0f, 1f);
        }
    }

    public void decreaseClipDim3()
    {
        if (clipForward)
        {
            _ClipDim3Min = Mathf.Clamp(_ClipDim3Min - 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim3Max = Mathf.Clamp(_ClipDim3Max + 0.01f, 0f, 1f);
        }
    }

    public void changeClipDir()
    {
        if (clipForward)
        {
            clipForward = false;
        }else{
            clipForward = true;
        }

    }

    public void playOnOff()
    {
        if (isPlay)
        {
            isPlay = false;
        }
        else
        {
            isPlay = true;
        }
    }
}
