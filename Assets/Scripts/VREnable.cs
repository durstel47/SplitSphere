using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.VR;

using System.Collections;

public class VREnable :NetworkBehaviour {


    void Awake()
    {

    }
    // Use this for initialization
    void Start ()
   {
        if (base.isServer)
        {
			Debug.Log ("ADMIN:don't enablee VR");
        }
        else
        {
            Debug.Log("STAGE: Enable VR Device now ");
            VRSettings.enabled = true;
            InputTracking.Recenter();
        }
    }

}
