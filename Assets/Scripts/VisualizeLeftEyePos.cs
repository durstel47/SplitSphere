using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class VisualizeLeftEyePos :  NetworkBehaviour  {

	private GameObject menuCanvas;
	private RecordEyesPosition recordEye;
	private Vector2 screenPos;

    // Use this for initialization
    override public void OnStartLocalPlayer()
    {
		menuCanvas = GameObject.Find ("MenuCanvas");
		recordEye = menuCanvas.GetComponent<RecordEyesPosition>();
		if (!isServer) {
			GetComponent<MeshRenderer> ().enabled = false;
		} else {
			GetComponent<MeshRenderer> ().enabled = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!isServer) {
			if (!screenPos.Equals (recordEye.lEyeScreen)) {
				screenPos = recordEye.lEyeScreen;
				CmdOnChangedScreenPos(screenPos);
			}
		}
	}
	[Command]
	void CmdOnChangedScreenPos(Vector2 pos)
	{
		if (isServer) {
			Debug.Log ("pos:" + pos.ToString());
			Vector3 vec = new Vector3(pos.x, pos.y);
			transform.localPosition = vec;
		}
	}
}
