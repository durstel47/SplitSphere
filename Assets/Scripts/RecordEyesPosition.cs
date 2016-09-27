using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.Networking;
//using System.Collections;
using System;
using System.IO;
using SMI;

//Origin of this program based on G. Bertolini's SMI recording example

//Note: Network scripts should by attached to "real" Unity objects like the Menuas here. Networkipts attached to an empty transfrom risk not to be called by 
//network calls even when accompangyied by a valid network ID

public class RecordEyesPosition :  NetworkBehaviour
{

	public enum SceneType:int { Sphere, Target, Both, None, Dots };
		
	//gaze feedback modile is integrated in the recording module since both need actual data from the SMI Eye Tracker
	//public bool isGazeFeedback { get { return _isGazeFeedback; } } //if true: mask hhorizontal position is coupled to horizontal component of eye movements

	public Quaternion hmdOrientation
	{
		get { return _hmdOrientation;}
	}

	public SMI.SMIGazeController.unity_SampleHMD hmdSample
	{
		get { return _hmdSample; }
	}
	public bool IsValidSample
	{
		get {
			return _isValidSample;
		}
	}
	//variable to transmit gaze position to server pc not used yet
	public Vector2 lGazeScreenPos;
	public Vector2 rGazeScreenPos;

	public Vector3 lEyeAngles {
		get { return new Vector3 (lEyeElev, lEyeOri,0f); } //left eye's pitch, yaw  and roll angles (roll is not measured and therefore set to zero)
	}
	public Vector3 rEyeAngles {
		get { return new Vector3 (rEyeElev, rEyeOri, 0f); } //right eye's pitch, yaw and roll angles (roll is not measured and therefore set to zero)
	}

	public Vector3 lEyeGazeDir {
		get { return _lEyeGazeDir;}
	}
	public Vector3 rEyeGazeDir {
		get{ return _rEyeGazeDir; }
	}

	public Vector3 lEyeGazePoint {
		get{ return _lEyeGazePt; }
	}

	public Vector3 rEyeGazePoint {
		get{ return _rEyeGazePt; }
	}

	public Vector2 lEyeScreen {
		get { return _lEyeScreen; }
	}

	public Vector2 rEyeScreen {
		get { return _rEyeScreen; }
	}



	//rethrieve info about head position
	private string newContent;
	private Vector3 hmdTilt;
    private Quaternion _hmdOrientation;
    //private Vector3 hmdPosition;
    private Vector2 _lEyeScreen; //screen coordinates for plotting actual gaze for left eye
    private Vector3 _lEyeGazeDir; //gaze vector (left eye)
	private Vector3 _lEyeGazePt;
    private float leftRad, rightRad; //length of gaze vector
	private float lEyeOri = 90f; //horizontal eye rotation in deg
	private float lEyeElev = 90f; //eye elevation in deg 
	private float rEyeOri = 90f;
	private float rEyeElev = 90f; 
    private Vector2 _rEyeScreen;
    private Vector3 _rEyeGazeDir;
	private Vector3 _rEyeGazePt;
    private string filename; //Current persistent data dath  C:/Users/max/AppData/LocalLow/UntereWeinegg/SMIRotOKNCube/

    //private Toggle recToggle;
	private bool _isValidSample = false;
    private SetSphereParameters sphereParams;
	//not yet recorded from...
    private SetEyeParameters lEyeParams;
    private SetEyeParameters rEyeParams;
   	private bool recording = false;
	private SetEyeRotation lEyeRot; //same date for left and right eye control.. this may change in the future
	//private SetEyeRotation rEyeRot;
	//private Vector3 lCamRot;
	//private Vector3 rCamRot;

	private GameObject sphereGo;
	private GameObject targetGo;
	private MenuCanvasControl menuControl;
	private TargetControl targetControl;

	private SMI.SMIGazeController.unity_SampleHMD _hmdSample = null;
	private SMI.SMIGazeController.unity_EyeDataHMDStruct lEyeSample;
	private SMI.SMIGazeController.unity_EyeDataHMDStruct rEyeSample;

	private float sceneStartTime;
	private float sceneTime;
	//private SceneType sceneType;

    void Start()
    {
		//_sceneIndex = 1;
		menuControl = GameObject.Find("MenuCanvas").GetComponent<MenuCanvasControl> ();
		lEyeParams = GameObject.Find ("Camera_LEFT").GetComponent<SetEyeParameters>();
		rEyeParams = GameObject.Find ("Camera_RIGHT").GetComponent<SetEyeParameters>();
		lEyeRot = GameObject.Find ("Camera_LEFT").GetComponent<SetEyeRotation> ();
		//rEyeRot = GameObject.Find ("Camera_RIGHT").GetComponent<SetEyeRotation> ();
        sphereGo = GameObject.Find("SkyboxOktahedronSphere");
        sphereParams = sphereGo.GetComponent<SetSphereParameters>();
        targetGo = GameObject.Find("MyTarget");
        targetControl = targetGo.GetComponent<TargetControl>();
		//lGazeScreenPos = Vector2.zero;
		//rGazeScreenPos = Vector2.zero;
        GetMenuAccess();
#if EYEMONITOR
        if (!isServer)
        {
            SMIGazeController.SMIcWrapper.smi_showEyeImageMonitor();
        }
#endif
    }

    //Unity builtin in feature for detecting level changes starting from 1st online sceneEntries on but not for the change form offline to online sceneEntries
    /*************************
    void OnLevelWasLoaded(int level)
	{
		_sceneIndex = level;
		GetMenuAccess ();
	}
    ********************/

    // Called on the server whenever a Network.InitializeServer was invoked and has completed.
    void GetMenuAccess()
    {
        //sceneType = menuControl
  		//if (sphereGo != null) {
			//sphereGo = GameObject.Find ("SkyboxOktahedronSphere");
			//sphereParams = sphereGo.GetComponent<SetSphereParameters> ();
		//} else if (targetGo != null) {
			//targetGo = GameObject.Find ("MyTarget");
		_isValidSample = false;

		if (!isServer)
        {
            Debug.Log("Recording Eye Position Script has started on a non-server client");
        }


	}
    public void StartRecording()
    {
		if (isServer) // no recording on ADMIN
			return;
		Debug.Log("Check for active SMI Gazecontroller instance");
        if (SMIGazeController.Instance.isActiveAndEnabled)
        {
			if (recording)
				return;
 			Debug.Log("Starting Eye Recording (active SMI Gazecontroller instance");
			//filename = Application.persistentDataPath + "/SplitSphere" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
			string path  = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + @"\" +Application.productName;
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			filename = path + @"\SplitSphere_" + 
                menuControl.taskIndex.ToString("D1") + "-" + menuControl.taskId.ToString("D1") + "_" +
                menuControl.sceneIndex.ToString("D1") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

            Debug.Log("Filename: " + filename);
            //fs.Close ();
#if SMISIM
			//variable names in first row
            newContent = "Time";
			newContent += ", hmdEuler.x, hmdEuler.y, hmd.Euler.z"; 
			//orientation of head set (we could add position)
#else
            newContent = "TaskId";
            newContent += ",Scene";
			newContent += ",Time";
			newContent += ",isValidSample";
			newContent += ",hmdEulerX,hmdEulerY,hmd.EulerZ"; //orientation of head set (we could add position)
			newContent += ",SampleTime";
			//newContent += ",lLensDistance, lScreenDist,lPupilRadius,lPupilPosX,lPupilPosY"; 
			newContent += ",lPupilRadius,lPupilPosX,lPupilPosY"; 
            newContent += ",lEyeOri,lEyeElev"; //rotation and elevation of lefteye in degrees
			//newContent += ",lEyePtx,lEyePty,lEyePtz"; //gaze baze point
			//newContent += ", lGazeDirX, lGazeDirY, lGazeDirZ";
            newContent += ",lEyeScreenX,lEyeScreenY"; //left eye rotation and elevevation in screen coordinates
			newContent += ",rPupilRadius, rPupilPosX,rPupilPosY"; 
			newContent += ",rEyeOri,rEyeElev";
			//newContent += ",rEyePtx,rEyePty,rEyePtz"; //gaze baze point
			//newContent += ", rGazeDirX, rGazeDirY, rGazeDirZ";
			newContent += ", rEyeScreen.x,rEyeScreen.y";
#endif
			newContent += ",lHemifieldContrast,rHemifieldContrast";
			newContent += ",lEyeContrast,rEyeContrast";
			newContent += ",lEyePeripheralMirror,lEyeCentralMirror";
			newContent += ",rEyePeripheralMirror,rEyeCentralMirror";
			newContent += ",lHemifieldBorder,rHemifieldBorder";
			newContent += ",lBorderMap,rBorderMap";
			newContent += ",eyeCamOption"; //MRD new 5.8.2016
            newContent += ",eyeStabSource"; //MRD new 5.8.2016 
            newContent += ",BorderMapMode"; //MRD new 5.8.2016 : projection map mode
			newContent += ",horGazeGain";
			//if(menuControl.sceneTypeControlDD.value.Equals(1) || menuControl.sceneTypeControlDD.Equals (2)) //does not work
			//{
			newContent += ",SphereSpeed";
			newContent += ",SphereMap";
			newContent += ",SphereContrast";
			//}
			//if (menuControl.sceneTypeControlDD.value > 0){
			newContent += ",StimHPos";
			newContent += ",IsStimOn";
			//}

			//newContent += ",lWorldEulerX,lWorldEulerY,lWorldEulerZ";
			//newContent += ",rWorldEulerX,rWorldEulerY,rWorldEulerZ";
			sceneStartTime = Time.time;

			File.AppendAllText(filename, newContent + Environment.NewLine);
			recording = true;

			//List available calibrations : there are none: list is null
            //string[] calibs = SMIGazeController.SMIcWrapper.smi_getAvailableCalibrations();
            //foreach (string calib in calibs)
            //    Debug.Log("Calibration type: " + calib);
        }
    }  

    public void StopRecording()
    {
        if (!isServer)
        {
            Debug.Log("Stopped eye recording");
            recording = false;
       }
    }
	
	// Update is called once per frame
	void Update () {
        if (!isServer) {
			_hmdOrientation = InputTracking.GetLocalRotation (VRNode.CenterEye);
			//hmdPosition = InputTracking.GetLocalPosition(VRNode.CenterEye);
			//check for SMI simulation mode define
			Vector3 euler = niceAngles(_hmdOrientation.eulerAngles);
#if SMISIM  
			if (recording)
			{
				newContent = Time.time.ToString() + ","
					+ euler.x.ToString() + ","
					+ euler.y.ToString() + ","
					+ euler.z.ToString() + ","
					+ sphereParams.drumSphereSpeed.ToString () + ","
					+ sphereParams.drumSphereContrast.ToString ()
					+ Environment.NewLine;
				File.AppendAllText (filename, newContent);
			}
#else
			//we could restrict taking measurments to causes when we record data and/or make any kind of gaze feedback.
			//to keep things simple we take data samples anyway (at least when there are available
			if (SMIGazeController.Instance.isActiveAndEnabled) {
				_hmdSample = SMIGazeController.Instance.smi_getSample();
				//collect only valid eye recording samples with their tiem stamp
				_isValidSample = _hmdSample.isValid;
				//if (_isValidSample)
				//{
					lEyeSample = _hmdSample.left;
					rEyeSample = _hmdSample.right;

					//gaze direction direction straight ahaead is recorded in our paradigm at 90° yaw, 90° pitch and 0° roll
					//in out Unity universum; for reporting it we will have to subtract 90° from the yaw and the pitch angle
					_lEyeGazeDir = lEyeSample.gazeDirection.normalized;
					lEyeOri = niceAngle(Mathf.Rad2Deg * Mathf.Acos(_lEyeGazeDir.x) -90f); 
					lEyeElev = niceAngle(Mathf.Rad2Deg * Mathf.Acos(_lEyeGazeDir.y) -90f);
					_lEyeGazePt = lEyeSample.gazeBasePoint;

					_rEyeGazeDir = rEyeSample.gazeDirection.normalized;
					rEyeOri = niceAngle(Mathf.Rad2Deg * Mathf.Acos(_rEyeGazeDir.x) -90f);
					rEyeElev = niceAngle(Mathf.Rad2Deg * Mathf.Acos(_rEyeGazeDir.y) -90f);
					_rEyeGazePt = lEyeSample.gazeBasePoint;

					//2d eye data sample
					_lEyeScreen = lEyeSample.por;
					_rEyeScreen = rEyeSample.por;
					sceneTime = Time.time - sceneStartTime;

					if (recording)
					{
                        float valid = 0f;
						if (_isValidSample)
							valid = 1f;
						newContent = menuControl.taskId.ToString() + ","
                            + menuControl.sceneIndex + ","
							+ sceneTime.ToString() + ","
							+ valid.ToString () + ","
							//Oculus tracking input
							+ euler.x.ToString() + ","
							+ euler.y.ToString() + ","
							+ euler.z.ToString() + ","
							+ _hmdSample.timeStamp.ToString() + ","
							/***check possibilities of SMI (eliminate later)
							+ leftEyeSample.eyeLensDistance.ToString() + ","
							+ rightEyeSample.eyeScreenDistance.ToString() + ","
							//***************/
							+ lEyeSample.pupilRadius.ToString() + ","
							+ lEyeSample.pupilPosition.x.ToString() + ","
							+ lEyeSample.pupilPosition.y.ToString() + ","
							//get left eye yaw and pitch in degrees and in screen pixels;
							+ lEyeOri.ToString () + "," + lEyeElev.ToString () + ","

							+ _lEyeScreen.x.ToString () + "," + _lEyeScreen.y.ToString () + ","

							/*** check possibilities of SMI (elliminate later on)
							+ leftEyeSample.eyeLensDistance.ToString() + ","
							+ rightEyeSample.eyeScreenDistance.ToString() + ","
							*************/
							+ rEyeSample.pupilRadius.ToString() + ","
							+ rEyeSample.pupilPosition.x.ToString() + ","
							+ rEyeSample.pupilPosition.y.ToString() + ","
							//get right eye yaw and pitch in degrees and in screen pixels;
							+ rEyeOri.ToString () + "," + rEyeElev.ToString () + ","

							+ _rEyeScreen.x.ToString () + "," + _rEyeScreen.y.ToString () + ","

							//Note: the left- and rightside hemifield should the same in the leftEyeParams and the rightEyeParams
							+ lEyeParams.peripheralContrast.ToString()  + "," + lEyeParams.centralContrast.ToString() + "," 
							+ lEyeParams.eyeContrast.ToString() + "," + rEyeParams.eyeContrast.ToString()  + ","

							+ lEyeParams.peripheralIsMirror.ToString() + "," + lEyeParams.centralIsMirror.ToString() + ","
							+ rEyeParams.peripheralIsMirror.ToString() + "," + rEyeParams.centralIsMirror.ToString() + ","

							+ lEyeParams.fieldMapMidPointX.ToString() + "," + rEyeParams.fieldMapMidPointX.ToString() + ","
							+ lEyeParams.fieldMapIndex.ToString()+ "," + rEyeParams.fieldMapIndex.ToString()  + ","
							//we are using allways the eyeCamControlOption and horizontal gains gains for either eye
							// this may change in the future....
							+ lEyeRot.eyeCamControlOption.ToString() + ","
                            + menuControl.stabilisationSource.value.ToString() + ","
                            + menuControl.projectionMapMode.value.ToString() + ","
							+ lEyeRot.horGazeGain.ToString() + ",";

						//if(	menuControl.sceneTypeControlDD.value.Equals(1) || menuControl.sceneTypeControlDD.Equals (2)) 
						//{
						newContent = newContent + sphereParams.drumSphereSpeed.ToString() + "," 
							+ menuControl.panoIndex.ToString() + ","   //bug in previous versions of this programm: resulted in keeping the startiong value of 12 (Paradeplatz). MRD 5.8.2016
							+ sphereParams.drumSphereContrast.ToString() + ",";
						//} 
						//if (menuControl.sceneTypeControlDD.value > 0){
						newContent = newContent + targetControl.stimHPos.ToString() + ","
							+ targetControl.iStimOn.ToString();
						//}

						newContent += Environment.NewLine;
						File.AppendAllText (filename, newContent);
					} //if recording
				//} // if _hmdSample.isValid
			}
			#endif
		}
	}
    private Vector3 niceAngles(Vector3 inVec)
    {
        Vector3 outVec = new Vector3(inVec.x, inVec.y, inVec.z);
        if (outVec.x > 180f)
            outVec.x -= 360f;
        if (outVec.y > 180f)
            outVec.y -= 360f;
        if (outVec.z > 180f)
            outVec.z -= 360f;
        return outVec;

    }
	private float niceAngle(float inAngle)
	{
		float angle = inAngle % 360f;
		if (inAngle > 180f)
			angle -= 360f;
		if (inAngle < -180f)
			angle += 360f;
		return angle;

    }
#if EYEMONITOR
    void OnApplicationQuit()
    {
        if (!isServer)
        {
            SMIGazeController.SMIcWrapper.smi_hideEyeImageMonitor();
        }
    }
#endif

    /**********************
	[Command]
	public void CmdLeftGazePos(Vector2 gazePos)
	{
		lGazeScreenPos = gazePos;
	}
	[Command]
	public void CmdRightGazePos(Vector2 gazePos)
	{
		rGazeScreenPos = gazePos;
	}
	******************/


    //outsourced to MenuCanvasControl
    /******************************************************************************
    [ServerCallback]
    public void OnRecording(bool bRecording)
    {
        Debug.Log("OnRecording");
        RpcOnRecording(bRecording);
    }
	[ClientRpc]
    public void RpcOnRecording(bool bRecording)
    {
        Debug.Log("RpcOnChangeRecording:" + bRecording.ToString());
        if (!isServer)
        {
            if (bRecording)
                StartRecording();
            else
                StopRecording();
        }
    }
    **********************************/

    /*********************Destroy not required since this module remains in memory until the end of the program
	void Destroy()
	{
		//remove listeners
		recToggle.onValueChanged.RemoveListener((value) => {OnRecording (value);});

	}
	public void Reset()
	{
		OnRecording (false);
	}
	******************/
}
