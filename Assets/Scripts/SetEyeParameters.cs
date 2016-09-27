using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.Networking;
//using UnityEngine.Events;
//using UnityEngine.EventSystems;
//using UnityEngine.VR;
using System;
//using System.Collections;
using System.IO;
//using SMI;

public class SetEyeParameters : NetworkBehaviour
{
	private GameObject menuCanvas;
	private MenuCanvasControl menuControl; //acces to MenuCanvasControl 
	private RecordEyesPosition recordEye; //accces to just measured eye positions

	private Camera rayCastCam; // camera to which this script is attached
	private bool isLeftEye;
	
	//for reporting 
	public float eyeContrast { get { return 100f * _eyeContrast; } }
	public float peripheralContrast { get { return 100f *_peripheralContrast; } }
	public float centralContrast { get { return 100f *_centralContrast; } }
	//Mathlab cannot plot boolean -> therefore conversion from boolean to float
	public float peripheralIsMirror { get { return _peripheralIsMirror;}}
	public float centralIsMirror { get { return _centralIsMirror;}}
	public float fieldMapMidPointX { get { return _fieldMapMidPointX; } } //horizontal angular position of decussation or border between hemifiuelds relativ to gaze straight ahead
	//public float maskMidPointX { get { return _maskMidPointX; } } //horizontal mask position: to be used for gaze feedback for gaze orientation
	public float fieldMapScale { get { return 100f *_fieldMapScale; } } // horizontal scale factor of field maps in %
	public int fieldMapIndex { get { return _fieldMapIndex; } } //index of field projection map file
                                                                //visualisation our Albinism model 
    public int fieldMapMode { get { return _fieldMapMode; } }
 
    public bool isGray { get { return _isGray; } } //if true: gray patches are painted over visual receptive fields in the retina which do not project to V1



	private float _eyeContrast = 1f;
	private float _peripheralContrast = 1f;
	private float _centralContrast = 1f;
	private float _peripheralIsMirror = 0f;
	private float _centralIsMirror = 0f;
	private float _fieldMapMidPointX = 0f; //horizontal angular position of border between hemifield at decussation of optic nerve fibres relativ to straight ahead
	//private float _maskMidPointX = 0f;
	private float _fieldMapScale = 100f;
	private int _fieldMapIndex; //index of field projection map file
    private int _fieldMapMode = 0;
    private bool _isGray = false;

    //private Dropdown fieldMapDropDown;

	//private Toggle grayPatchToggle;
    //private FileInfo[] fileInfos;
    //private int mapCount = 0;
	private float displayWidth = SMI.SMIGazeController.SMIcWrapper.Constants.FOV_GazeMapping; // is set to 84°
	//Oculus rift DK2 screen width
	private const float DK2ScreenWidth = 1920f; //twice the range of a single screen
	private const float DK2ScreenHeight = 1080f;
 	//private Vector2 eyeMidPos;
    private Vector2 maskScale;
    private Vector2 dfltMaskTexOffset;

	private SetEyeRotation setEyeRot;

	
	
	//private Material mat; //access to shaders over the current renderer's material
	private UnityStandardAssets.ImageEffects.SplitSphereContrastTexture lEffect; //access to right camera's postshader script
	private UnityStandardAssets.ImageEffects.SecondSplitSphereContrastTexture rEffect; //access to right camera's postshader script

	//make connection to other modules in Awake before listener are activated in the Start routines
	void Awake()
	{
		try {
			//menuCanvas = GameObject.Find ("MenuCanvas"); // did not work here twice
            //menuControl = menuCanvas.GetComponent<MenuCanvasControl> ();
			//access to recording data
			//recordEye = menuCanvas.GetComponent<RecordEyesPosition>();
			//access to other module of our common game object
			//setEyeRot = gameObject.GetComponent<SetEyeRotation> ();
            
			//eyeMidPos = new Vector2 (DK2ScreenWidth * 0.5f, DK2ScreenHeight * 0.5f);
              rayCastCam = GetComponent<Camera> (); 
			if (rayCastCam.name.Equals ("Camera_LEFT"))
			{
				isLeftEye = true;
				lEffect = GetComponent<UnityStandardAssets.ImageEffects.SplitSphereContrastTexture> ();
			} else {
				isLeftEye = false;
				rEffect = GetComponent<UnityStandardAssets.ImageEffects.SecondSplitSphereContrastTexture> ();	
			}

		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Awake SetEyeParams Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log (" Awake SetEyeParams  Error:" + exp.Message);
			return;
		}

	}
    // Use this for initialization
    void Start()
    {
        try {
            maskScale = new Vector2(1.0f / DK2ScreenWidth, 1.0f / DK2ScreenHeight);
            dfltMaskTexOffset = new Vector2(0.25f, 0f);
            menuCanvas = GameObject.Find ("MenuCanvas");
			menuControl = menuCanvas.GetComponent<MenuCanvasControl> ();
			recordEye = menuCanvas.GetComponent<RecordEyesPosition>();
            //access to other module of our common game object
            setEyeRot = gameObject.GetComponent<SetEyeRotation> ();

            if (rayCastCam == null)
            {
                rayCastCam = GetComponent<Camera>();
                if (rayCastCam.name.Equals("Camera_LEFT"))
                {
                    isLeftEye = true;
                    lEffect = GetComponent<UnityStandardAssets.ImageEffects.SplitSphereContrastTexture>();
                }
                else {
                    isLeftEye = false;
                    rEffect = GetComponent<UnityStandardAssets.ImageEffects.SecondSplitSphereContrastTexture>();
                }
            }

            _fieldMapIndex = 3; //adapt to a convenient right side projection map
			OnChangeFieldMap(_fieldMapIndex); //Load initial mask texture

			if (isLeftEye){
				if (_isGray)
					lEffect.isGray = 1;
				else
					lEffect.isGray = 0;


				lEffect.mainTextureOffset = new Vector2(0f, 0f);
				lEffect.mainTextureScale = new Vector2(1f, 1f);


                //setting tiling of hemifield mapping mask texture
                //lEffect.maskTexOffset = new Vector2(0.25f, 0f); 
                lEffect.maskTexOffset = dfltMaskTexOffset;
				lEffect.maskTexScale = new Vector2 (0.5f, 1f);


				lEffect.maskMidPointX = _fieldMapMidPointX;
				lEffect.maskScaleX = _fieldMapScale * 0.01f;

             
			} else {
				if (_isGray)
					rEffect.isGray = 1;
				else
					rEffect.isGray = 0;

				rEffect.mainTextureOffset = new Vector2(0f, 0f);
				rEffect.mainTextureScale = new Vector2(1f, 1f);


                //setting tiling of hemifield mapping mask texture
                //rEffect.maskTexOffset = new Vector2(0.25f, 0f); 
                rEffect.maskTexOffset = dfltMaskTexOffset;
				rEffect.maskTexScale = new Vector2 (0.5f, 1f);

				rEffect.maskMidPointX = _fieldMapMidPointX;
				rEffect.maskScaleX = _fieldMapScale * 0.01f;
			}
			Debug.Log ("SetEyeParemters:RecordEyePosition:" + recordEye.gameObject.name);
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Start SetEyeParams Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log (" Start SetEyeParams Error:" + exp.Message);
			return;
		}

    }

    void Update()
    {
        Vector2 screenPos;
        Vector2 offSet;
        if (!isServer && _fieldMapMode.Equals(1)) 
        {
            if (recordEye.IsValidSample) //if is not true then recordEEye.hmdSample will be null
            {
                switch (setEyeRot.stabilisationSource)
                {
                    case 0:  //left eye: get averaged screen position [range -0.5 to 0.5]
                        screenPos.x = 0.5f - recordEye.hmdSample.left.por.x * maskScale.x;
                        screenPos.y = recordEye.hmdSample.left.por.y * maskScale.y - 0.5f;
                        break;
                    case 1: //right eye
                        screenPos.x =  0.5f - recordEye.hmdSample.right.por.x * maskScale.x;
                        screenPos.y = recordEye.hmdSample.right.por.y * maskScale.y - 0.5f;
                        break;
                    case 2:  //either eye
                        if (isLeftEye)
                        {
                            screenPos.x = 0.5f - recordEye.hmdSample.left.por.x * maskScale.x;
                            screenPos.y = recordEye.hmdSample.left.por.y * maskScale.y - 0.5f;
                        }
                        else
                        {
                            screenPos.x = 0.5f - recordEye.hmdSample.right.por.x * maskScale.x;
                            screenPos.y = recordEye.hmdSample.right.por.y * maskScale.y - 0.5f;
                        }
                        break;
                    default:
                        screenPos.x = 0.5f - recordEye.hmdSample.left.por.x * maskScale.x;
                        screenPos.y = recordEye.hmdSample.left.por.y * maskScale.y - 0.5f;
                        break;

                }
                if (isLeftEye)
                {
                    offSet.x = screenPos.x * lEffect.mainTextureScale.x;
                    offSet.y = screenPos.y * lEffect.mainTextureScale.y;
                    lEffect.maskTexOffset = dfltMaskTexOffset + offSet;
                }
                else
                {
                    offSet.x = screenPos.x * rEffect.mainTextureScale.x;
                    offSet.y = screenPos.y * rEffect.mainTextureScale.y;
                    rEffect.maskTexOffset = dfltMaskTexOffset + offSet;
                }
            }
        }
    }

    [ServerCallback]
	public void OnChangeFieldMapMidPointX(string strMidPointX)
    {
		Debug.Log ("Field Map Midpoint X:" + strMidPointX);
		try
        {
			float borderPos = float.Parse(strMidPointX);
			RpcOnChangeFieldMapMidPointX(borderPos);
			if (menuControl != null) {
				if (isLeftEye)
					menuControl.AddMenuEntry("LeftEyeDecussationPos", strMidPointX);
				else 
					menuControl.AddMenuEntry("RightEyeDecussationPos", strMidPointX);
			}

        }
        catch (System.FormatException fe)
        {
            Debug.Log(fe);
            return;
        }
        catch (System.Exception exp)
        {
            Debug.LogException (exp);
            return;
        }
    }

    [ClientRpc]
	public void RpcOnChangeFieldMapMidPointX(float borderPos)
    {
		//borderpos is in degrees, display width is about 84°, the mask
        //maskOffsets.x = 0.25f + borderPos * maskScale.x  * 0.5f / displayWidth; //using Unity3d predefined tiling structure
        //mat.SetTextureOffset("_MaskTex", maskOffsets); //to shift border mask
		if (isLeftEye){
			lEffect.maskMidPointX = borderPos * 0.5f / displayWidth;
			Debug.Log ("Rpc Field Map Midpoint X:" + lEffect.maskMidPointX.ToString ());
		}
		else
		{
			rEffect.maskMidPointX = borderPos * 0.5f / displayWidth;
			Debug.Log ("Rpc Field Map Midpoint X:" + rEffect.maskMidPointX.ToString ());
		}
		_fieldMapMidPointX = borderPos;
    }

    [ServerCallback]
    public void OnChangeFieldMapMode(int mode)
    {
        Debug.Log("OnChangeMapMode: " + mode.ToString());
        try
        {
            RpcOnChangeFieldMapMode(mode);
            if( menuControl != null)
                if (isLeftEye)
                    menuControl.AddMenuEntry("ProjectionMapMode", mode.ToString());                     
        }
        catch (System.FormatException fe)
        {
            Debug.Log(fe);
            return;
        }
        catch (System.Exception exp)
        {
            Debug.LogException(exp);
            return;
        }
    }
    [ClientRpc]
    public void RpcOnChangeFieldMapMode(int mode)
    {
        _fieldMapMode = mode;
    }


    [ServerCallback]
	public void OnChangeFieldMapScale(string strFieldMapScale)
	{
		Debug.Log ("Field Map Scale :" + strFieldMapScale);
		try
		{
			_fieldMapScale = float.Parse(strFieldMapScale);
			float fieldMapScale = _fieldMapScale * 0.01f;
			RpcOnChangeFieldMapScale(fieldMapScale);
			
			if (menuControl != null) {
				if (isLeftEye)	
					menuControl.AddMenuEntry ("LeftEyeCenterScale", strFieldMapScale);
				else
					menuControl.AddMenuEntry ("RightEyeCenterScale", strFieldMapScale);
			}
		}
		catch (System.FormatException fe)
		{
			Debug.Log(fe);
			return;
		}
		catch (System.Exception exp)
		{
			Debug.LogException (exp);
			return;
		}
	}
	[ClientRpc]
	public void RpcOnChangeFieldMapScale(float fieldMapScale)
	{
		if (isLeftEye) {
			if (fieldMapScale > 0.01) 
				lEffect.maskScaleX = 1f / fieldMapScale;
			else
				lEffect.maskScaleX = 1f;
		} else {
			if (fieldMapScale > 0.01) 
				rEffect.maskScaleX = 1f / fieldMapScale;
			else
				rEffect.maskScaleX = 1f;
		}
	}


    [ServerCallback]
	public void OnTogglePeripheralMirror(bool bOn)
    {
        //if (rightsideToggle.isOn)
		if (bOn)
			RpcOnTogglePeripheralMirror(1f);
        else
            RpcOnTogglePeripheralMirror(0f);
		Debug.Log ("Toggle Periphery: " + bOn.ToString());
		if (menuControl != null) {
			if (isLeftEye)
				menuControl.AddMenuEntry ("LeftEyePeripheralSceneToggle", bOn.ToString ());
			else
				menuControl.AddMenuEntry ("RightEyePeripheralSceneToggle", bOn.ToString ());
		}
	}

    [ClientRpc]
    public void RpcOnTogglePeripheralMirror(float val)
    {
		_peripheralIsMirror = val;
		if (isLeftEye)
			lEffect.isMirroredPeriphery = val;
		else
			rEffect.isMirroredPeriphery = val;
    }

    [ServerCallback]
    public void OnToggleCentralMirror(bool bOn)
    {
		if (bOn)
            RpcOnToggleCentralMirror(1f);
        else
            RpcOnToggleCentralMirror(0f);
		
		Debug.Log ("Toggle Center:" + bOn.ToString ());
		if (menuControl != null) {
			if (isLeftEye)
				menuControl.AddMenuEntry ("LeftEyeCenterSceneToggle", bOn.ToString ());
			else
				menuControl.AddMenuEntry ("RightEyeCenterSceneToggle", bOn.ToString ());
		}
	}

    [ClientRpc]
    public void RpcOnToggleCentralMirror(float val)
    {
		_centralIsMirror = val;
		if (isLeftEye)
			lEffect.isMirroredCenter = val;
		else
			rEffect.isMirroredCenter = val;
	}

    [ServerCallback]
	public void OnChangePeripheralContrast(string strContrast)
    {
		//string strContrast = rightsideContrastInputField.text;
        Debug.Log("LeftsidePeripheral Contrast " + strContrast);
        try
        {
            float contrast = float.Parse(strContrast) * 0.01f;
            RpcOnChangePeripheralContrast(contrast);
			if (menuControl != null) 
				if (isLeftEye)
					menuControl.AddMenuEntry ("PeripheralFieldContrast", strContrast);
        }
        catch (System.FormatException fe)
        {
            Debug.Log(fe);
            return;
        }
        catch (System.Exception exp)
        {
            Debug.LogException(exp);
            return;
        }
    }
    [ClientRpc]
    public void RpcOnChangePeripheralContrast(float contrast)
    {
        Debug.Log("RpcPeripheralContrast " + contrast.ToString());
		if (isLeftEye)
			lEffect.pheripheralContrast = contrast;
		else
			rEffect.pheripheralContrast = contrast;
		_peripheralContrast = contrast;
    }

    [ServerCallback]
	public void OnChangeCentralContrast(string strContrast)
    {
		if (menuControl != null) {
			if (isLeftEye)
				menuControl.AddMenuEntry ("CenterFieldContrast", strContrast);
		}
        Debug.Log("Central Contrast " + strContrast);
        try
        {
			float contrast = float.Parse(strContrast) * 0.01f;
            RpcOnChangeCentralContrast(contrast);
        }
        catch (System.FormatException fe)
        {
            Debug.Log(fe);
            return;
        }
        catch (System.Exception exp)
        {
            Debug.LogException(exp);
            return;
        }
    }
    [ClientRpc]
    public void RpcOnChangeCentralContrast(float contrast)
    {
        Debug.Log("RpcCentralContrast " + contrast.ToString());
		_centralContrast = contrast;
		if (isLeftEye)
				lEffect.centralContrast = contrast;
			else
				rEffect.centralContrast = contrast;
	}
    [ServerCallback]
	public void OnChangeContrast(string strContrast)
    {
		Debug.Log("Eye Contrast " + strContrast);
        try
        {
            float contrast = float.Parse(strContrast) * 0.01f;
            RpcOnChangeContrast(contrast);
			if (menuControl != null) {
				if (isLeftEye)
					menuControl.AddMenuEntry ("LeftEyeDisplayContrast", strContrast);
				else
					menuControl.AddMenuEntry ("RightEyeDisplayContrast", strContrast);
			}
		}
        catch (System.FormatException fe)
        {
            Debug.Log(fe);
            return;
        }
        catch (System.Exception exp)
        {
            Debug.LogException(exp);
            return;
        }
    }
    [ClientRpc]
    public void RpcOnChangeContrast(float contrast)
    {
		_eyeContrast = contrast;
		if (isLeftEye)
			lEffect.contrast = contrast;
		else
			rEffect.contrast = contrast;
	}

    [ServerCallback] //this will only be performed on server
    public void OnChangeFieldMap(int index)
    {
		if (menuControl != null) {
			if (isLeftEye)
				menuControl.AddMenuEntry ("LeftEyeMaskTexture", index.ToString ());
			else
				menuControl.AddMenuEntry ("RightEyeMaskTexture", index.ToString ());
		}
		//int level = fieldMapDropDown.GetComponent<Dropdown>().value;
        RpcOnChangeFieldMap(index); //host changed skybox also on its own client
    }

    [ClientRpc] //to be used, if the server gives commands for the clients. Use [CMD] and cmdDoAction() if clients give commands like fireing bullets 
    public void RpcOnChangeFieldMap(int index)
    {
		FileInfo info = menuControl.mapFileInfos[index];
		string path = info.FullName;
        Debug.Log("Change field map on Client " + index.ToString());
		_fieldMapIndex = index;
		ChangeMaps(path);
	}

	[ServerCallback]
	public void OnChangeGrayPatch(bool isOn)
	{
		int val;
		if (isOn)
			val = 1;
		else
			val = 0;
		RpcOnChangeGrayPatch (val);
		/******* no menoControl available
		if (isLeftEye)
			menuControl.AddMenuEntry ("LeftGrayPatchToggle", isOn.ToString ());
		else
			menuControl.AddMenuEntry ("RightGrayPatchToggle", isOn.ToString ());
		Debug.Log ("Change Gray Patches to: " + isOn.ToString ());
		**************************/
	}
	[ClientRpc]
	public void RpcOnChangeGrayPatch(int val)
	{
		if (isLeftEye)
			lEffect.isGray = val;
		else
			rEffect.isGray = val;
	}
	[ServerCallback]
	public void OnNewScene()
	{
		Debug.Log ("OnNewScene");
		ResetEffets ();
		RpcOnNewScene();
	}
	[ClientRpc]
	public void RpcOnNewScene()
	{
		Debug.Log ("Client OnNewScene");
		ResetEffets ();
	}
	private void ResetEffets()
	{
		if (isLeftEye) {
			lEffect.maskTexOffset = new Vector2(0.25f, 0f); 
			lEffect.maskTexScale = new Vector2 (0.5f, 1f);
			lEffect.maskMidPointX = 0f;
			lEffect.maskScaleX = 1f;
			lEffect.centralContrast = 1f;
			lEffect.pheripheralContrast = 1f;
			lEffect.isGray = 0;
			lEffect.isMirroredCenter = 0;
			lEffect.isMirroredPeriphery = 0;
		} else {
			rEffect.maskTexOffset = new Vector2(0.25f, 0f); 
			rEffect.maskTexScale = new Vector2 (0.5f, 1f);
			rEffect.maskMidPointX = 0f;
			rEffect.maskScaleX = 1f;
			rEffect.centralContrast = 1f;
			rEffect.pheripheralContrast = 1f;
			rEffect.isGray = 0;
			rEffect.isMirroredCenter = 0;
			rEffect.isMirroredPeriphery = 0;
		}
	}

    void ChangeMaps(string path) //should be called when OnBorderMapSelected event occurs/is rised
    {
        Debug.Log("CangeMaps: " + path);
        Texture2D tex = null;
        byte[] fileData;
        if (File.Exists(path))
        {
            //get rid of old texture to free up unused memory by using an asynchronous funtion thread
            AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
            if (unloadOp.isDone == true)
                Debug.Log("Unloading unused ressources finished");
            //fill map texture file into a dynamic 2d-texture
            fileData = File.ReadAllBytes(path);
            Debug.Log("Read projection map texture file " + path);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            //set my shaders masking texture
			if (isLeftEye)
				lEffect.maskTexture = tex;
			else
				rEffect.maskTexture = tex;
			Debug.Log("got Texture " + path );
		}
    }
	
}
