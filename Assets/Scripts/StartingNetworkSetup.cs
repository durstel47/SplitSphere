//#define ADMIN   set this in scripts sefines in builder options
//set define above before compiling for Admin with VR disabled.
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.VR;
using System.Collections;



public class StartingNetworkSetup : MonoBehaviour {


    private NetworkManager netManager;
    private GameObject offlineCanvas;
#if ADMIN
    private Button startButton;
    private GameObject clientControls;
#else
    private GameObject serverControls;
	private InputField serverInputField;
	private Button joinButton;
#endif

    // Use this for initialization
    void Start () {
		netManager = GetComponent<NetworkManager> ();
        offlineCanvas = GameObject.Find("OfflineCanvas");

#if ADMIN
        clientControls = (GameObject)Resources.Load<GameObject> ("ClientControls");
		clientControls = Instantiate (clientControls, Vector3.zero, Quaternion.identity) as GameObject;
		clientControls.name = clientControls.name.Replace ("(Clone)", "");
		clientControls.transform.SetParent (offlineCanvas.transform);
		clientControls.transform.localScale = new Vector3(1f, 1f, 1f);
		startButton = clientControls.GetComponentInChildren<Button> ();
		startButton.onClick.AddListener (() => {
			OnStartTesting ();});
#else
        /******
		serverControls = (GameObject)Resources.Load<GameObject>("ServerControls");
		serverControls.name = serverControls.name.Replace("(Clone)", "");
		serverControls = Instantiate(serverControls , Vector3.zero, Quaternion.identity) as GameObject;
		serverControls.transform.SetParent(offlineCanvas.transform);
		serverInputField = serverControls.GetComponentInChildren<InputField>();
		serverInputField.onEndEdit.AddListener ((value) => {OnChangeIPAdress (value);});
		joinButton = serverControls.GetComponentInChildren<Button>();
		joinButton.onClick.AddListener(() => {OnJoinTest();});
        ******/
        OnChangeIPAdress("localhost");
        OnJoinTest();
#endif
	}
#if ADMIN
	public void OnStartTesting()
	{
		Debug.Log ("Starting Host");
		netManager.StartHost ();
	}
 	public void OnClientDisconnectedFromHost(NetworkConnection conn)
	{
	    clientControls = (GameObject)Resources.Load<GameObject> ("ClientControls");
	    clientControls = Instantiate (clientControls, Vector3.zero, Quaternion.identity) as GameObject;
	    clientControls.name = clientControls.name.Replace ("(Clone)", "");
	    clientControls.transform.SetParent (offlineCanvas.transform);
	    clientControls.transform.localScale = new Vector3(1f, 1f, 1f);
	    startButton = clientControls.GetComponentInChildren<Button> ();
		startButton.onClick.RemoveAllListeners();
	    startButton.onClick.AddListener (() => {
		OnStartTesting ();});
	}


#else
	public void OnJoinTest()
	{
		Debug.Log ("Starting Client");
		netManager.StartClient ();
	}

    public void OnChangeIPAdress(string adress)
    {
        Debug.Log("Change IPAdress" + adress);
        if(adress != null && adress.Length > 0)
        {
            netManager.networkAddress = adress;
        }
    }
	/******does not work on timeout
	public void OnClientDisconnect(NetworkConnection conn)
	{
		serverControls = Instantiate(serverControls , Vector3.zero, Quaternion.identity) as GameObject;
		serverControls.transform.SetParent(offlineCanvas.transform);
		serverInputField = serverControls.GetComponentInChildren<InputField>();
		serverInputField.onEndEdit.AddListener ((value) => {OnChangeIPAdress (value);});
		joinButton = serverControls.GetComponentInChildren<Button>();
		joinButton.onClick.AddListener(() => {OnJoinTest();});
	}
	*****************/
#endif

} //end StartingNetworkSetup
