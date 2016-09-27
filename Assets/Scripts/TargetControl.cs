using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;


//Note: make shure that in ths Unity3d inspector the network ID component in the game object to which this scipt is attached to, is located above and not below this script.
//If the network ID component is below the script, all bad things will happen (game object disabled, therefore it is no longer reachable by the script

public class TargetControl : NetworkBehaviour {

	public enum SceneType:int {Sphere, Target, Both};
	//menu items for scnes with gaze targets only: outsources to 
	//public  InputField nearExcentricity{ get; set;}
	//public  InputField farExcentricity{ get; set; }
	//public InputField stField { get; set;}
	//public InputField isiField { get; set;}

	public int iStimOn
	{
		get 
		{
			if (myTargetRend.enabled)
				return 1;
			else
				return 0;
		}
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

	public float stimHPos
	{
		get {
			return targetYawAngle[targetPosIndex];
		}
	}

	public bool isVisible
	{
		get {return _isVisible;}
	}
	private float zPos, xnPos, xfPos, znPos, zfPos;
	private float stimTime, isi, remainingSt, remainingIsi;
	private int frameCount;

	private int _nStim = 15; // number of times the same stimulus is shown before going to the neext one 
	private GameObject myCamera;
	private GameObject myTarget;
	private GameObject myCenter;
	private MenuCanvasControl menuControl;
	private Vector3 lookAtCamera;
	public Vector3 [] targetPos;
	public float[] targetYawAngle;
	private Renderer myTargetRend;
	private Renderer myCenterRend; 
	private int targetPresentationIndex;
	private int targetPosIndex;
	private bool _isVisible;


	//private bool isRunning = false;
 
    //private MenuCanvasControl menuControl;

    // Use this for initialization
    void Start()
    {
		_isVisible = false;
        GetAccess ();
	}

	public void GetAccess()
	{
		try
		{

			myTarget = GameObject.Find ("MyTarget");
			myTargetRend = myTarget.GetComponent<MeshRenderer> ();
			myCenter = GameObject.Find("MyCenter");
			myCenterRend = myCenter.GetComponent<MeshRenderer>();
			myCamera = GameObject.Find("Camera_LEFT");
			lookAtCamera = myCamera.transform.localPosition;

			//my default scene layer viewing  with either eye for both eyes
			//myTarget.layer = LayerMask.NameToLayer("SphericalSkyBox");
			stimTime = 500f; //stimulus time 500ms
			isi = 2000f - stimTime; //interstimulus time interval 1950ms

			zPos = 20f;
			float xn = 15f;
			float xf = 30f;
			xnPos = zPos * Mathf.Sin(Mathf.Deg2Rad * xn);
			znPos = zPos * Mathf.Cos (Mathf.Deg2Rad * xn);
			xfPos = zPos * Mathf.Sin(Mathf.Deg2Rad * xf);
			zfPos = zPos * Mathf.Cos(Mathf.Deg2Rad * xf);
			targetYawAngle = new float[9]; //same as targetPos, but keeping the angle
			targetPos = new Vector3[9];
			targetYawAngle[0] = targetYawAngle[2] = targetYawAngle[4] = targetYawAngle[6] = targetYawAngle[8] = 0f;
			targetPos [0] = targetPos [2] = targetPos [4] = targetPos [6] = targetPos [8] = new Vector3 (0f, 0f, zPos);
			targetYawAngle[1] = -xn;
			targetPos [1] = new Vector3 (-xnPos, 0f, znPos);
			targetYawAngle[3] = xn;
			targetPos [3] = new Vector3 (xnPos, 0f, znPos);
			targetYawAngle[5] = -xf;
			targetPos [5] = new Vector3 (-xfPos, 0f, zfPos);
			targetYawAngle[7] = xf;
			targetPos [7] = new Vector3 (xfPos, 0f, zfPos);

			menuControl = GameObject.Find ("MenuCanvas").GetComponent<MenuCanvasControl> ();
			ResetTargetIndices();

		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("TargetControl GetMenuAccess Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log ("TargetControl GetMenuAccess  Error:" + exp.Message);
			return;
		}


	}

	public void ResetTargetIndices()
	{
		targetPresentationIndex = 0;
		targetPosIndex = 0;
		remainingSt = 0f;
		remainingIsi = isi * 0.001f;
	}

	private void InitTargetPos()
	{
		transform.localPosition = targetPos [targetPosIndex];
		//regular walk accoss all target position
		transform.LookAt (lookAtCamera);
	}
	private void TargetOn()
	{
		//if (!myTargetRend.enabled) {
		//yield return new WaitForEndOfFrame();
		myTargetRend.enabled = true;
		myCenterRend.enabled = true;
		//}
	}
	
	private void TargetOff()
	{
		//if (myTargetRend.enabled) {
		//yield return new WaitForEndOfFrame();
		myTargetRend.enabled = false;
		myCenterRend.enabled = false;
		//}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (isClient) {
			//coount down from the remaining time (is not dependent on scene duration
			if (_isVisible) { //0 = Sphere.Type
				if (remainingIsi > 0f)
				{	
					remainingIsi -= Time.fixedDeltaTime;
					if (remainingIsi <= 0f) {
						remainingSt = stimTime * 0.001f;
						//StartCoroutine(TargetOn()); //prodiuuces additional dotss
						TargetOn();
					}
				} else {
					remainingSt -= Time.fixedDeltaTime;
					if (remainingSt <= 0f) {
						remainingIsi = isi * 0.001f;
						TargetOff();
						targetPresentationIndex++;
						if (targetPresentationIndex >= _nStim) {
							targetPresentationIndex = 0;
							targetPosIndex++;
							if (targetPosIndex >= targetPos.Length)
								targetPosIndex = 0;
							InitTargetPos ();
						}
					}
				}
			}
			else
			{
				if (myTargetRend.enabled)
					TargetOff();
			}
		}
	}


	// get (string) value out of an InputField, if there is none, return default value stored in place holder
	private string GetStrVal(InputField field)
	{
		if (field.text.Length > 4)
			return field.text;
		else
			return field.placeholder.GetComponent<Text> ().text;
	}

	[ServerCallback]
	public void OnChangeStimRepeats(string  strVal)
	{
		try
		{
			int val = int.Parse(strVal);
			Debug.Log ("OnChangeStimRepeats: " + val.ToString ());
			RpcOnChangeStimRepeats(val);
			if (menuControl != null)
				menuControl.AddMenuEntry ("StimRepeats", strVal);
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
	[ClientCallback]
	public void RpcOnChangeStimRepeats(int val)
	{
		_nStim = val;
	}

	[ServerCallback]
	public void OnChangeNearExcentricity(string strAngle)
	{
		try
		{
			Debug.Log("OnChangeNearExcentricity" + strAngle);
			float angle = float.Parse(strAngle);
			RpcOnChangeNearExcentricity(angle);
			if (menuControl != null)
				menuControl.AddMenuEntry ("NearExcentricity", strAngle);

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
	public void RpcOnChangeNearExcentricity(float angle)
	{
		xnPos = zPos * Mathf.Sin(Mathf.Deg2Rad * angle);
		znPos = zPos * Mathf.Cos(Mathf.Deg2Rad * angle);
		targetPos [1] = new Vector3 (-xnPos, 0f, znPos);
		targetYawAngle [1] = -angle;
		targetPos [3] = new Vector3 (xnPos, 0f, znPos);
		targetYawAngle [3] = angle;
	}

	[ServerCallback]
	public void OnChangeFarExcentricity(string strAngle)
	{
		try
		{
			Debug.Log("OnChangeFarExcentricity" + strAngle);
			float angle = float.Parse(strAngle);
			RpcOnChangeFarExcentricity(angle);
			if (menuControl != null)
				menuControl.AddMenuEntry ("FarExcentricity", strAngle);

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
	public void RpcOnChangeFarExcentricity(float angle)
	{
		xfPos = zPos * Mathf.Sin(Mathf.Deg2Rad * angle);
		zfPos = zPos * Mathf.Cos(Mathf.Deg2Rad * angle);
		targetPos [5] = new Vector3 (-xfPos, 0f, zfPos);
		targetYawAngle [5] = -angle;
		targetPos [7] = new Vector3 (xfPos, 0f, zfPos);
		targetYawAngle [7] = angle;
	}

	[ServerCallback]
	public void OnChangeST(string strST)
	{
		try
		{
			Debug.Log("OnChangeST" + strST);
			float st = float.Parse(strST);
			RpcOnChangeST(st);
			if (menuControl != null)
				menuControl.AddMenuEntry ("StimulusTime", strST);

			
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
	public void RpcOnChangeST(float st)
	{
		stimTime = st;
	}

	[ServerCallback]
	public void OnChangeISI(string strISI)
	{
		try
		{
			Debug.Log("OnChangeST" + strISI);
			float iSI = float.Parse(strISI);
			RpcOnChangeISI(iSI);
			if (menuControl != null)
				menuControl.AddMenuEntry ("ISI", strISI);

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
	public void RpcOnChangeISI(float iSI)
	{
		isi = iSI;
	}

	[ServerCallback]
	public void OnChangeTargetScale(string txtScale)
	{
		try
		{
			Debug.Log ("OnChangeTargetScale:" + txtScale);
			float scale = float.Parse(txtScale);
			RpcOnChangeTargetScale(scale);
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
	public void RpcOnChangeTargetScale(float scale)
	{
		Vector3 vecScale = new Vector3(scale, scale, scale);
		transform.localScale = vecScale;
	}

	[ServerCallback]
	public void OnChangeWhichEyesControl(int val)
	{
		Debug.Log ("OnChangeWhichEyesControl: " + val.ToString());
		RpcOnChangeWhichEyesControl (val);
		if (menuControl != null)
			menuControl.AddMenuEntry ("WhichEyesControl", val.ToString());
	}
	[ClientRpc]
	public void RpcOnChangeWhichEyesControl(int val)
	{
		switch (val) {
		case 0: //both eyes
			gameObject.layer = LayerMask.NameToLayer("SphericalSkybox"); //14
			myCenter.layer = LayerMask.NameToLayer("SphericalSkybox");
			break;
		case 1: //left eye only
			gameObject.layer = LayerMask.NameToLayer("ViewLeftEye");  //11
			myCenter.layer = LayerMask.NameToLayer("ViewLeftEye"); 
			break;
		case 2: //right eye only
			gameObject.layer = LayerMask.NameToLayer("ViewRightEye"); //12
			myCenter.layer = LayerMask.NameToLayer("ViewRightEye"); 
			break;
		default: //both eyes
			gameObject.layer = LayerMask.NameToLayer("SphericalSkybox"); //14
			myCenter.layer = LayerMask.NameToLayer("SphericalSkybox");
			break;
		}
	}
	[ServerCallback]
	public void OnChangeVisability(bool bVisible)
	{
		RpcOnChangeVisability (bVisible);
        //ResetTargetIndices();

	}
	
	[ClientRpc]
	public void RpcOnChangeVisability (bool bVisible)
	{
		_isVisible = bVisible;
		ResetTargetIndices ();
        InitTargetPos();
	}


}
