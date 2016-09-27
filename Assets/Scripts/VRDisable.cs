using UnityEngine;

using System.Collections;

public class VRDisable : MonoBehaviour {

	// Use this for initialization to switch of VR
	void Awake () {
        UnityEngine.VR.VRSettings.enabled = false;
    }
	

}
