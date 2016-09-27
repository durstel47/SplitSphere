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
using System.Collections.Generic;

namespace SMI
{
    [AddComponentMenu("SMI/ SMI Default Keyboard Manager")]
    public class SMIGazeControllerKeyInput : MonoBehaviour
    {
        public bool useCalibrationMenuExample = true;

        [HideInInspector]
        [SerializeField]
        private KeyCode quitApplication;
        public KeyCode QuitApplication
        {
            get { return quitApplication; }
            set { quitApplication = value; }
        }

        [HideInInspector]
        [SerializeField]
        private KeyCode closeVisualization;
        public KeyCode CloseVisualization
        {
            get { return closeVisualization; }
            set { closeVisualization = value; }
        }

        [HideInInspector]
        [SerializeField]
        private KeyCode startOnePointCalibration;
        public KeyCode StartOnePointCalibration
        {
            get { return startOnePointCalibration; }
            set { startOnePointCalibration = value; }
        }

        [HideInInspector]
        [SerializeField]
        private KeyCode startThreePointCalibration;
        public KeyCode StartThreePointCalibration
        {
            get { return startThreePointCalibration; }
            set { startThreePointCalibration = value; }
        }

        [HideInInspector]
        [SerializeField]
        private KeyCode startFivePointCalibration;
        public KeyCode StartFivePointCalibration
        {
            get { return startFivePointCalibration; }
            set { startFivePointCalibration = value; }
        }

        [HideInInspector]
        [SerializeField]
        private KeyCode startNinePointCalibration;
        public KeyCode StartNinePointCalibration
        {
            get { return startNinePointCalibration; }
            set { startNinePointCalibration = value; }
        }

        [HideInInspector]
        [SerializeField]
        private KeyCode resetCalibration;
        public KeyCode ResetCalibration
        {
            get { return resetCalibration; }
            set { resetCalibration = value; }
        }

        [HideInInspector]
        [SerializeField]
        private KeyCode startQuantitativeValidation;
        public KeyCode StartQuantitativeValidation
        {
            get { return startQuantitativeValidation; }
            set { startQuantitativeValidation = value; }
        }

        [HideInInspector]
        [SerializeField]
        private KeyCode startGridValidation;
        public KeyCode StartGridValidation
        {
            get { return startGridValidation; }
            set { startGridValidation = value; }
        }

        [HideInInspector]
        [SerializeField]
        private KeyCode loadCalibration;
        public KeyCode LoadCalibration
        {
            get { return loadCalibration; }
            set { loadCalibration = value; }
        }

        [HideInInspector]
        [SerializeField]
        private KeyCode saveCalibration;
        public KeyCode SaveCalibration
        {
            get { return saveCalibration; }
            set { saveCalibration = value; }
        }

        private SMICalibrationVisualizer calibVis;
        private SMILoadAndSaveCalibration loadAndSaveCalibration;

        private GameObject calibrationCamera;

        private static SMIGazeControllerKeyInput instance;

        public static SMIGazeControllerKeyInput Instance
        {
            get
            {
                if (!instance)
                {
                    instance = (SMIGazeControllerKeyInput)FindObjectOfType(typeof(SMIGazeControllerKeyInput));

                    if (!instance)
                    {
                        GameObject gameObject = new GameObject();
                        gameObject.name = "SMIGazeControllerKeyInput";
                        instance = gameObject.AddComponent(typeof(SMIGazeControllerKeyInput)) as SMIGazeControllerKeyInput;
                    }
                }
                return instance;
            }
        }

        void Start()
        {
            if (!instance)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            calibrationCamera = GameObject.Find("SMICalibrationCamera");
            calibVis = GetComponent<SMICalibrationVisualizer>();

            if (useCalibrationMenuExample)
            {
                GameObject obj = Instantiate(Resources.Load("SMICalibrationMenu")) as GameObject;
                obj.transform.SetParent(gameObject.transform);
                //obj.transform.parent = gameObject.transform;
                obj.transform.SetParent(calibrationCamera.transform);
                //obj.transform.parent = calibrationCamera.transform;
                obj.name = "SMICalibrationMenu";
                loadAndSaveCalibration = obj.GetComponent<SMILoadAndSaveCalibration>();
            }
        }

        void Update()
        {
            smi_manageStandardKeyInput();
#if !UNITY_EDITOR
#endif
        }

//#if !UNITY_EDITOR
        /// <summary>
        /// Start Calibrations inside of the Unityapplication
        /// </summary>
        private void smi_manageStandardKeyInput()
        {
            if (!SMILoadAndSaveCalibration.isCalibrationMenuOpen)
            {
                //Setup a CalibrationClass
                SMI.SMIGazeController.SMIcWrapper.smi_CalibrationClass calibrationInformation = new
                SMI.SMIGazeController.SMIcWrapper.smi_CalibrationClass();

                //Use the default Visualisation
                calibrationInformation.client_visualisation = true;

                //Set the Colors of the background and the foreground
                calibrationInformation.backgroundColor = SMIGazeController.Instance.BackgroundColor;
                calibrationInformation.foregroundColor = SMIGazeController.Instance.ForegroundColor;

                calibrationInformation.calibrationPointList = new List<Vector2>();

                #region Set the Type and Start the Calibration
                if (Input.GetKeyDown(startOnePointCalibration) && !SMIGazeController.Instance.IsSimulationModeActive)
                {
					if (SMICalibrationVisualizer.stateOfTheCalibrationView.Equals(SMICalibrationVisualizer.VisualisationState.gridValidation)){
						calibVis.smi_FinishValidation(true);
					}
					if (SMICalibrationVisualizer.stateOfTheCalibrationView.Equals(SMICalibrationVisualizer.VisualisationState.quantitativeValidation)){
						calibVis.smi_AbortValidation(true);
					}

					calibrationInformation.type = SMIGazeController.SMIcWrapper.smi_CalibrationType.OnePointCalibration;

                    calibVis.smi_SetupCalibrationInClient(calibrationInformation);
                    calibVis.smi_CalibrateInUnity();
                }

                else if (Input.GetKeyDown(startThreePointCalibration) && !SMIGazeController.Instance.IsSimulationModeActive)
                {
					if (SMICalibrationVisualizer.stateOfTheCalibrationView.Equals(SMICalibrationVisualizer.VisualisationState.gridValidation)){
						calibVis.smi_FinishValidation(true);
					}
					if (SMICalibrationVisualizer.stateOfTheCalibrationView.Equals(SMICalibrationVisualizer.VisualisationState.quantitativeValidation)){
						calibVis.smi_AbortValidation(true);
					}
                    calibrationInformation.type = SMIGazeController.SMIcWrapper.smi_CalibrationType.ThreePointCalibration;

                    // define a custom calibration grid (which is actually the default one) 
                    //calibrationInformation.calibrationPointList.Add(new Vector2(780, 453));
                    //calibrationInformation.calibrationPointList.Add(new Vector2(1139, 453));
                    //calibrationInformation.calibrationPointList.Add(new Vector2(960, 626));

                    calibVis.smi_SetupCalibrationInClient(calibrationInformation);
                    calibVis.smi_CalibrateInUnity();
                }

                else if (Input.GetKeyDown(startFivePointCalibration) && !SMIGazeController.Instance.IsSimulationModeActive)
                {
					if (SMICalibrationVisualizer.stateOfTheCalibrationView.Equals(SMICalibrationVisualizer.VisualisationState.gridValidation)){
						calibVis.smi_FinishValidation(true);
					}
					if (SMICalibrationVisualizer.stateOfTheCalibrationView.Equals(SMICalibrationVisualizer.VisualisationState.quantitativeValidation)){
						calibVis.smi_AbortValidation(true);
					}
                    calibrationInformation.type = SMIGazeController.SMIcWrapper.smi_CalibrationType.FivePointCalibration;

                    calibVis.smi_SetupCalibrationInClient(calibrationInformation);
                    calibVis.smi_CalibrateInUnity();
                }

                else if (Input.GetKeyDown(startNinePointCalibration) && !SMIGazeController.Instance.IsSimulationModeActive)
                {
					if (SMICalibrationVisualizer.stateOfTheCalibrationView.Equals(SMICalibrationVisualizer.VisualisationState.gridValidation)){
						calibVis.smi_FinishValidation(true);
					}
					if (SMICalibrationVisualizer.stateOfTheCalibrationView.Equals(SMICalibrationVisualizer.VisualisationState.quantitativeValidation)){
						calibVis.smi_AbortValidation(true);
					}
                    calibrationInformation.type = SMIGazeController.SMIcWrapper.smi_CalibrationType.NinePointCalibration;

                    calibVis.smi_SetupCalibrationInClient(calibrationInformation);
                    calibVis.smi_CalibrateInUnity();
                }
                #endregion

                //Reset Calibration
                else if (Input.GetKeyDown(resetCalibration) && !SMIGazeController.Instance.IsSimulationModeActive)
                {
                    SMIGazeController.Instance.smi_resetCalibration();
                }

                    //Show Quantitative Validation Screen
                else if (Input.GetKeyDown(startQuantitativeValidation) && !SMIGazeController.Instance.IsSimulationModeActive)
                {
                    calibVis.smi_SetupQuantitativeValidation();
                }
                //Show ValidationGrid
                else if (Input.GetKeyDown(startGridValidation))
                {
                    calibVis.smi_ShowGridValidation();
                }

                else if (Input.GetKeyDown(saveCalibration) && !SMIGazeController.Instance.IsSimulationModeActive)
                {
                    loadAndSaveCalibration.ShowSaveCalibrationScreen();
                }
                else if (Input.GetKeyDown(loadCalibration) && !SMIGazeController.Instance.IsSimulationModeActive)
                {
                    loadAndSaveCalibration.ShowLoadCalibrationScreen(); 
                }
                else if (Input.GetKeyDown(closeVisualization))
                {
                    calibVis.closeVisualization();
                }

                else if(Input.GetKeyDown(quitApplication))
                {
                    Application.Quit();
                }
            }
        }
    }
}
