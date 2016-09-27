using UnityEngine;
using UnityEngine.Networking;


public class RotatingRandomDots : NetworkBehaviour
{

	public int numOfDots = 1000;
	public Transform dot;
	public float rotVel = 5f;
	public float distance = 40f;
	public int numOfCycles = 3;
    public float backgroundLum = 0.2f;

	private GameObject camRig;
    private Camera leftCam;
    private Camera rightCam;
	private Transform _rotCenterTransform;
	public Transform rotCenterTransform {
		get { return _rotCenterTransform; }
	}
	private Vector3 _rotAxis;
	public Vector3 rotAxis {
		get { return _rotAxis; }
	}
	private float _rotAngle;
	public float rotAngle {
		get { return _rotAngle; }
	}

	private Transform[] dots = null;

    //access to menu
    private MenuCanvasControl menuControl;

    // Use this for initialization
    void Start()
    {
        camRig = GameObject.Find("LeftCameraHolder");
        leftCam = GameObject.Find("Camera_LEFT").GetComponent<Camera>();
        rightCam = GameObject.Find("Camera_RIGHT").GetComponent<Camera>();
        menuControl = GameObject.Find("MenuCanvas").GetComponent<MenuCanvasControl>();
        _rotAxis = Vector3.up;
        _rotCenterTransform = camRig.transform;
    }

	// Update is called once per frame
	void Update () {
		if (rotVel >= 0f) {
			_rotAngle = rotVel * Time.deltaTime;
			_rotAxis = Vector3.up;
		} else {
			_rotAngle = -rotVel * Time.deltaTime;
			_rotAxis = Vector3.down;
		}
        /*********************************
		for (int i = 0; i < dots.Length; i++) {
			if ((i + 1) % iCycle == 0) {
				dots[i].transform.localPosition = distance * Random.onUnitSphere;
			} else {
				dots [i].transform.RotateAround (_rotTransform.localPosition, _rotAxis, _rotAngle);
			}			
		}

		iCycle++;
		if (iCycle > numOfCycles)
			iCycle = 1;
        ******************************/
	}
    //to be called if number of dots has to be changed!
	public void InitDots(int nDots)
	{
        DestroyDots();
        //Vector3 vec;
		dots = new Transform[nDots];
		for (int i = 0; i < nDots; i++) {
			//vec = distance * Random.onUnitSphere;
			//dots[i] = (Transform) Instantiate (dot, vec, Quaternion.identity);
			dots[i] = (Transform)Instantiate(dot);
		}
        numOfDots = dots.Length;

	}
    private void DestroyDots()
    {
        if (dots == null)
            return;
        if (dots.Length > 0)
        {
            for (int i = 0; i < dots.Length; i++)
                Destroy(dots[i].gameObject);
        }
        dots = null;
    }

    [ServerCallback]
    public void OnChangeNumOfDots(string strVal)
    {
        try
        {
            int nDots = int.Parse(strVal);
            Debug.Log("NumOfDots:" + nDots.ToString());
           if (menuControl != null)
                menuControl.AddMenuEntry("NumOfDots", strVal);
            RpcOnChangeNumOfDots(nDots);
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
    public void RpcOnChangeNumOfDots(int nDots)
    {
        InitDots(nDots);
    }
    [ServerCallback]
    public void OnChangeNumOfLifeCyles(string strVal)
    {
        try
        {
            int val = int.Parse(strVal);
            Debug.Log("NumOfLifeCycles:" + val.ToString());
            if (menuControl != null)
                menuControl.AddMenuEntry("NumOfLifeCyles", strVal);
            RpcOnChangeNumOfLifeCycles(val);
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
    public void RpcOnChangeNumOfLifeCycles(int nLifeCyles)
    {
        numOfCycles = nLifeCyles;
        if (dots == null)
            return;
    }

    [ServerCallback]
    public void OnChangeDotRotVel(string strVal)
    {
        try
        {
            float val = float.Parse(strVal);
            Debug.Log("DotRotVel:" + val.ToString());
            if (menuControl != null)
                menuControl.AddMenuEntry("DotRotVel", strVal);
            RpcOnChangeDotRotVel(val);
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
    public void RpcOnChangeDotRotVel(float vel)
    {
        rotVel = vel;
    }

    [ServerCallback]
    public void OnChangeDotDistance(string strVal)
    {
        try
        {
            float val = float.Parse(strVal);
            Debug.Log("DotDistance:" + val.ToString());
            if (menuControl != null)
                menuControl.AddMenuEntry("DotDistance", strVal);
            RpcOnChangeDotDistance(val);
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
    public void RpcOnChangeDotDistance(float dist)
    {
        distance = dist;
    }

    [ServerCallback]
    public void OnChangeBgLuminance(string strVal)
    {
        try
        {
            float val = float.Parse(strVal);
            Debug.Log("BgLuminance:" + val.ToString());
            if (menuControl != null)
                menuControl.AddMenuEntry("BgLuminance", strVal);
            float luminance = 0.01f * val;
            RpcOnChangeBgLuminance(luminance);

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
    public void RpcOnChangeBgLuminance(float luminance)
    {
        Color bgCol = new Color(luminance, luminance, luminance);
        leftCam.backgroundColor = bgCol;
        rightCam.backgroundColor = bgCol;
        backgroundLum = luminance;
    }


    [ServerCallback]
    public void OnChangeVisability(bool isVisible)
    {
        RpcOnChangeVisability(isVisible);
    }

    [ClientRpc]
    public void RpcOnChangeVisability(bool isVisible)
    {
        if (isVisible)
        {
            InitDots(numOfDots);
            Color bgCol = new Color(0.2f, 0.2f, 0.2f);
            leftCam.backgroundColor = bgCol;
            rightCam.backgroundColor = bgCol;
        }
        else
        {
            DestroyDots();
            leftCam.backgroundColor = Color.black;
            rightCam.backgroundColor = Color.black;
        }
    }


}


