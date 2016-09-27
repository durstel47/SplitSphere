using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.Networking;
//using UnityEngine.VR;
//using UnityEngine.Events;
//using UnityEngine.EventSystems;
using System;
//using System.Collections;
//using System.IO;

public class SetSphereParameters : NetworkBehaviour {

    //public GameObject speedLabelNumInputField;
    //public GameObject contrastLabelNumInputField;
    //public GameObject panoDropDownField;

	//We outsource all the menu funtion to the MenuCanvasControl which will communicate with this module using the servers callbacks
	//There, we also initialize its parameters with values from the menu
	//public InputField speedInputField { get; set; }
	//public InputField contrastInputField { get; set; }

	private Vector3 axis = Vector3.up;

    private float sphereSpeed;
	private float sphereContrast;

	private Renderer rend;
	private MenuCanvasControl menuControl;
	private ReverseNormals reverseNormals;
	private Material sphereMat;
    //private Dropdown panoDropDown;
	private Texture panoTexture;
    public bool isVisible = true;

    public float drumSphereSpeed
    {
        get
        {
            if (axis.Equals(Vector3.up))
                return sphereSpeed;
            else
                return -sphereSpeed;
        }
    }

    public float drumSphereContrast 
	{
		get { return 100f * sphereContrast;}
    }
    // Use this for initialization. One needs do do this before Enable/disable scripts are
    void Start()
    {
        try {
			sphereMat = gameObject.GetComponent<Renderer> ().material;
			reverseNormals = GetComponent<ReverseNormals>();
			GetMenuAccess();
            ChangeVisability(isVisible);
        }
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Start SetSphereParams Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log (" Start SetSphereParams Error:" + exp.Message);
			return;
		}

    }
    private void ChangeVisability(bool bVisible)
    {
        if (bVisible)
        {
            if (reverseNormals.isReversed)
                reverseNormals.Reverse();
        }
        else {
            if (!reverseNormals.isReversed)
                reverseNormals.Reverse();
        }

    }

    private void GetMenuAccess()
	{
		try {
			// note: we change the texture of the sphere from outside (menucanvasControl) by exchanging the maintexture of the sphere's material by using a reference to the sphere's renderer
			menuControl = GameObject.Find("MenuCanvas").GetComponent<MenuCanvasControl> ();
			menuControl.AccessSphere ();
			if (menuControl.spherePanoTexture != null)
				sphereMat.mainTexture = menuControl.spherePanoTexture; //get appropriate texture from the main menu control
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Start SetSphereParams Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log (" Start SetSphereParams Error:" + exp.Message);
			return;
		}
	}
		
	// Update is called once per frame
    void FixedUpdate () 
	{
       transform.Rotate(axis, sphereSpeed * Time.deltaTime);	
    }


	// get (string) value out of an InputField, if there is none, return default value stored in place holder
	/******************
	private string GetStrVal(InputField field)
	{
		if (field.text.Length > 0)
			return field.text;
		else
			return field.placeholder.GetComponent<Text> ().text;
	}
	*****************/
	
   [ServerCallback]
	public void OnChangeSpeed(string strSpeed)
    {
        try
        {
            Debug.Log("Speed " + strSpeed);
            float speed = float.Parse(strSpeed);
            RpcChangeSpeed(speed);
			if (menuControl != null)
				menuControl.AddMenuEntry ("SphereYawSpeed", strSpeed);

        }
		catch (System.NullReferenceException nExp)
		{
			Debug.Log("OnChangeSpeed;" + nExp.Message);
			return;
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
    public void  RpcChangeSpeed(float speed)
    {
        Debug.Log("Clientspeed: " + speed.ToString());
        if (speed < 0)
        {
            sphereSpeed = -speed;
            axis = Vector3.down;
        }
        else
        {
            sphereSpeed = speed;
            axis = Vector3.up;
        }
    }


    [ServerCallback]
	public void OnChangeContrast(string strContrast)
    {
        try
        {
			Debug.Log("Sphere contrast" + strContrast);
			sphereContrast = float.Parse(strContrast) * 0.01f;
            RpcChangeContrast(sphereContrast);
			if (menuControl != null)
				menuControl.AddMenuEntry ("SphereTextureContrast", strContrast);

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
    public void RpcChangeContrast(float contrast)
    {
        sphereContrast = contrast;
        sphereMat.SetFloat("_Contrast", contrast);
    }


	[ServerCallback]
	public void OnChangeVisability(bool bVisible)
	{
		RpcOnChangeVisability (bVisible);
	}

	[ClientRpc]
	public void RpcOnChangeVisability (bool bVisible)
	{
        ChangeVisability(bVisible);
	}
}
