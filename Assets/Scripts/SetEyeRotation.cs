using UnityEngine;
using UnityEngine.VR;
//using UnityEngine.UI;
using UnityEngine.Networking;
//using UnityEngine.Events;
//using UnityEngine.EventSystems;
//using SMI;
using System;
//using System.Collections;

//Note: Network scripts should by attached to "real" Unity objects like the MenuCanvas here. Network scripts attached to an empty transfrom risk not to be callled by 
//network calls even when accompangyied by a valid network ID

public class SetEyeRotation : NetworkBehaviour {

	public int eyeCamControlOption { 
		get { return _eyeCamControlOption; } 
	}
	public float horGazeGain {
		get { return _horGazeGain; }
	}
	public float speedLimit {
		get { return _speedLimit; }
	}
	public Quaternion cameraOrientation
	{
		get { return cameraHolder.localRotation;}
	}

    public int stabilisationSource
    {
        get { return _stabilisationSource; }
    }

	private int _eyeCamControlOption;
    private int _stabilisationSource;
	private float _horGazeGain = 1f;
	private float _speedLimit = 30f;

	public Quaternion hmdRotation;


	private MenuCanvasControl menuControl;

	private bool _isTargetToggle = false;
	private RecordEyesPosition recordEye;
	private Transform cameraHolder;
     private Quaternion oculusOrientation;
	private Quaternion gazeQuat;
	private Vector3 oculusPosition;
	private Vector3 gazeAngles;
	private Vector2 screenPos;
	private Vector2 rScreenPos;
	private Vector3 gazeBasePt;
	private Camera rayCastCam; // camera to which this script is attached
	private bool isLeftEye;
	private GameObject cameraTarget;
	private GameObject menuCanvas;
	private GameObject gazeTarget; //gaze target based on SMI measurementss of eye position placed onto the display using an SMI subroutine
                                   //private Quaternion offRot;
    private Vector2 dir;
	private Vector3 lastDir;
    //private Vector3 oldDir;
	//private Vector2 oldGazePos;
	private Quaternion hmdRot;

	void Awake()
	{
		try {
			menuCanvas = GameObject.Find ("MenuCanvas");
			menuControl = menuCanvas.GetComponent<MenuCanvasControl> ();
			recordEye = menuCanvas.GetComponent<RecordEyesPosition> ();
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Awake SetEyeRot Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log (" Awake SetEyeRot Error:" + exp.Message);
			return;
		}

	}

    // Use this for initialization
    void Start()
    {
        try {
			menuCanvas = GameObject.Find ("MenuCanvas");
			menuControl = menuCanvas.GetComponent<MenuCanvasControl> ();
			recordEye = menuCanvas.GetComponent<RecordEyesPosition> ();
			cameraHolder = transform.parent;
			cameraHolder.rotation = Quaternion.identity;
			rayCastCam = GetComponent<Camera> (); 
			if (rayCastCam.name.Equals ("Camera_LEFT")) {
				isLeftEye = true;
			} else {
				isLeftEye = false;
			}

			_eyeCamControlOption = 0; //0: sphere earthfixed, camera attached to  oculus rift (head fixed view)
            _stabilisationSource = 0; //0: stabilise using left eye position only


			//Set Spheres as Camera targets
			cameraTarget = LoadCameraTarget ();

			//make target sphere for gaze direction
			gazeTarget = LoadGazeTarget();
			//keep gaze and camera targets allways visible on the server, provide a toggle for the cl
			if (isServer) {
				gazeTarget.GetComponent<MeshRenderer> ().enabled = true;
				if (_isTargetToggle) {
					//make gaze target visible by enabling its drawing routine.
					cameraTarget.GetComponent<MeshRenderer> ().enabled = true;
				} else {
					//make the gaze target invisible by disenabling its drawing routine. So the gaze target is still active, but invisible.
					cameraTarget.GetComponent<MeshRenderer> ().enabled = false;
				}
			} else {
				if (_isTargetToggle) {
					//make gaze target visible by enabling its drawing routine.
					gazeTarget.GetComponent<MeshRenderer> ().enabled = true;
					cameraTarget.GetComponent<MeshRenderer> ().enabled = true;
				} else {
					//make the gaze target invisible by disenabling its drawing routine. So the gaze target is still active, but invisible.
					gazeTarget.GetComponent<MeshRenderer> ().enabled = false;
					cameraTarget.GetComponent<MeshRenderer> ().enabled = false;
				}
			}
			hmdRotation = Quaternion.identity;
			if (VRDevice.isPresent)
			{
				InputTracking.Recenter(); // reset the Oculus Drift tracking system
	        }
			lastDir = Vector3.forward;
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Start SetEyeRot Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log (" Start SetEyeRot Error:" + exp.Message);
			return;
		}
    }

 
	Quaternion HMDOrientation(bool isLeftEye)
	{
		Quaternion orientation;
    
		if (isLeftEye)
		{
			orientation = InputTracking.GetLocalRotation(VRNode.LeftEye);
		}
		else
		{
			orientation = InputTracking.GetLocalRotation(VRNode.RightEye);
		}
		return orientation;
	}
	//load small spherical target indicating straight ahead "gaze" direction of the eye cameras (right red, right green)

	GameObject LoadCameraTarget()
	{
		GameObject sphere;
		if (isLeftEye)
			sphere = (GameObject)Resources.Load<GameObject>("LeftTargetSphere");
		else
			sphere = (GameObject)Resources.Load<GameObject>("RightTargetSphere");
		sphere = Instantiate<GameObject> (sphere) as GameObject;
		sphere.transform.SetParent (rayCastCam.transform);
		sphere.transform.localPosition = new Vector3 (0f, 0f, 20f);
		return sphere;
	}

	GameObject LoadGazeTarget()
	{
		GameObject target;
		if (isLeftEye)
			target = (GameObject)Resources.Load<GameObject> ("LeftGazeTarget");
		else
			target = (GameObject)Resources.Load<GameObject> ("RightGazeTarget");
		target = Instantiate<GameObject> (target) as GameObject;
		if (isServer)
			target.layer = LayerMask.NameToLayer ("Default");
		target.transform.SetParent (rayCastCam.transform);
        target.transform.localPosition = new Vector3(0f, 0f, 20f);
		//draw the gaze target as estimated from the SMI eyetracker 
		return target;
	}

    void Update()
    {
        if (VRDevice.isPresent) {
			Ray ray;
			Vector2 gain = new Vector2 (_horGazeGain, 1f);
            hmdRot = HMDOrientation(isLeftEye);
			if (!isServer) {
                 if (recordEye.IsValidSample)
                {
                    switch (_stabilisationSource)
                    {
                        case 0:  //left eye
                            screenPos = recordEye.lEyeScreen;
                            break;
                        case 1: //right eye
                            screenPos = recordEye.rEyeScreen;
                            break;
                        case 2:  //either eye
                            if (isLeftEye)
                                screenPos = recordEye.lEyeScreen;
                             else
                                screenPos = recordEye.rEyeScreen;
                            break;
                        default:
                            screenPos = recordEye.lEyeScreen;
                             break;

                    }
                    //draw the gaze target as estimated from the SMI eyetracker 
                    dir = smi_transformGazePositionToWorldPosition (screenPos);
                    //use info from the previous frame to show gaze target
                    ray = new Ray(Vector3.zero, lastDir);
                    gazeTarget.transform.localPosition = ray.GetPoint(20f);
                    lastDir = dir;
                }
			}

			switch (_eyeCamControlOption) {
			case 0: // Camera follows head movement: head controls cameras, gaze is not involved in camera movement
				break; //nothing to do-> default Coulus Rift setting with cameras controlled by oculus rift orientation and position sensors
			case 1: //Camera follows gaze movement we look at the gaze target points by pointing the camraholder in the direction of the gaze target
                    //cameraHolder.localPosition += offset; // not quite shure if would need something like this. Offset between hold and cam are mostly below 1°
                    //here we try to implement a kind of feedback gain by shifting the apparent screen position of the gaze target
                    //formula is slightly wrong substituting gain * angle for point distance * sin(gain * angle)
                //if (recordEye.IsValidSample)
                        //cameraHolder.localRotation = SetHmdOrientation (hmdEuler) * GetGaze (screenPos, gain) * InverseHmdOrientation (hmdEuler);
                        cameraHolder.localRotation = hmdRot * GetGaze(screenPos, gain) * Quaternion.Inverse(hmdRot); //adds head and gaze rotation
                        //cameraHolder.localRotation = hmdRot * GetSMIGaze(eyeGazeDir, gain) * Quaternion.Inverse(hmdRot); ;
				        //cameraHolder.localPosition -= offset;
 				break;
			case 2: //Head fixed world: here we do not consider head poisition tracking
                    //cameraHolder.localPosition += offset;
                    cameraHolder.localRotation = Quaternion.Inverse(hmdRot);
				//cameraHolder.localPosition -= offset;
				break;
			case 3: //Gaze fixed world
                    //cameraHolder.localPosition += offset;
                   // if (recordEye.IsValidSample)
                        //cameraHolder.localRotation = GetGaze (screenPos, gain) * InverseHmdOrientation (hmdEuler);
                        cameraHolder.localRotation = StabilizeGaze(screenPos, gain) * Quaternion.Inverse(hmdRot); //subtracts gaze rotation
                        //cameraHolder.localRotation = GetSMIGaze(eyeGazeDir, gain) * Quaternion.Inverse(hmdRot);
                    //else
                    //    cameraHolder.localRotation = Quaternion.Inverse(hmdRot);
                    //cameraHolder.localPosition -= offset;
                    break;
			default:
				break;

			} // end switch
		}// if (VR.Deveice.isPresent
    } 

	private Quaternion InverseHmdOrientation(Vector3 euler)
	{
		Quaternion invYaw = Quaternion.AngleAxis(euler.y, Vector3.down); //inverse yaw
		Quaternion invPitch = Quaternion.AngleAxis(euler.x, Vector3.left); //pitch 
		Quaternion invRoll = Quaternion.AngleAxis(euler.z, Vector3.back); //roll
		return invRoll * invPitch * invYaw;
	}
	private Quaternion SetHmdOrientation(Vector3 hmdEuler)
	{
		Quaternion qYaw = Quaternion.AngleAxis(hmdEuler.y, Vector3.up);
		Quaternion qPitch = Quaternion.AngleAxis(hmdEuler.x, Vector3.right);
		Quaternion qRoll = Quaternion.AngleAxis(hmdEuler.z, Vector3.forward);
		return qYaw * qPitch * qRoll;
	}

	private Quaternion GetGaze(Vector2 screenPos, Vector2 gain)
	{
		Quaternion gazeQuat = Quaternion.identity;
        Vector3 dir = smi_transformGazePositionToWorldPosition(screenPos); //return gaze direction realtive to Oculus rift screen, not relative to the virtual word
		//Vector2 gazePosChange = new Vector2(dir.x, dir.y) - new Vector2(oldDir.x, oldDir.y);
		//float angChange = Mathf.Asin (gazePosChange.magnitude / dir.z);
		//float gazeSpeed = angChange * 1.0f / Time.deltaTime;
		Vector3 newDir;
        //if (gazeSpeed < _speedLimit)
            newDir = new Vector3(dir.z * gain.x * Mathf.Atan2(dir.x, dir.z), dir.z * gain.y * Mathf.Atan2(dir.y, dir.z), dir.z);
            //newDir = new Vector3(gain.x * Mathf.Atan2(dir.x, dir.z), gain.y * Mathf.Atan2(dir.y, dir.z), 1.0f);
        //else
        //    newDir = new Vector3(oldDir.z * gain.x * Mathf.Atan2(oldDir.x, oldDir.z), oldDir.z * gain.y * Mathf.Atan2(oldDir.y, oldDir.z), oldDir.z);
            //newDir = new Vector3(gain.x * Mathf.Atan2(oldGazePos.x, dir.z), gain.y * Mathf.Atan2(oldGazePos.y, dir.z), 1.0f);
        //Debug.Log ("dir: " + dir.ToString () + " newDir: " + newDir.ToString ());
		gazeQuat.SetLookRotation (newDir.normalized, Vector3.up);
        //oldGazePos = newGazePos; //store previous gaze direction
        //oldDir = dir;

		return gazeQuat;
	}
    private Quaternion StabilizeGaze(Vector2 screenPos, Vector2 gain)
    {
        Quaternion gazeStab = Quaternion.identity;
        Vector3 dir = smi_transformGazePositionToWorldPosition(screenPos);
        Vector3 newDir = new Vector3(dir.z * -gain.x * Mathf.Atan2(dir.x, dir.z), dir.z * -gain.y * Mathf.Atan2(dir.y, dir.z), dir.z);
        gazeStab.SetLookRotation(newDir.normalized, Vector3.up);
        return gazeStab;
    }
    private Quaternion GetSMIGaze(Vector3 gazeDir, Vector2 gain)
    {
        gazeDir.x *= gain.x;
        gazeDir.y *= gain.y;
        Debug.Log("gazeDir: " + gazeDir.ToString());
        Quaternion gazeQuat = Quaternion.identity;
        gazeQuat.SetLookRotation(gazeDir.normalized, Vector3.up);
        return gazeQuat;
    }

	public void ResetEyeOrientation()
	{
		cameraHolder.localRotation = InputTracking.GetLocalRotation(VRNode.CenterEye);
	}

 	//Some help from SMI (see SMIGazeController.cs)
	/// <summary>
	/// Transform the raw-Gazeposition into the WorldPosition of the 3D-World of Unity
	/// </summary>
	/// <param name="gazePos">the raw-GazePosition</param>
	/// <returns>Transformed GazePosition in the World Space</returns>
	private Vector3 smi_transformGazePositionToWorldPosition(Vector2 gazePos)
	{
		float planeDistForMapping = 1.5f;
		float gazeScreenWidth = 1920f;
		float gazeScreenHeight = 1080f;
		float horizFieldOfView = 87f * Mathf.Deg2Rad;
		float vertFieldOfView = horizFieldOfView;
		//Vector2 curGazePos;
		
		float xOff = planeDistForMapping * Mathf.Tan(horizFieldOfView / 2f);
		float yOff = planeDistForMapping * Mathf.Tan(vertFieldOfView / 2f);
		float zOff = planeDistForMapping;

		Vector3 gazePosInWorldSpace = new Vector3(smi_calculateGazeOffset(gazePos.x, gazeScreenWidth, xOff), -smi_calculateGazeOffset(gazePos.y, gazeScreenHeight, yOff), zOff);
		
		return gazePosInWorldSpace;
	}

	/// <summary>
	/// Stolen from SMI by MRD
	/// Calculate the gaze offset to the screen width per vector component
	/// </summary>
	/// <param name="xin"></param>
	/// <param name="gazeScreenWidth"></param>
	/// <param name="offset"></param>
	/// <returns></returns>
	private float smi_calculateGazeOffset(float xin, float gazeScreenWidth, float offset)
	{
		return (xin * 2f * offset) / gazeScreenWidth - offset;
	}

	/// <summary>
	/// Calculate a ray based on the position and the averaged POR 
	/// </summary>
	/// <returns> A Ray based from the Gaze Direction</returns>
	public Ray smi_getRayFromGaze(Vector3 porAverageGaze)
	{
		Matrix4x4 localToWorldMatrixCamera = rayCastCam.gameObject.transform.localToWorldMatrix;
		Matrix4x4 playerTransformMatrix = Matrix4x4.identity;
		
		//Vector3 porAverageGaze = smi_getSample().por;
		Vector3 cameraPor3d = smi_transformGazePositionToWorldPosition(porAverageGaze);
		
		//Position of the GazePos
		Vector3 instancePosition = playerTransformMatrix.MultiplyPoint(localToWorldMatrixCamera.MultiplyPoint(cameraPor3d));
		
		//calulate the Direction of the Gaze
		Vector3 zeroPoint = playerTransformMatrix.MultiplyPoint(localToWorldMatrixCamera.MultiplyPoint(Vector3.zero));
		Vector3 gazeDirection = playerTransformMatrix.MultiplyPoint((instancePosition - zeroPoint));
		
		return new Ray(transform.position, gazeDirection);
	}
	[ServerCallback]
	public void OnChangeTargetToggle(bool isOn)
	{
		if(isLeftEye) //add common entry only once
			menuControl.AddMenuEntry ("TargetToggle", isOn.ToString());
		Debug.Log ("OnChangeTargetToggle to: " + isOn.ToString ());

		RpcOnChangeTargetToggle(isOn);
	}
	[ClientRpc]
	public void RpcOnChangeTargetToggle(bool isOn)
	{
		Debug.Log ("RpcOnChangeTargetToggle to: " + isOn.ToString ());
		_isTargetToggle = isOn;
		if ((gazeTarget != null) && (cameraTarget != null)) {
			if (!isServer) {
				if (isOn) {
					gazeTarget.GetComponent<MeshRenderer> ().enabled = true;
					cameraTarget.GetComponent<MeshRenderer> ().enabled = true;
				} else {
					gazeTarget.GetComponent<MeshRenderer> ().enabled = false;
					cameraTarget.GetComponent<MeshRenderer> ().enabled = false;
				}
			} else {
				if (isOn) {
					gazeTarget.GetComponent<MeshRenderer> ().enabled = true;
					cameraTarget.GetComponent<MeshRenderer> ().enabled = true;
				} else {
					gazeTarget.GetComponent<MeshRenderer> ().enabled = true;
					cameraTarget.GetComponent<MeshRenderer> ().enabled = false;
				}
			}
		} else
			Debug.Log ("Gaze od camera target not yet found!");
	}

	[ServerCallback]
	public void OnChangeEyeCamControl(int option)
	{
		if(isLeftEye)
			menuControl.AddMenuEntry ("EyeCamControl", option.ToString ());
		Debug.Log ("Change EyeCamControl: " + option.ToString ());
		RpcOnChangeEyeCamControl (option);
		
	}
	[ClientRpc]
	public void RpcOnChangeEyeCamControl(int option)
	{
		_eyeCamControlOption = option;
		Debug.Log ("RpcOnChangeEyeCamControl: " + _eyeCamControlOption.ToString ());
	}

    [ServerCallback]
    public void OnChangeStabilisationSource(int option)
    {
        if (isLeftEye)
            menuControl.AddMenuEntry("StabilisationSource", option.ToString());
        Debug.Log("Change Stabilisation Source:" + option.ToString());
        RpcOnChangeStabilisationSource(option);
    }

    [ClientRpc]
    public void RpcOnChangeStabilisationSource(int option)
    {
        _stabilisationSource = option;
        Debug.Log("RpcOnChangeStabilisationSource:" + option.ToString());
    }

	[ServerCallback]
	public void OnChangedFeedbackGain(string strFeedbackGain)
	{
		if (isLeftEye)
			menuControl.AddMenuEntry ("FeedbackGain", strFeedbackGain);
		Debug.Log ("Hor. FeedbacGain :" + strFeedbackGain);
		try
		{
			float feedbackGain = float.Parse(strFeedbackGain);
			RpcOnChangedFeedbackGain(feedbackGain);
			
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
	public void RpcOnChangedFeedbackGain(float feedbackGain)
	{
		_horGazeGain = feedbackGain;
		Debug.Log ("RpcOnChangedFeedbackGain: " + _horGazeGain.ToString ());
	}

    /*******************************************************
	[ServerCallback]
	public void OnChangeSpeedLimit(string strLimit)
	{
		if (isLeftEye)
			menuControl.AddMenuEntry ("SpeedLimit", strLimit); 
		try
		{
			Debug.Log("Limit " + strLimit);
			float limit = float.Parse(strLimit);
			RpcChangeSpeedLimit(limit);
			
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
	public void  RpcChangeSpeedLimit(float limit)
	{
		Debug.Log("RpcChangeSpeedLimit" + limit.ToString());
		_speedLimit = limit;
	}
    **************************************/
	float niceAngle(float inAngle)
	{
		float angle = inAngle % 360f;
		if (inAngle > 180f)
			angle -= 360f;
		if (inAngle < -180f)
			angle += 360f;
		return angle;
		
	}

} // end SetEyeRotation


