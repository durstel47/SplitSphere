using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.VR;
using UnityEngine.Networking;
using System.Collections;
//using System.Collections.Generic;
using System;
using System.IO;
using SMI; //to enable eye monitor





public class MenuCanvasControl : NetworkBehaviour {

    public enum SceneType : int { Sphere, Target, Both, None, Dots };

    //Access to different sceneEntries menu panels
    //we have to different menus: namely the menuje for the scens with a panorama sphere and the target menu for the scenes 
    //with gaze targets only. We load this menus from the resources folder within out prefab folder
    //private GameObject sphereMenu;
    //private GameObject targetMenu;

    private GameObject[] scenePanels;
    private Transform myPanel;

    // Use this for initialization of scenes
    private int _taskIndex = 0;
    public int taskIndex
    {
        get { return _taskIndex; }
    }
    private int _taskId = 0; //used in stage client to keep track of current task id
    public int taskId
    {
        get { return _taskId;  }
    }
    private int _sceneIndex = 0; //used in stage client to keep track of current scene index
    public int sceneIndex
    {
        get { return _sceneIndex;  }
    }
    private Text taskTitle;
    private InputField taskNum;

    /***********************
    public int sceneLevel
    {
        get { return _sceneLevel; }
    }
    private int _sceneLevel;
    string[] levelNames;
    *****************************/
	//NetworkManager myNetManager;
	private float timeOut;

	//get acces to control classes for inializing and reseting
	private GameObject lEyeCamGo;
	private GameObject rEyeCamGo;
	private GameObject sphereGo;
	private GameObject targetGo;
    private GameObject dotMotionGo;

	// acess to other scripts: needed to apply menu settings deviating from defualt
	private SetEyeParameters setLEyeParams;
	private SetEyeParameters setREyeParams;
	private SetEyeRotation setLEyeRot;
	private SetEyeRotation setREyeRot;
 	public  RecordEyesPosition recordEyePos { get; set; }


	public InputField sceneDuration { get; set; } //maximal presentation duration of current sceneEntries, thereafter loading of next sceneEntries
	//get acces to controll bottons //
	private Button nextSceneButton;
    private Button newTaskButton;
	//private bool b1stSceneOverride = false;
	public Dropdown sceneTypeControlDD;
	//enable sceneEntries editing mode (switch time out off and save settings to internal settings list either on
	//leaving edit mode or on changing manually to next sceneEntries
	private Toggle editModeToggle;
    private Toggle randomizeToggle; 
	private bool isEditMode;
	private Text remainingTime;
	
	//save or laad all the sceneEntries menu stettings to disk
	private Button saveScenesButton;
	//private Button loadScenesButton;
	private Dropdown loadScenesDD;

	//record eye positions ...
	private Toggle recordToggle;
	private bool isRecording = false;
	//private bool isKeepRecording = false;

	//eye tracking controls
	public Dropdown eyeCamControl{ get; set; }
    public Dropdown stabilisationSource { get; set; }
	public InputField feedbackGain { get; set; }
	public InputField speedLimit { get;  set;}
    public Dropdown projectionMapMode { get; set; }
	public Toggle targetToggle { get; set; } //make visible, whereat head and eyes are looking at

	// the following items are controlling the left
	// and the right eye displays post processsing shaders

	//menu fields for center and peripheral fields
	public InputField centerFieldContrast{ get;  set;}
	public InputField peripheralFieldContrast{ get; set; }

	//eye specific menu items
	public Toggle leftEyeCenterSceneToggle{ get; set; }
	public Toggle leftEyePeripheralSceneToggle{ get;  set;}
	public Dropdown leftEyeMaskTexture{ get; set;}
	public InputField leftEyeDecussationPos{ get; set;}
	public InputField leftEyeCenterScale{ get; set; }
	public InputField leftEyeDisplayContrast{ get; set; }
	public Toggle leftGrayPatchToggle{ get; set;}

	public Toggle rightEyeCenterSceneToggle{ get; set; }
	public Toggle rightEyePeripheralSceneToggle{ get; set; }
	public Dropdown rightEyeMaskTexture{ get; set; }
	public InputField rightEyeDecussationPos{ get; set;}
	public InputField rightEyeCenterScale{ get; set; }
	public InputField rightEyeDisplayContrast{ get; set;}
	public Toggle rightGrayPatchToggle{ get; set; }
	private Toggle eyeMonitorToggle;

	//menu sceneEntries and variables acces for the panorama sphere scenes
	private SetSphereParameters setSphereParams;
	private InputField sphereSpeedField;
	private InputField sphereContrastField;
	public Dropdown spherePanosDropDown{ get; set; }
	private Texture _spherePanoTexture;
	public Texture spherePanoTexture
	{
		get {return _spherePanoTexture;}
	}
	public int panoIndex
	{
		get{ return _panoIndex;}
	}
	public string panoFile
	{
		get {return panoFileInfos[_panoIndex].Name;}
	}

	private TargetControl targetControl;
    //Menu sceneEntries for gaze target scenes
    private GameObject targetPanel;
	public InputField stimRepeats{ get; set; }
	public InputField nearExcentricity{ get; set;}
	public InputField farExcentricity{ get; set; }
	public InputField stField { get; set;}
	public InputField isiField { get; set;}
	public InputField targetScale { get; set; }
	public Dropdown whichEyesControl { get; set; }

    private RotatingRandomDots setRandomDots;
    private GameObject dotMotionPanel;
    //Menu sceneEntries for moving dots' control
    public InputField numOfDots { get; set; }
    public InputField numOfLifeCycles { get; set; }
    public InputField dotRotVel { get; set; }
    public InputField dotDistance { get; set; }
    public InputField bgLuminance { get; set; }



    //in therory we can have more than one player with an oculus rift equipped Win PC  controlled by one admin computer (Mac or Windows) (without Oculus Rift)
    //her we are counting the players
    int playerCount = 0;

    //private bool isRecordingMode = false;

    //Task list
    public TaskList tasks { get; set; }
    //private Task task;
    //private Scene scene; //List with menu settings for all scene Entries for this task
    //private int _taskNum;
	
	public  Scene entries;
	private string filename; 
	private string newContent;
	//get panorama files
	public FileInfo[] panoFileInfos { get; set; }
	private int panoCount;
	private int _panoIndex;
	//get field projection maps
	public FileInfo[] mapFileInfos { get; set;}
	private int mapCount = 0;
	private SceneType _sceneType;
	public FileInfo[] menuFileInfos { get; set; }
	private int menuCount = 0;
	private int _menuIndex;


	void Awake()
	{
		try {
            tasks = new TaskList();
            //aphere menu sceneEntries access here before disenabling access
            sphereGo = GameObject.Find("SkyboxOktahedronSphere");
            setSphereParams = sphereGo.GetComponent<SetSphereParameters>();
            spherePanosDropDown = GameObject.Find("SphereTexture").GetComponent<Dropdown>();
            sphereSpeedField = GameObject.Find("SphereYawSpeed").GetComponentInChildren<InputField>();
            sphereContrastField = GameObject.Find("SphereTextureContrast").GetComponentInChildren<InputField>();
        }
        catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Awake Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log ("Awake Error:" + exp.Message);
			return;
		}
	}


    void Start()
    {
 
        try {
            sphereGo = GameObject.Find("SkyboxOktahedronSphere");
            setSphereParams = sphereGo.GetComponent<SetSphereParameters>();
            spherePanosDropDown = GameObject.Find("SphereTexture").GetComponent<Dropdown>();
            sphereSpeedField = GameObject.Find("SphereYawSpeed").GetComponentInChildren<InputField>();
            sphereContrastField = GameObject.Find("SphereTextureContrast").GetComponentInChildren<InputField>();

            timeOut = 1000000f;
            InitiatePanoList();
            InitProjectionMapFileInfo(); //get list of current projections maps
            InitMenuEntriesFileInfo(); //get list of available scenes menue files located a folder with the apps name within in the apersonal document folder of the admin pc
			//get acces to control
			lEyeCamGo = GameObject.Find ("Camera_LEFT");
			rEyeCamGo = GameObject.Find ("Camera_RIGHT");
			setLEyeParams = lEyeCamGo.GetComponent<SetEyeParameters> ();
			setLEyeRot = lEyeCamGo.GetComponent<SetEyeRotation> ();
			setREyeParams = rEyeCamGo.GetComponent<SetEyeParameters> ();
			setREyeRot = rEyeCamGo.GetComponent<SetEyeRotation> ();

			targetGo = GameObject.Find ("MyTarget");
			targetControl = targetGo.GetComponent<TargetControl>();
			setRandomDots = GameObject.Find("DotMotionEntries").GetComponent<RotatingRandomDots>();
			targetPanel = GameObject.Find("TargetPanel");
			dotMotionPanel = GameObject.Find("DotMotionPanel");
			// due to some quirks on how networked object are spawned, we will not yet find the sphere related script components at the start of a sphere sceneEntries
			// so we will keep the sphere/pano related menu sceneEntries in the sphere object
			// So this menu items will be access by the sphere object directly
			//Here I try it again without old listeners interfering...
            populatePanoDropDown();
 
            if (base.isServer) 
			{
				Canvas canv = GetComponent<Canvas>();
				canv.renderMode = RenderMode.ScreenSpaceOverlay;
				CanvasScaler scaler = canv.GetComponent<CanvasScaler>();
				scaler.referenceResolution = new Vector2(1200f,800f);
				scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;			
			}
            GetMenuAccess();
			AccessTarget();
            AccessSphere();
            AccessDotMotion();
			dotMotionPanel.SetActive(false);
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Start Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log ("Start Error:" + exp.Message);
			return;
		}


	}


	private void GetMenuAccess()
	{
		try
		{
			//before adding new listeners, make sure to remove als old listeners from the previous sceneEntries..
			//establish listener to menu sceneEntries for recording.
			taskNum = GameObject.Find ("TaskNum").GetComponentInChildren<InputField>();
			taskNum.onEndEdit.RemoveAllListeners();
			string val= GetStrVal(taskNum);
			//numOfTasks = int.Parse(val);
			taskNum.onEndEdit.AddListener((value)=>{OnChangeTaskNum(value);});
			taskNum.onEndEdit.Invoke(val);
			recordEyePos = GetComponentInChildren<RecordEyesPosition> ();
			recordToggle = GameObject.Find ("RecordToggle").GetComponent<Toggle> ();
			recordToggle.onValueChanged.RemoveAllListeners();
			recordToggle.onValueChanged.AddListener ((value) => {OnToggleRecordingMode (value);});
			//we will switch on recording only after all everything is prepered in AddLoadedEntries
			sceneTypeControlDD = GameObject.Find ("SceneTypeControl").GetComponent<Dropdown>();
			sceneTypeControlDD.onValueChanged.RemoveAllListeners();
            _sceneType = (SceneType)sceneTypeControlDD.value;  
			sceneTypeControlDD.onValueChanged.AddListener((value) => { OnChangeSceneType(value);});
            //no special init routine should be required since
            //change manually to next sceneEntries
            nextSceneButton = GameObject.Find ("NextSceneButton").GetComponent<Button> ();
			nextSceneButton.onClick.RemoveAllListeners();
			nextSceneButton.onClick.AddListener (() => OnNextSceneClick ());
			//reset image effects to normal before setting menus
			setLEyeParams.OnNewScene();
			setREyeParams.OnNewScene();
            //newTaskButton inserts a tag and resets the current sceneEntries to the menu default values to prepare for randomisation of tasks
            newTaskButton = GameObject.Find("NewTaskButton").GetComponent<Button>();
            newTaskButton.onClick.RemoveAllListeners();
            newTaskButton.onClick.AddListener(() => OnNewTaskClick());
            //projection map mode controlling separation bewtween "central", "peripheral" and "gay patches: head or gaze fixed
            projectionMapMode = GameObject.Find("ProjectionMapMode").GetComponent<Dropdown>();
            projectionMapMode.onValueChanged.RemoveAllListeners();
            setLEyeParams.OnChangeFieldMapMode(projectionMapMode.value);
            setREyeParams.OnChangeFieldMapMode(projectionMapMode.value);
            projectionMapMode.onValueChanged.AddListener((value) => { setLEyeParams.OnChangeFieldMapMode(value); });
            projectionMapMode.onValueChanged.AddListener((value) => { setREyeParams.OnChangeFieldMapMode(value); });
            //establish listener control for setting eye parameters
            stabilisationSource = GameObject.Find("StabilizationSource").GetComponent<Dropdown>();
            stabilisationSource.onValueChanged.RemoveAllListeners();
            setLEyeRot.OnChangeStabilisationSource(stabilisationSource.value);
            setREyeRot.OnChangeStabilisationSource(stabilisationSource.value);
            stabilisationSource.onValueChanged.AddListener((value) => { setLEyeRot.OnChangeStabilisationSource(value); });
            stabilisationSource.onValueChanged.AddListener((value) => { setREyeRot.OnChangeStabilisationSource(value); });
            eyeCamControl = GameObject.Find ("EyeCamControl").GetComponent<Dropdown> ();
			eyeCamControl.onValueChanged.RemoveAllListeners();
			setLEyeRot.OnChangeEyeCamControl(eyeCamControl.value);
			setREyeRot.OnChangeEyeCamControl(eyeCamControl.value);
			eyeCamControl.onValueChanged.AddListener ((value) => {setLEyeRot.OnChangeEyeCamControl (value);});
			eyeCamControl.onValueChanged.AddListener ((value) => {setREyeRot.OnChangeEyeCamControl (value);});
			feedbackGain = GameObject.Find ("FeedbackGain").GetComponentInChildren<InputField> ();
			feedbackGain.onEndEdit.RemoveAllListeners();
			setLEyeRot.OnChangedFeedbackGain(GetStrVal(feedbackGain));
			setREyeRot.OnChangedFeedbackGain(GetStrVal(feedbackGain));
			feedbackGain.onEndEdit.AddListener ((value) => {setLEyeRot.OnChangedFeedbackGain (value);});
			feedbackGain.onEndEdit.AddListener ((value) => {setREyeRot.OnChangedFeedbackGain (value);});
            /********************************************
			speedLimit = GameObject.Find ("SpeedLimit").GetComponentInChildren<InputField> ();
			speedLimit.onEndEdit.RemoveAllListeners();
			setLEyeRot.OnChangeSpeedLimit(GetStrVal(speedLimit));
			setREyeRot.OnChangeSpeedLimit(GetStrVal(speedLimit));
			speedLimit.onEndEdit.AddListener ((value) => {setLEyeRot.OnChangeSpeedLimit (value);});
			speedLimit.onEndEdit.AddListener ((value) => {setREyeRot.OnChangeSpeedLimit (value);});
            ******************************************************/
			targetToggle = GameObject.Find ("TargetToggle").GetComponent<Toggle> ();
			targetToggle.onValueChanged.RemoveAllListeners();
			setLEyeRot.OnChangeTargetToggle(targetToggle.isOn);
			setREyeRot.OnChangeTargetToggle(targetToggle.isOn);
			targetToggle.onValueChanged.AddListener((value) => {setLEyeRot.OnChangeTargetToggle(value);});
			targetToggle.onValueChanged.AddListener ((value) => {setREyeRot.OnChangeTargetToggle(value);});
			//task time
			//GameObject sceneDurationGo = GameObject.Find("SceneDuration");
            //sceneDurationGo.SetActive(true);
            sceneDuration = GameObject.Find("SceneDuration").GetComponentInChildren<InputField>();
            sceneDuration.onEndEdit.RemoveAllListeners();
			sceneDuration.onEndEdit.AddListener ((value) => {OnChangeSceneDuration(value);});

			//establish listener for eye paramters in menu

			centerFieldContrast = GameObject.Find ("CenterFieldContrast").GetComponentInChildren<InputField> ();
			centerFieldContrast.onEndEdit.RemoveAllListeners();
			setLEyeParams.OnChangeCentralContrast(GetStrVal(centerFieldContrast));
			setREyeParams.OnChangeCentralContrast(GetStrVal(centerFieldContrast));
			centerFieldContrast.onEndEdit.AddListener ((value) => {setLEyeParams.OnChangeCentralContrast (value);});
			centerFieldContrast.onEndEdit.AddListener ((value) => {setREyeParams.OnChangeCentralContrast (value);});

			peripheralFieldContrast = GameObject.Find ("PeripheralFieldContrast").GetComponentInChildren<InputField> ();
			peripheralFieldContrast.onEndEdit.RemoveAllListeners();
			setLEyeParams.OnChangePeripheralContrast(GetStrVal(peripheralFieldContrast)); 
			peripheralFieldContrast.onEndEdit.AddListener ((value) => {
				setLEyeParams.OnChangePeripheralContrast (value);});
			peripheralFieldContrast.onEndEdit.AddListener ((value) => {
				setREyeParams.OnChangePeripheralContrast (value);});

			leftEyeCenterSceneToggle = GameObject.Find ("LeftEyeCenterSceneToggle").GetComponent<Toggle> ();
			leftEyeCenterSceneToggle.onValueChanged.RemoveAllListeners();
			setLEyeParams.OnToggleCentralMirror(leftEyeCenterSceneToggle.isOn);
			leftEyeCenterSceneToggle.onValueChanged.AddListener ((value) => {
				setLEyeParams.OnToggleCentralMirror (value);});
			leftEyePeripheralSceneToggle = GameObject.Find ("LeftEyePeripheralSceneToggle").GetComponent<Toggle> ();
			leftEyePeripheralSceneToggle.onValueChanged.RemoveAllListeners();
			setLEyeParams.OnTogglePeripheralMirror(leftEyePeripheralSceneToggle.isOn);
			leftEyePeripheralSceneToggle.onValueChanged.AddListener ((value) => {
				setLEyeParams.OnTogglePeripheralMirror (value);});
			leftEyeDecussationPos = GameObject.Find ("LeftEyeDecussationPos").GetComponentInChildren<InputField> ();
			leftEyeDecussationPos.onEndEdit.RemoveAllListeners();
			setLEyeParams.OnChangeFieldMapMidPointX(GetStrVal( leftEyeDecussationPos));
			leftEyeDecussationPos.onEndEdit.AddListener ((value) => {
				setLEyeParams.OnChangeFieldMapMidPointX (value);});
			leftEyeCenterScale = GameObject.Find ("LeftEyeCenterScale").GetComponentInChildren<InputField> ();
			leftEyeCenterScale.onEndEdit.RemoveAllListeners();
			setLEyeParams.OnChangeFieldMapScale(GetStrVal(leftEyeCenterScale));
			leftEyeCenterScale.onEndEdit.AddListener ((value) => {
				setLEyeParams.OnChangeFieldMapScale (value);});
			leftEyeDisplayContrast = GameObject.Find ("LeftEyeDisplayContrast").GetComponentInChildren<InputField> ();
			leftEyeDisplayContrast.onEndEdit.RemoveAllListeners();
			setLEyeParams.OnChangeContrast(GetStrVal(leftEyeDisplayContrast));
			leftEyeDisplayContrast.onEndEdit.AddListener ((value) => {
				setLEyeParams.OnChangeContrast (value);});
			leftGrayPatchToggle = GameObject.Find ("LeftGrayPatchToggle").GetComponent<Toggle> ();
			leftGrayPatchToggle.onValueChanged.RemoveAllListeners();
			setLEyeParams.OnChangeGrayPatch(leftGrayPatchToggle.isOn); 
			leftGrayPatchToggle.onValueChanged.AddListener ((value) => {
				setLEyeParams.OnChangeGrayPatch (value);});

			rightEyeCenterSceneToggle = GameObject.Find ("RightEyeCenterSceneToggle").GetComponent<Toggle> ();
			rightEyeCenterSceneToggle.onValueChanged.RemoveAllListeners();
			setREyeParams.OnToggleCentralMirror(rightEyeCenterSceneToggle.isOn);
			rightEyeCenterSceneToggle.onValueChanged.AddListener ((value) => {
				setREyeParams.OnToggleCentralMirror (value);});
			rightEyePeripheralSceneToggle = GameObject.Find ("RightEyePeripheralSceneToggle").GetComponent<Toggle> ();
			rightEyePeripheralSceneToggle.onValueChanged.RemoveAllListeners();
			setREyeParams.OnTogglePeripheralMirror(rightEyePeripheralSceneToggle.isOn);
			rightEyePeripheralSceneToggle.onValueChanged.AddListener ((value) => {
				setREyeParams.OnTogglePeripheralMirror (value);});
			rightEyeDecussationPos = GameObject.Find ("RightEyeDecussationPos").GetComponentInChildren<InputField> ();
			rightEyeDecussationPos.onEndEdit.RemoveAllListeners();
			setREyeParams.OnChangeFieldMapMidPointX(GetStrVal(rightEyeDecussationPos));
			rightEyeDecussationPos.onEndEdit.AddListener ((value) => {
				setREyeParams.OnChangeFieldMapMidPointX (value);});
			rightEyeCenterScale = GameObject.Find ("RightEyeCenterScale").GetComponentInChildren<InputField> ();
			rightEyeCenterScale.onEndEdit.RemoveAllListeners();
			setREyeParams.OnChangeFieldMapScale(GetStrVal(rightEyeCenterScale));
			rightEyeCenterScale.onEndEdit.AddListener ((value) => {
				setREyeParams.OnChangeFieldMapScale (value);});
			rightEyeDisplayContrast = GameObject.Find ("RightEyeDisplayContrast").GetComponentInChildren<InputField> ();
			rightEyeDisplayContrast.onEndEdit.RemoveAllListeners();
			setREyeParams.OnChangeContrast(GetStrVal(rightEyeDisplayContrast));
			rightEyeDisplayContrast.onEndEdit.AddListener ((value) => {
				setREyeParams.OnChangeContrast (value);});
			rightGrayPatchToggle = GameObject.Find ("RightGrayPatchToggle").GetComponent<Toggle> ();
			rightGrayPatchToggle.onValueChanged.RemoveAllListeners();
			setREyeParams.OnChangeGrayPatch(rightGrayPatchToggle.isOn);
			rightGrayPatchToggle.onValueChanged.AddListener ((value) => {
				setREyeParams.OnChangeGrayPatch (value);});
		
			leftEyeMaskTexture = GameObject.Find ("LeftEyeMaskTexture").GetComponent<Dropdown> ();
			rightEyeMaskTexture = GameObject.Find ("RightEyeMaskTexture").GetComponent<Dropdown> ();
			//access to left and right eye msak texture has to be assured before calling this special
			//subrroutine
			PopulateProjectionMapDropDowns ();
			//now the mask texture drop down items have been set up and we can add the listener
			leftEyeMaskTexture.onValueChanged.RemoveAllListeners();
			setLEyeParams.OnChangeFieldMap(leftEyeMaskTexture.value);
			leftEyeMaskTexture.onValueChanged.AddListener ((value) => {
				setLEyeParams.OnChangeFieldMap (value);});

			rightEyeMaskTexture.onValueChanged.RemoveAllListeners();
			setREyeParams.OnChangeFieldMap(rightEyeMaskTexture.value); 
			rightEyeMaskTexture.onValueChanged.AddListener ((value) => {
				setREyeParams.OnChangeFieldMap (value);});

			editModeToggle = GameObject.Find ("EditModeToggle").GetComponent<Toggle> ();
			editModeToggle.onValueChanged.RemoveAllListeners();
            isEditMode = editModeToggle.isOn;
            //editMode toggle should be set on start
			editModeToggle.onValueChanged.AddListener ((value) => {
				OnToggleEditMode (value); });
            //enabling/dienabling rnadomisation of all tasks except starting task 0
            randomizeToggle = GameObject.Find("RandomizeToggle").GetComponent<Toggle>();
            randomizeToggle.onValueChanged.RemoveAllListeners();
            randomizeToggle.onValueChanged.AddListener((value) => { OnToggleRandomizeTasks(value); });
			//saving/loading menu settings 
			saveScenesButton = GameObject.Find ("SaveScenesButton").GetComponent<Button> ();
			saveScenesButton.onClick.RemoveAllListeners();
			saveScenesButton.onClick.AddListener (() => OnSaveMenuEntries ());
			
			loadScenesDD = GameObject.Find ("LoadScenesMenues").GetComponent<Dropdown> ();
			PopulateMenuFileDropDowns();
			loadScenesDD.onValueChanged.RemoveAllListeners();
            if (tasks.Count > 0)
                loadScenesDD.value = _menuIndex;
			//if (entries.sceneEntries.Count > 0)
			//	loadScenesDD.value = _menuIndex;
			loadScenesDD.onValueChanged.AddListener((value) => {OnChangeLoadScenesDD(value); });
			//label to display remaining sceneEntries time
			remainingTime = GameObject.Find ("RemainingTime").GetComponent<Text> ();
            //all the dynmic loaaded texture are still missing on the client (why???) but can be made available now
            //toggle to enable SMI eye monitors
            eyeMonitorToggle = GameObject.Find ("EyeMonitorToggle").GetComponent<Toggle>();
			eyeMonitorToggle.onValueChanged.AddListener((value) => {OnEnableEyeMonitors(value); });
            //display sceneEntries number
            taskTitle = GameObject.Find("TaskTitle").GetComponent<Text>();
            OnSetTaskTitle();
            if (!isServer)
				Invoke ("InitMaps", 0.03f);

			//ApplyLoadedSceneEntries();
			if (isServer)
            {
                sceneTypeControlDD.onValueChanged.Invoke((int)_sceneType);
 				if (!(tasks.Count > 0))
				{
					//invoke only simple subroutine without other sub routines inside. 
					//Some minimal time is required to get the new menus content and not the previous ones
					Invoke("InitEntries", 0.1f);
					Invoke ("InitEyeParams",0.02f);
					Invoke ("InitEyeRot", 0.03f); 
					if (_sceneType.Equals(SceneType.Sphere))
						Invoke("InitSphere", 0.04f);
					else if (_sceneType.Equals(SceneType.Target))
						Invoke ("InitTarget", 0.04f);
				} else {
						Invoke ("ApplyLoadedSceneEntries", 0.05f);
				}
			}

			//now, as everything ist again neatly set up ans connected, we are ready to continue recording, if recording is on
			//if (isRecording)
			//	recordEyePos.StartRecording ();
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("GetMenuAccess Nullrefexp:" + zeroExp);
			return;
		}
		catch(Exception exp)
		{
			Debug.Log ("GetMenuAccess Error!"  + exp.Message);
		}
	}
	//should be called from SetSphereParameters Start()

	public void AccessSphere()
	{
		try {
            spherePanosDropDown.onValueChanged.RemoveAllListeners();
            OnChangePano(spherePanosDropDown.value); //set pano texture for current sceneEntries level
			spherePanosDropDown.onValueChanged.AddListener ((value) => {
				OnChangePano (value);});
			//OnChangePano(spherePanosDropDown.value); //set pano texture for current sceneEntries level

			sphereSpeedField.onEndEdit.RemoveAllListeners ();
			setSphereParams.OnChangeSpeed(GetStrVal(sphereSpeedField));
			sphereSpeedField.onEndEdit.AddListener ((value) => {
				setSphereParams.OnChangeSpeed (value);});
			setSphereParams.OnChangeSpeed(GetStrVal(sphereSpeedField));
			sphereContrastField.onEndEdit.RemoveAllListeners ();
			setSphereParams.OnChangeContrast(GetStrVal(sphereContrastField));
			sphereContrastField.onEndEdit.AddListener ((value) => {
				setSphereParams.OnChangeContrast (value);});
			setSphereParams.OnChangeContrast(GetStrVal(sphereContrastField));

		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Access Sphere Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log ("Access Sphere Error:" + exp.Message);
			return;
		}

	}
	//should be called from TargetControl Start()
	public void AccessTarget()
	{
		try {	
			stimRepeats = GameObject.Find ("StimRepeats").GetComponentInChildren<InputField>();
			stimRepeats.onEndEdit.RemoveAllListeners();
			targetControl.OnChangeStimRepeats(GetStrVal(stimRepeats));
			stimRepeats.onEndEdit.AddListener((value) => { targetControl.OnChangeStimRepeats(value); });
			nearExcentricity = GameObject.Find ("NearExcentricity").GetComponentInChildren<InputField> ();
			nearExcentricity.onEndEdit.RemoveAllListeners();
			targetControl.OnChangeNearExcentricity (GetStrVal (nearExcentricity));
			nearExcentricity.onEndEdit.AddListener ((value) => {
				targetControl.OnChangeNearExcentricity (value);});
			farExcentricity = GameObject.Find ("FarExcentricity").GetComponentInChildren<InputField> ();
			farExcentricity.onEndEdit.RemoveAllListeners();
			targetControl.OnChangeFarExcentricity (GetStrVal (farExcentricity));
			farExcentricity.onEndEdit.AddListener ((value) => {
				targetControl.OnChangeFarExcentricity (value);});
			stField = GameObject.Find ("StimulusTime").GetComponentInChildren<InputField> ();
			stField.onEndEdit.RemoveAllListeners();
			targetControl.OnChangeST (GetStrVal (stField));
			stField.onEndEdit.AddListener ((value) => {
				targetControl.OnChangeST (value);});
			isiField = GameObject.Find ("ISI").GetComponentInChildren<InputField> ();
			isiField.onEndEdit.RemoveAllListeners();
			targetControl.OnChangeISI (GetStrVal (isiField));
			isiField.onEndEdit.AddListener ((value) => {
				targetControl.OnChangeISI (value);});
			targetScale = GameObject.Find ("TargetScale").GetComponentInChildren<InputField>();
			targetScale.onEndEdit.RemoveAllListeners();
			targetControl.OnChangeTargetScale(GetStrVal (targetScale));
			targetScale.onEndEdit.AddListener((value) => {
				targetControl.OnChangeTargetScale(value);});
			whichEyesControl = GameObject.Find ("WhichEyesControl").GetComponent<Dropdown>();
			whichEyesControl.onValueChanged.RemoveAllListeners();
			//note Camera_LEFT culling mask is set to VieLeftEye, Spherical Skybox; Camera_RIGHT to ViewRightEye, SphericalSkybox
			whichEyesControl.onValueChanged.AddListener((value) => {targetControl.OnChangeWhichEyesControl(value);}); //changes target's layer


		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("AccessTarget Nullrefexp:" + zeroExp);
			return;
		}

		catch (Exception exp)
		{
			Debug.Log ("Access Target Error:" + exp.Message);
			return;
		}
	}

    public void AccessDotMotion()
    {
        try
        {
            numOfDots = GameObject.Find("NumOfDots").GetComponentInChildren<InputField>();
            numOfDots.onEndEdit.AddListener((value) => { setRandomDots.OnChangeNumOfDots(value); });
            numOfLifeCycles = GameObject.Find("NumOfLifeCycles").GetComponentInChildren<InputField>();
            numOfLifeCycles.onEndEdit.AddListener((value) => { setRandomDots.OnChangeNumOfLifeCyles(value); });
            dotRotVel = GameObject.Find("DotRotVel").GetComponentInChildren<InputField>();
            dotRotVel.onEndEdit.AddListener((value) => { setRandomDots.OnChangeDotRotVel(value); });
            dotDistance = GameObject.Find("DotDistance").GetComponentInChildren<InputField>();
            dotDistance.onEndEdit.AddListener((value) => { setRandomDots.OnChangeDotDistance(value); });
			bgLuminance = GameObject.Find("BgLuminance").GetComponentInChildren<InputField>();
            bgLuminance.onEndEdit.AddListener((value) => { setRandomDots.OnChangeBgLuminance(value); });

        }
        catch (NullReferenceException zeroExp)
        {
            Debug.Log("AccessDotMotion Nullrefexp:" + zeroExp);
            return;
        }

        catch (Exception exp)
        {
            Debug.Log("AccessDotMotion Error:" + exp.Message);
            return;
        }
    }
	public void InitEntries()
	{
		taskNum.onEndEdit.Invoke (GetStrVal (taskNum));
	}

    public void InitMaps()
	{
		Debug.Log ("InitMaps");
		spherePanosDropDown.onValueChanged.Invoke(spherePanosDropDown.value);
		leftEyeMaskTexture.onValueChanged.Invoke (leftEyeMaskTexture.value);
		rightEyeMaskTexture.onValueChanged.Invoke (rightEyeMaskTexture.value);
    }
    public void InitTarget()
	{
		nearExcentricity.onEndEdit.Invoke(GetStrVal (nearExcentricity));
		farExcentricity.onEndEdit.Invoke(GetStrVal (farExcentricity));
		stField.onEndEdit.Invoke (GetStrVal (stField));
		isiField.onEndEdit.Invoke (GetStrVal (isiField));
	}
	public void InitSphere()
	{
		sphereSpeedField.onEndEdit.Invoke (GetStrVal (sphereSpeedField));
		sphereContrastField.onEndEdit.Invoke (GetStrVal (sphereContrastField));
	}

	private void InitEyeRot()
	{
		try
		{
			eyeCamControl.onValueChanged.Invoke(eyeCamControl.value);
			feedbackGain.onEndEdit.Invoke(GetStrVal (feedbackGain));
			speedLimit.onEndEdit.Invoke(GetStrVal (speedLimit));
			targetToggle.onValueChanged.Invoke(targetToggle.isOn);
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("InitEyeRot Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log ("InitEyeRot Target Error:" + exp.Message);
			return;
		}
	}
	//Initialize eye post shader control with current menu values
	private void InitEyeParams()
	{	
		try {
			//reset params
			//reset image effects to normal before setting menus
			setLEyeParams.OnNewScene();
			setREyeParams.OnNewScene();

			centerFieldContrast.onEndEdit.Invoke(GetStrVal(centerFieldContrast));
			peripheralFieldContrast.onEndEdit.Invoke(GetStrVal (peripheralFieldContrast));
			leftEyeCenterSceneToggle.onValueChanged.Invoke(leftEyeCenterSceneToggle.isOn);
			leftEyePeripheralSceneToggle.onValueChanged.Invoke(leftEyePeripheralSceneToggle.isOn);
			leftEyeMaskTexture.onValueChanged.Invoke(leftEyeMaskTexture.value);
			leftEyeDecussationPos.onEndEdit.Invoke(GetStrVal (leftEyeDecussationPos));
			leftEyeCenterScale.onEndEdit.Invoke(GetStrVal (leftEyeCenterScale));
			leftEyeDisplayContrast.onEndEdit.Invoke(GetStrVal (leftEyeDisplayContrast)); 
			leftGrayPatchToggle.onValueChanged.Invoke(leftGrayPatchToggle.isOn); 
			rightEyeCenterSceneToggle.onValueChanged.Invoke(rightEyeCenterSceneToggle.isOn);
			rightEyePeripheralSceneToggle.onValueChanged.Invoke(rightEyePeripheralSceneToggle.isOn);
			rightEyeMaskTexture.onValueChanged.Invoke(rightEyeMaskTexture.value);
			rightEyeDecussationPos.onEndEdit.Invoke(GetStrVal (rightEyeDecussationPos));
			rightEyeCenterScale.onEndEdit.Invoke(GetStrVal (rightEyeCenterScale));
			rightEyeDisplayContrast.onEndEdit.Invoke(GetStrVal (rightEyeDisplayContrast)); 
			rightGrayPatchToggle.onValueChanged.Invoke(rightGrayPatchToggle.isOn);
            _sceneType = (SceneType)sceneTypeControlDD.value;
            sceneTypeControlDD.onValueChanged.Invoke((int)_sceneType);
        }
        catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Init EyeParams Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log (" InitEyeParams Error:" + exp.Message);
			return;
		}
	}


	// get (string) value out of an InputField, if there is none, return default value stored in place holder
	private string GetStrVal(InputField field)
	{
		if (field.text.Length > 0)
			return field.text;
		else
			return field.placeholder.GetComponent<Text> ().text;
	}
	private float GetFloatVal(InputField field)
	{
		try
		{
			string strVal = GetStrVal(field);
			float val = float.Parse(strVal);
			return val;
		}
		catch (System.FormatException fe)
		{
			Debug.Log(fe);
			return -999f;
		}
		catch (System.Exception exp)
		{
			Debug.LogException (exp);
			return -999f;
		}

	}

	//look for available panorama picure files in the folder Panos in the users Picture folder on Mac or Window PC
	private void InitiatePanoList()
	{
		try {
			string user;
			user = System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyPictures);
			#if UNITY_STANDALONE_OSX
			string panoDir = user + @"/Panos";
			#else
			string panoDir = user + @"\Panos";
			#endif
			Debug.Log (panoDir);
			DirectoryInfo dirInfo = new DirectoryInfo (panoDir);
			panoFileInfos = dirInfo.GetFiles ("*.???");
			//fileInfos = dirInfo.GetFiles("*.jpg"); does not find *.JPG on MAC!
			panoCount = panoFileInfos.Length;
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Initiate Panos Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log (" InitiatePanos Error:" + exp.Message);
			return;
		}
	}
	// show available pano picture file names
	private void populatePanoDropDown()
	{	
		try {
			spherePanosDropDown = GameObject.Find ("SphereTexture").GetComponent<Dropdown> ();
			//Fill ip option list
			spherePanosDropDown.options.Clear();
			for (int i = 0; i < panoCount; i++)
			{
				Dropdown.OptionData item = new Dropdown.OptionData(panoFileInfos[i].Name);
				spherePanosDropDown.options.Add(item);
			}
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("PopulatePanos Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log ("PopulatePanos Error:" + exp.Message);
			return;
		}

	}

    [ServerCallback]
    public void OnSetTaskTitle()
    {
        int itask = tasks.taskIndex;
        int idtask = tasks.taskId;
        int iscene = tasks.GetSceneIndex();
        taskTitle.text = "SplitSphere - " + itask.ToString() + ':' + idtask.ToString() + "." + iscene.ToString();
        RpcOnSetTaskTitle(itask, idtask, iscene);
    }
    [ClientRpc]
    public void RpcOnSetTaskTitle(int itask, int idtask, int iscene)
    {
        Debug.Log("RpcOnSetTitle:" + itask.ToString() + "-" + idtask.ToString() + "_" + iscene.ToString());
        _taskIndex = itask;
        _taskId = idtask;
        _sceneIndex = iscene;
    }

    //network calls and relatated subroutines
    [ServerCallback]
	public void OnChangeTaskNum(string val)
	{
		try
		{
			AddMenuEntry("TaskNum", val);
			int num = int.Parse(val);
			RpcOnChangeTaskNum(num);
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
	public void RpcOnChangeTaskNum(int num)
	{
        if (num > tasks.maxTaskNum)
            tasks.bEditing = true; //we want to add new task, therefore we need to reenable tasks/scenes editing
        tasks.maxTaskNum = num;
	}

	[ServerCallback] //this will only be performed on server
	public void OnChangePano(int index)
	{
		AddMenuEntry ("SphereTexture", index.ToString ());
		//int level = panoDropDown.GetComponent<Dropdown>().value;
		RpcChangePanoOnClient(index); //host changed skybox also on its own client
	}
	
	[ClientRpc] //to be used, if the server gives commands for the clients. Use [CMD] and cmdDoAction() if clients give commands like fireing bullets 
	public void RpcChangePanoOnClient(int index)
	{
		Debug.Log("Change pano on Client " + index);
		//scenePanoTextures[_sceneIndex] = ChangePano(index);
		_spherePanoTexture = ChangePano (index);
	}
	
	private Texture ChangePano(int level) //should be called when OnPanoSelected event occurs/is rised
	{
		try {
		    //Debug.Log(level);
		    Texture2D tex = null;
		    byte[] fileData;
		    //menuControl = GameObject.Find ("MenuCanvas").GetComponent<MenuCanvasControl> ();
		    FileInfo info = panoFileInfos [level];
		    string path = info.FullName;
		    Debug.Log("Level:" + level.ToString() + " " + path);
		    if (File.Exists(path))
		    {
			    //get rid of old texture to free up unused memory by using an asynchronous funtion thread
			    AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
			    if (unloadOp.isDone == true)
				    Debug.Log("Unloading unused ressources finished");
			    //fill panorama texture file into a dynamic 2D-texture
			    fileData = File.ReadAllBytes(path);
			    Debug.Log("Read panorama texture file " + path);
			    tex = new Texture2D(2, 2);
			    tex.LoadImage(fileData);
			    //sphereGo = GameObject.Find ("SkyboxOktahedronSphere");
				Renderer sphereRend = sphereGo.GetComponent<Renderer>();
			    //GetComponent<Renderer>().material.mainTexture = tex;->to by applyed to sphere
			    //sphereGo.GetComponent<Renderer>().material.mainTexture = tex; //returns a material of null
				sphereRend.material.mainTexture = tex;
			    Debug.Log("got Texture");
			    _panoIndex = level;
			    return tex;
		    }
		    else return null;
		}
        catch(IndexOutOfRangeException iExp)
        {
            Debug.Log("Panos IndexOutOfRange" + iExp.Message);
            return null;
        }
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Change Nullrefexp:" + zeroExp);
			return null;
		}
		catch (Exception exp)
		{
			Debug.Log ("Change Pano Error:" + exp.Message);
			return null;
		}

	}
	//assynchronous function to free up unused memory
	IEnumerator Unloading()
	{
		AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
		yield return unloadOp;
		Debug.Log("Unloading unused ressources finished");
		
	}

	//gets a list of all projection maps to be called once at the start of this program module and accessed by ths sphere object if needed
	private void InitProjectionMapFileInfo()
	{
		string user;
		user = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
		#if UNITY_STANDALONE_OSX
		string mapDir = user + @"/ProjectionMaps";
		#else
		string mapDir = user + @"\ProjectionMaps";
		#endif
		Debug.Log(mapDir);
		DirectoryInfo dirInfo = new DirectoryInfo(mapDir);
		mapFileInfos = dirInfo.GetFiles("*.???");  //png ist the better format for masks
		mapCount = mapFileInfos.Length;
		
		Debug.Log("BorderMap Count: " + mapCount.ToString());
	}
	//populates the projection map dropdown lists: to be called at the start of a each new sceneEntries even when used only in some of the scenes with a panorama..
	private void PopulateProjectionMapDropDowns()
	{
		try
		{
			//Fill ip option list
			leftEyeMaskTexture.options.Clear();
			rightEyeMaskTexture.options.Clear ();
			for (int i = 0; i < mapCount; i++)
			{
				Dropdown.OptionData item = new Dropdown.OptionData(mapFileInfos[i].Name);
				leftEyeMaskTexture.options.Add(item);
				rightEyeMaskTexture.options.Add(item);
			}
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("Populate Projection Maps Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log (" PopulateProjectionMaps Error:" + exp.Message);
			return;
		}
	}
	//gets a list of all files with menu sceneEntries 
	private void InitMenuEntriesFileInfo()
	{
		try {
			string user;
			#if UNITY_STANDALONE_OSX
			user = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			string mnuDir = user + @"/" + Application.productName;
			#else
			user = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			string mnuDir = user + @"\" + Application.productName;
#endif
            if (isServer)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(mnuDir);
                menuFileInfos = dirInfo.GetFiles("*.txt");  //its only a text file
                menuCount = mapFileInfos.Length;
                Debug.Log("Menu Files Count: " + menuCount.ToString());
            }
			
			
		}
		catch (IOException ioExp)
		{
			Debug.Log ("IO Exption in InitMenuFileInfo " + ioExp.Message); 
		}
		catch (Exception exp)
		{
			Debug.Log ("InitMenuFileInfo Error:" + exp.Message);
			return;
		}

	}

	private void PopulateMenuFileDropDowns()
	{
		try
		{
			loadScenesDD.options.Clear ();
            if (isServer)
            {
                for (int i = 0; i < menuFileInfos.Length; i++)
                {
                    Dropdown.OptionData item = new Dropdown.OptionData(menuFileInfos[i].Name);
                    loadScenesDD.options.Add(item);
                }
            }
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("PopulateMenuFileDropDowns Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log ("PopulateMenuFileDropDowns Error:" + exp.Message);
			return;
		}
	}



	
	// Update is called once per frame
	void Update () {
		//check if new new sceneEntries has been loaded, if yes, start coroutine
		//for sceneEntries timeout
		//Debug.Log (Time.deltaTime.ToString ());
		if (base.isServer) {
            if (Input.anyKeyDown)
            {
                //start recording on pressing Right Shift Key on ADMIN app (space is used by calibration
                if (Input.GetKeyDown(KeyCode.RightShift))
                {
                    if (recordToggle.isOn)
                        recordToggle.isOn = false;
                    else
                        recordToggle.isOn = true;

                }
            }
            SceneTimeOut();
        }
    }

	//should be called when changing a sceneEntries
	//called only on the server
	//
	void OnPlayerConnected(NetworkPlayer player)
	{
		playerCount++;
		Debug.Log("Player connected:" + playerCount.ToString());
		//timeOut = 5f;
	
	}
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}

	//Timer coroutine does not function here
	private void SceneTimeOut()
	{
        //Debug.Log (timeOut.ToString() + "Scene:" + Application.loadedLevel.ToString());

        //if (!isEditMode && ((_sceneIndex > 1) || b1stSceneOverride)) //no timeout on starting sceneEntries with the exception of havving pressed the "Next Scene" button nor in edit mode		if (!isEditMode && ((_sceneIndex > 1) || b1stSceneOverride)) //no timeout on starting sceneEntries with the exception of havving pressed the "Next Scene" button nor in edit mode
        if (!isEditMode) //no timeout in edit mode
                timeOut -= Time.deltaTime;
		if (timeOut < Time.fixedDeltaTime) {
			ChangeToNextScene ();
		} else {
			if (remainingTime != null) {
				remainingTime.text = "Time left: " + timeOut.ToString ("F3");
			}
		}
	}
	[ServerCallback]
	private void ChangeToNextScene()
	{
		try {
			RpcOnNextScene();
			//myNetManager.ServerChangeScene(levelNames[_sceneIndex]);
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("ChangeToNextScene Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log (" ChangeToNextScene Error:" + exp.Message);
			return;
		}

	}
	[ClientRpc]
	public void RpcOnNextScene()
	{
		timeOut = float.Parse (GetStrVal (sceneDuration)); //set timeout to sceneEntries duration
		if (!isServer)
		{
			if (isRecording)
				recordEyePos.StopRecording();
		}
         //Scene scene = tasks.NextScene();
        tasks.NextScene();
        OnSetTaskTitle();
        ApplyLoadedSceneEntries();
		int val = sceneTypeControlDD.value;
		sceneTypeControlDD.onValueChanged.Invoke (val);
        if (isRecording)
        {
            if (!isServer)
                StartRecording(0.05f); //delay of 50 ms to allow for task menu changes to be over
        }

	}
	[ServerCallback]
	public void OnChangeSceneType(int index)
	{
		AddMenuEntry ("SceneTypeControl", index.ToString ());
		//targetControl = targetGo.GetComponent<TargetControl> ();
		RpcOnChangeSceneType (index);
		switch (index) {
		    case 0: //panorama only
                targetPanel.SetActive(true);
                dotMotionPanel.SetActive(false);
			    setSphereParams.OnChangeVisability (true);
			    targetControl.OnChangeVisability (false);
                setRandomDots.OnChangeVisability(false);
			    break;
		    case 1: //fixation dot only
                targetPanel.SetActive(true);
                dotMotionPanel.SetActive(false);
                setSphereParams.OnChangeVisability (false);
			    targetControl.OnChangeVisability (true);
                setRandomDots.OnChangeVisability(false);
                break;
		    case 2: //panorama and fixation dot
                targetPanel.SetActive(true);
                dotMotionPanel.SetActive(false);
                setSphereParams.OnChangeVisability (true);
    			targetControl.OnChangeVisability (true);
                setRandomDots.OnChangeVisability(false);
                break;
		    case 3: //neither panorama nor any dots -> darkness
                targetPanel.SetActive(true);
                dotMotionPanel.SetActive(false);
                setSphereParams.OnChangeVisability(false);
			    targetControl.OnChangeVisability(false);
                setRandomDots.OnChangeVisability(false);
                break;
            case 4: //neiter panorama nor fixation tagets, but moving dots
                targetPanel.SetActive(false);
                dotMotionPanel.SetActive(true);
                setSphereParams.OnChangeVisability(false);
                targetControl.OnChangeVisability(false);
                setRandomDots.OnChangeVisability(true);
                break;
		    default:
			    break;
		}
	}

	[ClientRpc]
	public void RpcOnChangeSceneType(int index)
	{
		switch (index) {
    		case 0:
    			_sceneType = SceneType.Sphere;
    			break;
    		case 1:
    			_sceneType = SceneType.Target;
    			break;
	    	case 2:
			    _sceneType = SceneType.Both;
    			break;
            case 3:
                _sceneType = SceneType.Dots;
                break;
		default:
			break;

		}
	}

	[ServerCallback]
	public void OnChangeSceneDuration(string strDuration)
	{
		AddMenuEntry ("SceneDuration", strDuration);
		try
		{
			float duration = float.Parse(strDuration);
			RpcOnChangeSceneDuration(duration);
			if (!_sceneType.Equals(SceneType.Sphere))
				targetControl.OnChangeVisability (true); //reset fixation point settings to their start settings
			//timeOut = duration; // give yourself more time
		}
		catch (System.FormatException fe)
		{
			Debug.Log("OnChangeSceneDuration Parse Error:" + fe.Message);
			return;
		}
		catch (System.Exception exp)
		{
			Debug.LogException (exp);
			return;
		}
	}

	[ClientRpc]
	public void RpcOnChangeSceneDuration(float duration)
	{
		Debug.Log ("RpcOnChangeSceneDuration: " + duration.ToString ());
        float curTime = timeOut;
		timeOut = duration; // give yourself more time
		if (!isServer) {
            if (curTime > 200f)
            {
                //restart recording only if there was some time spent with the previous time setting
                if (isRecording)
                    RestartRecording();
            }
		}
	}

	//used for next sceneEntries button only
	[ServerCallback]
	public void OnNextSceneClick()
	{
		Debug.Log ("OnNextSceneClick");
		RpcOnNextSceneClick ();
        if (!_sceneType.Equals(SceneType.Sphere))
            targetControl.OnChangeVisability(true); //reset fixation point settings zo their start
    }
    [ClientRpc]
	public void RpcOnNextSceneClick()
	{
		Debug.Log ("RpcOnNextSceneClick");
		//b1stSceneOverride = true;
		timeOut = 0f; //do  sceneEntries change using "UpDate() with its timeOut method to avoid ringing
	}
	IEnumerator DeRinging()
	{
		yield return new WaitForSeconds (0.5f);
	}

    [ServerCallback]
    public void OnNewTaskClick()
    {
        Debug.Log("OnNewTaskClick");
        RpcOnNewTaskClick();
    }

    [ClientRpc]
    public void RpcOnNewTaskClick()
    {
        Debug.Log("RpcOnNewTaskClick");
        tasks.NextTask();
        OnSetTaskTitle();
    }

    [ServerCallback]
	public void OnToggleEditMode(bool isOn)
	{
		Debug.Log ("OnToggleEditMode:" + isOn.ToString ());
		RpcOnToggleEditMode (isOn);
		if (!_sceneType.Equals(SceneType.Sphere))
			targetControl.OnChangeVisability (true); //reseet fixation point settings zo their start

	}
	[ClientRpc]
	public void RpcOnToggleEditMode(bool isOn)
	{
		try {
			Debug.Log("RpcOnToggleEditMode: " + isOn.ToString());
			isEditMode = isOn;
			if (!isOn) {
				//reset things
				timeOut = float.Parse (sceneDuration.text); //set timeout to sceneEntries duration
				if (!isServer)
				{
					if (isRecording)
						RestartRecording();
				}
			}
		}
		catch (NullReferenceException zeroExp)
		{
			Debug.Log ("OnSaveMenuEntries Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log ("OnSaveMenuEntries Error:" + exp.Message);
			return;
		}
	}
	[ServerCallback]
	public void OnToggleRecordingMode(bool isOn)
	{
		AddMenuEntry ("RecordToggle", isOn.ToString ());
		Debug.Log ("OnToggleRecordingMode:" + isOn.ToString ());
		RpcOnToggleRecordingMode (isOn);
	}

	[ClientRpc]
	public void RpcOnToggleRecordingMode(bool isOn)
	{
		Debug.Log("RpcOnToggleRecordingMode: " + isOn.ToString());
		if (isOn) {
			recordEyePos.StartRecording();
			isRecording = true;
		} else {
			recordEyePos.StopRecording ();
			isRecording = false;
		}
	}

    private IEnumerator RestartRecording()
    {
        recordEyePos.StopRecording();
        yield return new WaitForSeconds(0.2f);
        recordEyePos.StartRecording();
        isRecording = true;
    }
    private IEnumerator StartRecording(float delay)
    {
        yield return new WaitForSeconds(delay);
        recordEyePos.StartRecording();
        isRecording = true;
    }

    [ServerCallback]
    public void OnToggleRandomizeTasks(bool isOn)
    {
        AddMenuEntry("RandomizeToggle", isOn.ToString());
        Debug.Log("RandomizeToggle: " + isOn.ToString());
        RpcOnToggleRandomizeTasks(isOn);
    }

    [ClientRpc]
    public void RpcOnToggleRandomizeTasks(bool isOn)
    {
        Debug.Log("RpcRandomizeToggle: " + isOn.ToString());
        tasks.bRandomize = isOn;
     }

    [ServerCallback]
	public void OnSaveMenuEntries()
	{
		RpcOnSaveMenuEntries ();
	}
	[ClientRpc]
	public void RpcOnSaveMenuEntries()
	{
		Debug.Log ("RpcOnSaveEdits");
		try
		{
			Debug.Log ("OnSaveEdits");
#if UNITY_STANDALONE_OSX
		    string path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments) + @"/" + Application.productName;
		    if (!Directory.Exists (path))
			    Directory.CreateDirectory (path);
		    string menuFilename = path + @"/" + "SphereMenu" + DateTime.Now.ToString ("yyyyMMdd_HHmmss") + ".txt";
#else
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + @"\" + Application.productName;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string menuFilename = path + @"\SphereMenu" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
#endif
            Debug.Log("Filename: " + menuFilename);

            tasks.StoreTasks(menuFilename);

        }
        catch (NullReferenceException zeroExp)
		{
			Debug.Log ("OnSaveMenuEntries Nullrefexp:" + zeroExp);
			return;
		}
		catch (Exception exp)
		{
			Debug.Log ("OnSaveMenuEntries Error:" + exp.Message);
			return;
		}
		
	}
	// routine to load a sceneEntries menu file
	[ServerCallback]
	public void OnChangeLoadScenesDD(int index)
	{
		Debug.Log ("OnLoadSceneClick");
		RpcOnChangeScenesDD (index);

	}
	[ClientRpc]
	public void RpcOnChangeScenesDD(int index)
	{
		Debug.Log ("RpcOnLoadSceneClick" + index.ToString());
		if (index < menuCount) {
            tasks.LoadTasks(menuFileInfos[index].FullName);
			_menuIndex = index;
			ApplyLoadedSceneEntries();
		}
		else
			Debug.Log ("Did not find menu entry files");
	}
	[ServerCallback]
	public void OnEnableEyeMonitors(bool enable)
	{
		Debug.Log ("Enable SMI eye monitors: " + enable.ToString ());
		RpcOnEnableEyeMonitors (enable);
	}
	[ClientRpc]
	public void RpcOnEnableEyeMonitors(bool enable)
	{
		Debug.Log ("RPC Enable SMI eye monitors: " + enable.ToString ());

		if (enable) {
			SMIGazeController.SMIcWrapper.smi_showEyeImageMonitor ();			
		} else {
			SMIGazeController.SMIcWrapper.smi_hideEyeImageMonitor();
		}
		//there is no hide....
	}
	public void AddMenuEntry(string varName, string val)
	{
        Task task = tasks.GetTask();
        Scene scene = task.GetScene();
        SceneEntry item = new SceneEntry (varName, val, Time.timeSinceLevelLoad);
        scene.Add(item);
	}

	private bool ApplyLoadedEntry(string varName, string val)
	{
		try {
			bool found = false;
			switch(varName)
			{
			case "TaskNum": //make only sense in first sceneEntries...
				found = true;
				taskNum.text = val;
				taskNum.onEndEdit.Invoke(val);
                tasks.maxTaskNum = int.Parse(taskNum.text);
				break;
			case "SceneDuration":
				found = true;
				sceneDuration.text = val;
				sceneDuration.onEndEdit.Invoke(val);
				break;
			case "SceneTypeControl":
				found = true;
				sceneTypeControlDD.value = int.Parse(val);
				sceneTypeControlDD.onValueChanged.Invoke(int.Parse(val));
				break;
			case "EyeCamControl":
				found = true;
				eyeCamControl.value = int.Parse(val);
				eyeCamControl.onValueChanged.Invoke(int.Parse(val));
				break;
            case "StabilisationSource":
                found = true;
                stabilisationSource.value = int.Parse(val);
                stabilisationSource.onValueChanged.Invoke(int.Parse(val));
                break;
            case "ProjectionMapMode":
                 found = true;
                 projectionMapMode.value = int.Parse(val);
                 projectionMapMode.onValueChanged.Invoke(int.Parse(val));
                 break;
            case "FeedbackGain":
				feedbackGain.text = val;
				feedbackGain.onEndEdit.Invoke(val);
				break;
			case "SpeedLimit":
				found = true;
				speedLimit.text = val;
				speedLimit.onEndEdit.Invoke(val);
				break;
			case "TargetToggle":
				found = true;
				targetToggle.isOn = bool.Parse(val);
				targetToggle.onValueChanged.Invoke(bool.Parse(val));
				break;
			case "CenterFieldContrast":
				found = true;
				centerFieldContrast.text = val;
				centerFieldContrast.onEndEdit.Invoke(val);
				break;
			case "PeripheralFieldContrast":
				found = true;
				peripheralFieldContrast.text = val;
				peripheralFieldContrast.onEndEdit.Invoke(val);
				break;
			case "LeftEyeCenterSceneToggle":
				found = true;
				leftEyeCenterSceneToggle.isOn = bool.Parse(val);
				leftEyeCenterSceneToggle.onValueChanged.Invoke(bool.Parse(val));
				break;
			case "LeftEyePeripheralSceneToggle":
				found = true;
				leftEyePeripheralSceneToggle.isOn = bool.Parse(val);
				leftEyePeripheralSceneToggle.onValueChanged.Invoke(bool.Parse(val));
				break;
			case "LeftEyeMaskTexture":
				found = true;
				leftEyeMaskTexture.value = int.Parse(val);
				leftEyeMaskTexture.onValueChanged.Invoke(int.Parse(val));
				break;
			case "LeftEyeDecussationPos":
				found = true;
				leftEyeDecussationPos.text = val;
				leftEyeDecussationPos.onEndEdit.Invoke(val);
				break;
			case "LeftEyeCenterScale":
				found = true;
				leftEyeCenterScale.text = val;
				leftEyeCenterScale.onEndEdit.Invoke(val);
				break;
			case "LeftEyeDisplayContrast":
				found = true;
				leftEyeDisplayContrast.text = val;
				leftEyeDisplayContrast.onEndEdit.Invoke(val);
				break;
			case "LeftGrayPatchToggle":
				found = true;
				leftGrayPatchToggle.isOn = bool.Parse(val);
				leftGrayPatchToggle.onValueChanged.Invoke(bool.Parse(val));
				break;
			case "RightEyeCenterSceneToggle":
				found = true;
				rightEyeCenterSceneToggle.isOn = bool.Parse(val);
				rightEyeCenterSceneToggle.onValueChanged.Invoke(bool.Parse(val));
				break;
			case "RightEyePeripheralSceneToggle":
				found = true;
				rightEyePeripheralSceneToggle.isOn = bool.Parse(val);
				rightEyePeripheralSceneToggle.onValueChanged.Invoke(bool.Parse(val));
				break;
			case "RightEyeMaskTexture":
				found = true;
				rightEyeMaskTexture.value = int.Parse(val);
				rightEyeMaskTexture.onValueChanged.Invoke(int.Parse(val));
				break;
			case "RightEyeDecussationPos":
				rightEyeDecussationPos.text = val;
				rightEyeDecussationPos.onEndEdit.Invoke(val);
				break;
			case "RightEyeCenterScale":
				found = true;
				rightEyeCenterScale.text = val;
				rightEyeCenterScale.onEndEdit.Invoke(val);
				break;
			case "RightEyeDisplayContrast":
				found = true;
				rightEyeDisplayContrast.text = val;
				rightEyeDisplayContrast.onEndEdit.Invoke(val);
				break;
			case "RightGrayPatchToggle":
				found = true;
				rightGrayPatchToggle.isOn = bool.Parse(val);
				rightGrayPatchToggle.onValueChanged.Invoke(bool.Parse(val));
				break;
			case "RecordToggle":
				found = true;
				recordToggle.isOn = bool.Parse(val);
				recordToggle.onValueChanged.Invoke(bool.Parse(val));
				break;
            case "RandomizeToggle":
                found = true;
                randomizeToggle.onValueChanged.Invoke(bool.Parse(val));
                break;
			default:
				break;
			}
			return found;
		}
		catch (NullReferenceException  nr)
		{
		Debug.Log("Null reference " + nr.Message);
			return false;
		}
		catch (System.FormatException fe)
		{
			Debug.Log(fe);
			return false;
		}
		catch (System.Exception exp)
		{
			Debug.LogException (exp);
			return false;
		}
	}
	private bool ApplyLoadedTargetEntries(string varName, string val)
	{
		try
		{
			bool found = false;
			switch(varName)
			{
			case "StimRepeats":
				found = true;
				stimRepeats.text = val;
				stimRepeats.onEndEdit.Invoke(val);
				break;
			case "NearExcentricity":
				found = true;
				nearExcentricity.text = val;
				nearExcentricity.onEndEdit.Invoke(val);
				break;
			case "FarExcentricity":
				found = true;
				farExcentricity.text = val;
				farExcentricity.onEndEdit.Invoke(val);
				break;
			case "StimulusTime":
				found = true;
				stField.text = val;
				stField.onEndEdit.Invoke(val);
				break;
			case "ISI":
				found = true;
				isiField.text = val;
				isiField.onEndEdit.Invoke(val);
				break;
			case "TargetScale":
				found = true;
				targetScale.text = val;
				targetScale.onEndEdit.Invoke(val);
				break;
			case "WhichEyesControl":
				found = true;
				whichEyesControl.value = int.Parse(val);
				whichEyesControl.onValueChanged.Invoke(int.Parse(val));
				break;
			default:
				found = false;
				break;
			}
			return found;
		}
		catch (NullReferenceException  nr)
		{
			Debug.Log("Null reference " + nr.Message);
			return false;
		}
		catch (System.FormatException fe)
		{
			Debug.Log(fe);
			return false;
		}
		catch (System.Exception exp)
		{
			Debug.LogException (exp);
			return false;
		}
	}
	private bool ApplyLoadedSphereEntries(string varName, string val)
	{
		try
		{
			bool found = false;
			switch(varName)
			{
			case "SphereYawSpeed":
				found = true;
				sphereSpeedField.text = val;
				sphereSpeedField.onEndEdit.Invoke(val);
				break;
			case "SphereTextureContrast":
				found = true;
				sphereContrastField.text = val;
				sphereContrastField.onEndEdit.Invoke(val);
				break;
			case "SphereTexture":
				found = true;
				spherePanosDropDown.value = int.Parse(val);
				spherePanosDropDown.onValueChanged.Invoke(int.Parse(val));
				break;
			default:
				found = false;
				break;
			}
			return found;
		}
		catch (NullReferenceException  nr)
		{
			Debug.Log("Null reference " + nr.Message);
			return false;
		}
		catch (System.FormatException fe)
		{
			Debug.Log(fe);
			return false;
		}
		catch (System.Exception exp)
		{
			Debug.LogException (exp);
			return false;
		}
	}

    private bool ApplyLoadedDotMotionEntries(string varName, string val)
    {
        try
        {
            bool found = false;
            switch (varName)
            {
                case "NumOfDots":
                    found = true;
                    numOfDots.text = val;
                    numOfDots.onEndEdit.Invoke(val);
                    break;
                case "NumOfLifeCycles":
                    found = true;
                    numOfLifeCycles.text = val;
                    numOfLifeCycles.onEndEdit.Invoke(val);
                    break;
                case "DotRotVel":
                    found = true;
                    dotRotVel.text = val;
                    dotRotVel.onEndEdit.Invoke(val);
                    break;
                case "DotDistance":
                    found = true;
                    dotDistance.text = val;
                    dotDistance.onEndEdit.Invoke(val);
                    break;
                case "OnChangeBgLuminance":
                    found = true;
                    bgLuminance.text = val;
                    bgLuminance.onEndEdit.Invoke(val);
                    break;
                default:
                    found = false;
                    break;
            }
            return found;
        }
        catch (NullReferenceException nr)
        {
            Debug.Log("Null reference " + nr.Message);
            return false;
        }
        catch (System.FormatException fe)
        {
            Debug.Log(fe);
            return false;
        }
        catch (System.Exception exp)
        {
            Debug.LogException(exp);
            return false;
        }
    }

    private bool ApplyLoadedSceneEntries()
	{
		try
		{
            //apply menu changes through one full cycle (is set ot the  number of scenePanel (number of scenes) swhen loading menus. 
            //Information will be stored in the menu structures of the corresponding scenes
            //WE use this mechanism to converse with the (Oculus Rift) client by wiring thins into the menu just after the start of a new sceneEntries
            // On the admin side this would not be neccessary
            //if (menuAccessCount-- < 1)
            //	return;
            Task task = tasks.GetTask();
            Scene scene = task.GetScene();
            
			bool found = false;
			//SceneType sceneTyp = _sceneType;
			foreach (SceneEntry entry in scene.sceneEntries)
            {
				found = ApplyLoadedEntry(entry.varName, entry.val);
				if (!found)
				{
					//if (sceneTyp.Equals(SceneType.Sphere))
					found = ApplyLoadedSphereEntries(entry.varName, entry.val);
					if (!found)
					{
						//else if (sceneTyp.Equals(SceneType.Target))
						found = ApplyLoadedTargetEntries(entry.varName, entry.val);
                        if (!found)
                        {
                            found = ApplyLoadedDotMotionEntries(entry.varName, entry.val);
                        }
					}
				}
			}
			//make shure to switch recording on, it is enabled in menu but not saved in menu file
			//since we stopped it at the end of the previous scaene/task
			if (recordToggle.isOn)
				recordToggle.onValueChanged.Invoke(true);

			return found;
		}
		catch (NullReferenceException  nr)
		{
			Debug.Log("Null reference " + nr.Message);
			return false;
		}
		catch (System.FormatException fe)
		{
			Debug.Log(fe);
			return false;
		}
		catch (System.Exception exp)
		{
			Debug.LogException (exp);
			return false;
		}
	}


} // end MenuCanvasControl
