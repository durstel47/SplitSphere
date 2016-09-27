// -----------------------------------------------------------------------
//
// (c) Copyright 1997-2015, SensoMotoric Instruments GmbH
// 
// Permission  is  hereby granted,  free  of  charge,  to any  person  or
// organization  obtaining  a  copy  of  the  software  and  accompanying
// documentation  covered  by  this  license  (the  "Software")  to  use,
// reproduce,  display, distribute, execute,  and transmit  the Software,
// and  to  prepare derivative  works  of  the  Software, and  to  permit
// third-parties to whom the Software  is furnished to do so, all subject
// to the following:
// 
// The  copyright notices  in  the Software  and  this entire  statement,
// including the above license  grant, this restriction and the following
// disclaimer, must be  included in all copies of  the Software, in whole
// or  in part, and  all derivative  works of  the Software,  unless such
// copies   or   derivative   works   are   solely   in   the   form   of
// machine-executable  object   code  generated  by   a  source  language
// processor.
// 
// THE  SOFTWARE IS  PROVIDED  "AS  IS", WITHOUT  WARRANTY  OF ANY  KIND,
// EXPRESS OR  IMPLIED, INCLUDING  BUT NOT LIMITED  TO THE  WARRANTIES OF
// MERCHANTABILITY,   FITNESS  FOR  A   PARTICULAR  PURPOSE,   TITLE  AND
// NON-INFRINGEMENT. IN  NO EVENT SHALL  THE COPYRIGHT HOLDERS  OR ANYONE
// DISTRIBUTING  THE  SOFTWARE  BE   LIABLE  FOR  ANY  DAMAGES  OR  OTHER
// LIABILITY, WHETHER  IN CONTRACT, TORT OR OTHERWISE,  ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE  SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// -----------------------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace SMI {
//enable eye tracking only for STAGE settings

    public class SMIEyetracking : MonoBehaviour {
        [Tooltip("Select the layers which should be hit by gaze")]
        public LayerMask defaultLayerSelectionByGaze = -1; // -1 = "Everything"

        [Space]
        [Tooltip("Choose the color for the calibration target")]
        public Color foregroundColor = Color.white;

        [Tooltip("Choose the background color for the calibration")]
        public Color backgroundColor = new Color(0.3f, 0.3f, 0.3f);

        [Range (0,1)]
        [Tooltip("Choose the background alpha for calibration visualization")]
        public float backgroundAlpha = 1.0f;

        [Space]

        [Tooltip("Define how fast the calibration targets should move")]
        public float calibrationTargetSpeed = 1.0f;

        public float gazeMappingDistanceValidation = 1.5f;

        // Max Distance for the Raycasting
        [Tooltip("Define the maximal distance for gaze based raycasting")]
        public float maxRayCastDistance = 3500f;

        // Enable a fake Gazeinput. The Server will stream generated Data
        [Tooltip("The eye tracking server will simulate a datastream if enabled")]
        public bool simulationModeActive = false;

        // Disable the internal dataFilter of the HMD Data Streaming
        [Tooltip("If enabled, the eye tracking server will filter the gaze information")]
        public bool GazeFilterEnabled = false;

        [Space]

        [SerializeField]
        private KeyCode quitApplication = KeyCode.Escape;

        [SerializeField]
        private KeyCode closeVisualization = KeyCode.Q;

        [SerializeField]
        private KeyCode startOnePointCalibration = KeyCode.Alpha1;

        [SerializeField]
        private KeyCode startThreePointCalibration = KeyCode.Alpha3;

        [SerializeField]
        private KeyCode startFivePointCalibration = KeyCode.Alpha5;

        [SerializeField]
        private KeyCode startNinePointCalibration = KeyCode.Alpha9;

        [SerializeField]
        private KeyCode acceptCalibrationPoint = KeyCode.Space;

        [SerializeField]
        private KeyCode resetCalibration = KeyCode.N;

        [SerializeField]
        private KeyCode startQuantitativeValidation = KeyCode.B;

        [SerializeField]
        private KeyCode startGridValidation = KeyCode.V;

        [SerializeField]
        private KeyCode loadCalibration = KeyCode.K;

        [SerializeField]
        private KeyCode saveCalibration = KeyCode.H;

        [Space]

        [Tooltip("If enabled, Time.timescale will be set to '0' during a calibration")]

		public bool pauseGameWhileCalibrating = false;

        private GameObject calibrationCamera;
        public GameObject CalibrationCamera { get { return calibrationCamera; } }

   
        private GameObject smiEyeTracker;
        public GameObject SMIEyeTracker {  get { return smiEyeTracker; } }

#if STAGE       
		// Use this for initialization of SMI components
        void Awake()
        {
            SetupSMICalibrationCamera();
            SetupSMIEyeTracker();
            SetupSMIGazeControllerKeyInput();
            SetupSMIGazeController();
            SetupSMICalibrationVisualizer();
        }

        // Use this for initialization
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {

        }
#endif 
        void SetupSMIEyeTracker ()
        {
            smiEyeTracker = Instantiate(Resources.Load("SMIEyeTracker"), transform.position, transform.rotation) as GameObject;
            smiEyeTracker.name = smiEyeTracker.name.Replace("(Clone)", "");
            smiEyeTracker.transform.parent = transform;
        }

        // Sets up the SMI calibration camera
        void SetupSMICalibrationCamera()
        {
            calibrationCamera = Instantiate(Resources.Load("SMICalibrationCamera"), transform.position, transform.rotation) as GameObject;
            calibrationCamera.name = calibrationCamera.name.Replace("(Clone)", "");
            calibrationCamera.transform.parent = transform.parent;
            calibrationCamera.GetComponent<SMIUpdateCameraPositionAndRotation>().MainCamera = gameObject.GetComponent<Camera>();
        }

        void SetupSMIGazeControllerKeyInput()
        {
            SMIGazeControllerKeyInput.Instance.QuitApplication = quitApplication;
            SMIGazeControllerKeyInput.Instance.CloseVisualization = closeVisualization;
            SMIGazeControllerKeyInput.Instance.ResetCalibration = resetCalibration;
            SMIGazeControllerKeyInput.Instance.SaveCalibration = saveCalibration;
            SMIGazeControllerKeyInput.Instance.LoadCalibration = loadCalibration;
            SMIGazeControllerKeyInput.Instance.StartOnePointCalibration = startOnePointCalibration;
            SMIGazeControllerKeyInput.Instance.StartThreePointCalibration = startThreePointCalibration;
            SMIGazeControllerKeyInput.Instance.StartFivePointCalibration = startFivePointCalibration;
            SMIGazeControllerKeyInput.Instance.StartNinePointCalibration = startNinePointCalibration;
            SMIGazeControllerKeyInput.Instance.StartGridValidation = startGridValidation;
            SMIGazeControllerKeyInput.Instance.StartQuantitativeValidation = startQuantitativeValidation;
        }

        void SetupSMIGazeController()
        {
            SMIGazeController.Instance.BackgroundColor = backgroundColor;
            SMIGazeController.Instance.BackgroundAlpha = backgroundAlpha;
            SMIGazeController.Instance.ForegroundColor = foregroundColor;
            SMIGazeController.Instance.GazeFilterEnabled = GazeFilterEnabled;
            SMIGazeController.Instance.IsSimulationModeActive = simulationModeActive;
            SMIGazeController.Instance.MaxRayCastDistance = maxRayCastDistance;
            SMIGazeController.Instance.DefaultLayerForGazeSelection = defaultLayerSelectionByGaze;
        }

        void SetupSMICalibrationVisualizer()
        {
            SMICalibrationVisualizer.Instance.AcceptCalibrationKey = acceptCalibrationPoint;
            SMICalibrationVisualizer.Instance.PauseGameWhileCalibration = pauseGameWhileCalibrating;
            SMICalibrationVisualizer.Instance.CalibrationTargetSpeed = calibrationTargetSpeed;
            SMICalibrationVisualizer.Instance.GazeMappingDistanceValidation = gazeMappingDistanceValidation;
	    }
		  
    }


}