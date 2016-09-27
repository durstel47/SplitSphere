// -----------------------------------------------------------------------
//
// (c) Copyright 1997-2015, SensoMotoric Instruments GmbH
// 
// Permission  is  hereby granted,  free  of  charge,  to any  person  or
// organization  obtaining  a  copy  of  the  software  and  accompanying
// documentation  covered  by  this  license  (the  "Software")  to  use,
// reproduce,  displdas ay, distribute, execute,  and transmit  the Software,
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
using System.Threading;



namespace SMI
{
    [System.Serializable]
    [RequireComponent(typeof(SMIGazeController))]
    public class SMICalibrationVisualizer : MonoBehaviour
    {

        #region public member variables
        //state of the Element
        public enum VisualisationState
        {
            calibration,
            gridValidation,
            quantitativeValidation,
            resultsOfQuantitativeValidation,
            closeResults,
            None
        }
        public static VisualisationState stateOfTheCalibrationView = VisualisationState.None;
        public KeyCode AcceptCalibrationKey
        {
            get { return acceptCalibrationKey; }
            set { acceptCalibrationKey = value; }
        }
        public bool PauseGameWhileCalibration
        {
            get { return pauseGameWhileCalibration; }
            set { pauseGameWhileCalibration = value; }
        }

        public float CalibrationTargetSpeed
        {
            get { return calibrationTargetSpeed; }
            set { calibrationTargetSpeed = value; }
        }

        public float GazeMappingDistanceValidation
        {
            get { return gazeMappingDistanceValidation; }
            set { gazeMappingDistanceValidation = value; }
        }
        #endregion

        #region private member variables        
        private KeyCode acceptCalibrationKey;
        private bool pauseGameWhileCalibration;
        private float calibrationTargetSpeed;
        private float gazeMappingDistanceValidation;

        private GameObject calibrationTarget;
        private GameObject calibrationTarget_Scale;
        private Vector3 initScale;
        private GameObject gazePositionTarget;
        private GameObject anchorValidationGrid;
        private GameObject anchorValidationView;
        private GameObject[] gazeTargetValidationViewItems;
        private GameObject[] gazeTargetsOfQuantiativeValidation;
        private string calibrationTargetName = "SMICalibrationTarget_RedSphere";
        private Thread calibrationThread;
        private CalibrationJob job;
        private Vector2[] positionsOfPORForQuantiativeValidation;
        private Vector2[] targetPositions;
        private LayerMask calibrationLayer;
        private Camera rayCam;
        private GameObject calibrationCamera;
        private GameObject backgroundCamera;
        private int targetID = 0;
        private float[] validationItems;
        private SMITextView TextViewValidation;
        private float targetSizeMin = 0.3f;
        private bool movementAnimationDone = false;
        private float timeStampSinceLastCalibrationAction;
        private float timeright = 5.0f;
        private bool isInitialized = false;

        private struct BackgroundColor
        {
            private float red;
            private float green;
            private float blue;
            private float alpha;

            public float Red
            {
                get { return red; }
                set { red = value; }
            }
            public float Green
            {
                get { return green; }
                set { green = value; }
            }
            public float Blue
            {
                get { return blue; }
                set { blue = value; }
            }
            public float Alpha
            {
                get { return alpha; }
                set { alpha = value; }
            }
        }
        private BackgroundColor backgroundColor;
        private Color b;

        private static SMICalibrationVisualizer instance;
        #endregion

        #region inherited unity methods

        void Awake()
        {
            if (!instance)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            calibrationCamera = GameObject.Find("SMICalibrationCamera");

            rayCam = calibrationCamera.GetComponent<Camera>();
            calibrationLayer = LayerMask.NameToLayer("CalibrationView");

            backgroundColor.Red = 0.4f;// SMIGazeController.Instance.BackgroundColor.r;
            backgroundColor.Green = 0.4f;//SMIGazeController.Instance.BackgroundColor.g;
            backgroundColor.Blue = 0.4f;//SMIGazeController.Instance.BackgroundColor.b;

            smi_InitCalibrationView();
            smi_InitValidationView();

            targetPositions = new[] { new Vector2(0f, 0f) };
        }

        /// <summary>
        /// Close the visualization and close the application
        /// </summary>
        void OnApplicationQuit()
        {
            closeVisualization();
        }

        public void closeVisualization()
        {
            switch (stateOfTheCalibrationView)
            {
                case VisualisationState.calibration:
                    smi_AbortCalibration(false);
                    break;

                case VisualisationState.gridValidation:
                    smi_FinishValidation(false);
                    break;

                case VisualisationState.quantitativeValidation:
                    smi_AbortValidation(false);
                    break;
            }
        }

        void Update()
        {
            if (stateOfTheCalibrationView.Equals(VisualisationState.None))
            {
				if (calibrationTarget != null)
                	calibrationTarget.SetActive(false);
				else return;
            }

                #region CalibrationMode
            if (stateOfTheCalibrationView.Equals(VisualisationState.calibration))
            {
                //Accept the Point and start the Detection for a new Fixation
                if (movementAnimationDone && (Input.GetKeyDown(acceptCalibrationKey) || SMIGazeController.SMIcWrapper.smi_checkForNewFixation()))
                {
                    timeStampSinceLastCalibrationAction = Time.timeSinceLevelLoad;
                    smi_UpdateTargetDestination();
                    movementAnimationDone = false;
                }

                Thread.Sleep(10);

                if (targetPositions != null)
                {
                    Vector3 newCalibrationTargetPosition = smi_CalculatePositionOnGazeMappingPlane(targetPositions[targetID], gazeMappingDistanceValidation);
                    
                    float distanceOfPoints = Vector3.Distance(calibrationTarget.transform.localPosition, newCalibrationTargetPosition);
                    
                    // Move element if the Distance is big enough
                    if (distanceOfPoints > 0.01f && movementAnimationDone == false)
                    {
                        calibrationTarget.transform.position = Vector3.Lerp(calibrationTarget.transform.position, newCalibrationTargetPosition, 0.15f * calibrationTargetSpeed);
                        calibrationTarget_Scale.transform.localScale = initScale;

                        SMIGazeController.SMIcWrapper.smi_startDetectingNewFixation();
                    }

                    // Element is on the final Position for the Calibration -> Stay on the position
                    else
                    {
                        movementAnimationDone = true;

                        calibrationTarget.transform.position = smi_CalculatePositionOnGazeMappingPlane(targetPositions[targetID], 1.5f);

                        if (!calibrationTarget_Scale.transform.localScale.Equals(initScale * targetSizeMin))
                        {
                            calibrationTarget_Scale.transform.localScale = Vector3.Lerp(calibrationTarget_Scale.transform.localScale, initScale * targetSizeMin, 0.15f * calibrationTargetSpeed);
                        }
                    }

                }
            }
            #endregion

            #region ValidationMode
            else if (stateOfTheCalibrationView.Equals(VisualisationState.quantitativeValidation))
            {
                //Accept the Point and start the Detection for a new Fixation
                if (movementAnimationDone && (Input.GetKeyDown(acceptCalibrationKey) || SMIGazeController.SMIcWrapper.smi_checkForNewFixation()))
                {
                    timeStampSinceLastCalibrationAction = Time.timeSinceLevelLoad;
                    smi_UpdateTargetDestination();
                    movementAnimationDone = false;
                }

                if (targetPositions != null && targetID < targetPositions.Length)
                {
                    Vector3 newValidationTargetPosition = smi_CalculatePositionOnGazeMappingPlane(targetPositions[targetID], gazeMappingDistanceValidation);
                    float distanceOfPoints = Vector3.Distance(calibrationTarget.transform.localPosition, newValidationTargetPosition);

                    if (distanceOfPoints > 0.01f && movementAnimationDone == false)
                    {
                        calibrationTarget.transform.position = Vector3.Lerp(calibrationTarget.transform.position, newValidationTargetPosition, 0.15f * calibrationTargetSpeed);
                        calibrationTarget_Scale.transform.localScale = initScale;

                        SMIGazeController.SMIcWrapper.smi_startDetectingNewFixation();
                    }
                    else
                    {
                        movementAnimationDone = true;
                        calibrationTarget.transform.position = smi_CalculatePositionOnGazeMappingPlane(targetPositions[targetID], 1.5f);


                        if (!calibrationTarget_Scale.transform.localScale.Equals(initScale * targetSizeMin))
                        {
                            calibrationTarget_Scale.transform.localScale = Vector3.Lerp(calibrationTarget_Scale.transform.localScale, initScale * targetSizeMin, 0.15f * calibrationTargetSpeed);
                        }
                    }

                }
            }
            else if (stateOfTheCalibrationView.Equals(VisualisationState.resultsOfQuantitativeValidation))
            {
                ShowValidationResults();
            }
            else if (stateOfTheCalibrationView.Equals(VisualisationState.closeResults))
            {
                CloseValidationResults();
            }

            else if (stateOfTheCalibrationView.Equals(VisualisationState.gridValidation))
            {
                gazePositionTarget.transform.position = smi_CalculatePositionOnGazeMappingPlane(SMI.SMIGazeController.Instance.smi_getSample().por, gazeMappingDistanceValidation);
            }

            #endregion

            #region NormalMode
            else
            {

            }

            //ThreadJoining
            if (stateOfTheCalibrationView.Equals(VisualisationState.None) && calibrationThread.IsAlive)
            {
                calibrationThread.Join();
            }

            #endregion

        }
        #endregion

        #region public methods

        /// <summary>
        /// Setup a Calibrationview
        /// </summary>
        /// <param name="calibrationInformation">Parameterclass for the paramter of the Calibration</param>
        public void smi_SetupCalibrationInClient(SMIGazeController.SMIcWrapper.smi_CalibrationClass calibrationInformation)
        {
            if (stateOfTheCalibrationView.Equals(VisualisationState.None))
            {
                if (calibrationInformation.client_visualisation == true)
                {
                    SMICreateBackgroundCamera.Instance.SetUpBackgroundCamera();
                    Thread.Sleep(10);
                    backgroundCamera = GameObject.Find("SMIBackgroundCamera");

                    targetPositions = calibrationInformation.calibrationPointList.ToArray();

                    backgroundColor.Red = calibrationInformation.backgroundColor.r;
                    backgroundColor.Green = calibrationInformation.backgroundColor.g;
                    backgroundColor.Blue = calibrationInformation.backgroundColor.b;

                    movementAnimationDone = false;

                    //No Custom Parameter Detected
                    if (targetPositions.Length == 0)
                    {
                        targetPositions = SMIGazeController.SMIcWrapper.DefaultCalibrationInformations.selectDefaultCalibration(calibrationInformation.type);
                    }

                    //Setup the informations for the ServerApp
                    SMIGazeController.Instance.smi_setupCalibration(calibrationInformation);
                    calibrationTarget.transform.position = smi_CalculatePositionOnGazeMappingPlane(new Vector2(960, 540), 1.5f);

                }
            }
        }

        /// <summary>
        /// Reset the ValidationValues and the position of the Gaze in the validationmode
        /// </summary>
        public void smi_SetupQuantitativeValidation()
        {
            if (stateOfTheCalibrationView.Equals(VisualisationState.None))
            {
                SMICreateBackgroundCamera.Instance.SetUpBackgroundCamera();
                backgroundCamera = GameObject.Find("SMIBackgroundCamera");
                
                smi_InitValidationView();

                SMIGazeController.SMIcWrapper.smi_startDetectingNewFixation();
                targetID = 0;

                validationItems = new float[4];
                positionsOfPORForQuantiativeValidation = new Vector2[4];

                calibrationTarget.SetActive(true);

                targetPositions = SMIGazeController.SMIcWrapper.DefaultCalibrationInformations.validationPoints;
                stateOfTheCalibrationView = VisualisationState.quantitativeValidation;

                calibrationTarget.transform.position = smi_CalculatePositionOnGazeMappingPlane(new Vector2(960, 540), 1.5f);

                backgroundCamera.GetComponent<SMICameraController>().dofFrom = 0f;
                backgroundCamera.GetComponent<SMICameraController>().dofTo = 20f;

                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt0.r = SMIGazeController.Instance.BackgroundColor.r;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt0.g = SMIGazeController.Instance.BackgroundColor.g;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt0.b = SMIGazeController.Instance.BackgroundColor.b;
                backgroundCamera.GetComponent<SMICameraController>().backgroundAlpha = SMIGazeController.Instance.BackgroundAlpha;

                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt1.r = SMIGazeController.Instance.BackgroundColor.r;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt1.g = SMIGazeController.Instance.BackgroundColor.g;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt1.b = SMIGazeController.Instance.BackgroundColor.b;

                Material mat = Resources.Load<Material>("SMICalibrationMaterial_White");
                mat.color = new Color(SMIGazeController.Instance.ForegroundColor.r, SMIGazeController.Instance.ForegroundColor.g, SMIGazeController.Instance.ForegroundColor.b, 1f);
            }
        }

        /// <summary>
        /// Show the GridValidation 
        /// </summary>
        public void smi_ShowGridValidation()
        {
            if (stateOfTheCalibrationView.Equals(VisualisationState.None))
            {
                SMICreateBackgroundCamera.Instance.SetUpBackgroundCamera();
                backgroundCamera = GameObject.Find("SMIBackgroundCamera");

                anchorValidationGrid.SetActive(true);
                gazePositionTarget.SetActive(true);
                gazePositionTarget.transform.position = new Vector3(0f, 0f, 2f);
                stateOfTheCalibrationView = VisualisationState.gridValidation;

                if (pauseGameWhileCalibration)
                {
                    Time.timeScale = 0;
                }

                backgroundCamera.GetComponent<SMICameraController>().dofFrom = 0f;
                backgroundCamera.GetComponent<SMICameraController>().dofTo = 20f;

                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt0.r = SMIGazeController.Instance.BackgroundColor.r;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt0.g = SMIGazeController.Instance.BackgroundColor.g;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt0.b = SMIGazeController.Instance.BackgroundColor.b;
                backgroundCamera.GetComponent<SMICameraController>().backgroundAlpha = SMIGazeController.Instance.BackgroundAlpha;

                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt1.r = SMIGazeController.Instance.BackgroundColor.r;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt1.g = SMIGazeController.Instance.BackgroundColor.g;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt1.b = SMIGazeController.Instance.BackgroundColor.b;

                Material mat = Resources.Load<Material>("SMICalibrationMaterial_White");
                mat.color = new Color(SMIGazeController.Instance.ForegroundColor.r, SMIGazeController.Instance.ForegroundColor.g, SMIGazeController.Instance.ForegroundColor.b, 1f);
            }
        }

        /// <summary>
        /// Start the Calibration
        /// </summary>
        public void smi_CalibrateInUnity()
        {
            if (!SMIGazeController.Instance.IsSimulationModeActive && stateOfTheCalibrationView.Equals(VisualisationState.None))
            {
                smi_InitCalibrationView();
               
                targetID = 0;
                stateOfTheCalibrationView = VisualisationState.calibration;
                calibrationTarget.SetActive(true);

                job = new CalibrationJob();
                calibrationThread = new Thread(job.DoWork);
                calibrationThread.Start();

                if (pauseGameWhileCalibration)
                {
                    Time.timeScale = 0;
                }
                backgroundCamera.GetComponent<SMICameraController>().dofFrom = 0f;
                backgroundCamera.GetComponent<SMICameraController>().dofTo = 20f;

                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt0.r = backgroundColor.Red;// SMIGazeController.Instance.BackgroundColor.r;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt0.g = backgroundColor.Green;// SMIGazeController.Instance.BackgroundColor.g;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt0.b = backgroundColor.Blue;// SMIGazeController.Instance.BackgroundColor.b;
                backgroundCamera.GetComponent<SMICameraController>().backgroundAlpha = SMIGazeController.Instance.BackgroundAlpha;

                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt1.r = backgroundColor.Red;// SMIGazeController.Instance.BackgroundColor.r;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt1.g = backgroundColor.Green;// SMIGazeController.Instance.BackgroundColor.g;
                backgroundCamera.GetComponent<SMICameraController>().destinationCalibrationColorAt1.b = backgroundColor.Blue;// SMIGazeController.Instance.BackgroundColor.b;

                Material mat = Resources.Load<Material>("SMICalibrationMaterial_White");
                mat.color = new Color(SMIGazeController.Instance.ForegroundColor.r, SMIGazeController.Instance.ForegroundColor.g, SMIGazeController.Instance.ForegroundColor.b, 1f);
            }
        }

        /// <summary>
        /// Abort the Calibration
        /// </summary>
        public void smi_AbortCalibration(bool startNewVisualization)
        {
            targetID = 0;
            SMIGazeController.Instance.smi_abortCalibration();
            calibrationThread.Abort();
            stateOfTheCalibrationView = VisualisationState.None;
            SMICreateBackgroundCamera.Instance.DestroyBackgroundCamera(startNewVisualization);
        }

        /// <summary>
        /// Abort the Validation 
        /// </summary>
        public void smi_AbortValidation(bool startNewVisualization)
        {
            targetID = 0;
            stateOfTheCalibrationView = VisualisationState.None;
            gazePositionTarget.SetActive(false);
            if (pauseGameWhileCalibration)
            {
                Time.timeScale = 1;
            }
            SMICreateBackgroundCamera.Instance.DestroyBackgroundCamera(startNewVisualization);
        }
        #endregion

        #region private methods

        /// <summary>
        /// Setup the CalibrationView
        /// </summary>
        private void smi_InitCalibrationView()
        {
            ////Instanciate the Targets
            if (calibrationTarget == null)
            {
                calibrationTarget = Instantiate(Resources.Load(calibrationTargetName, typeof(GameObject)), transform.position, Quaternion.identity) as GameObject;
                calibrationTarget.name = calibrationTargetName;
                calibrationTarget.layer = calibrationLayer;
                calibrationTarget.SetActive(false);
            }

            movementAnimationDone = false;
            if (!isInitialized)
            {
                foreach (Transform child in calibrationTarget.transform)
                {
                    if (child.name == "CalibrationTarget_WhiteSphere")
                    {
                        calibrationTarget_Scale = child.gameObject;
                        initScale = child.transform.localScale;
                    }
                    child.gameObject.layer = calibrationLayer;
                }
                isInitialized = true;
            } else
            {
                calibrationTarget_Scale.transform.localScale = initScale;
            }

            //Setup the Thread
            job = new CalibrationJob();
            calibrationThread = new Thread(job.DoWork);
        }

        /// <summary>
        /// Add the Components to the SMI Visualizer
        /// </summary>
        private void smi_InitValidationView()
        {
            //ValidationGrid;
            anchorValidationGrid = Instantiate(Resources.Load("SMIValidationGrid", typeof(GameObject)), transform.position, Quaternion.identity) as GameObject;
            anchorValidationGrid.name = "SMIValidationGrid";
            anchorValidationGrid.transform.parent = calibrationCamera.transform;
            anchorValidationGrid.layer = calibrationLayer;
            anchorValidationGrid.transform.localRotation = Quaternion.identity;
            anchorValidationGrid.SetActive(false);

            //gazeTarget for the Grid Validation
            gazePositionTarget = Instantiate(Resources.Load("SMIGazeTarget_Validation", typeof(GameObject)), transform.position, Quaternion.identity) as GameObject;
            gazePositionTarget.name = "SMIGazeTarget_Validation";
            gazePositionTarget.transform.parent = anchorValidationGrid.transform.parent;
            gazePositionTarget.layer = calibrationLayer;
            gazePositionTarget.SetActive(false);

            //ValidationView
            anchorValidationView = Instantiate(Resources.Load("SMIValidationView", typeof(GameObject)), transform.position, Quaternion.identity) as GameObject;
            anchorValidationView.name = "SMIValidationView";
            //anchorValidationView.transform.parent = calibrationCamera.transform;
            anchorValidationView.transform.SetParent(calibrationCamera.transform); //MRD
            anchorValidationView.transform.localRotation = Quaternion.identity;
            anchorValidationView.layer = calibrationLayer;
            
            TextViewValidation = anchorValidationView.GetComponentInChildren<SMITextView>();
            anchorValidationView.SetActive(false);

            //GazeTargets to visualize the position of the Gaze in for the ValidationScreen 
            gazeTargetsOfQuantiativeValidation = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                gazeTargetsOfQuantiativeValidation[i] = Instantiate(Resources.Load("SMIGazeTarget_Validation", typeof(GameObject)), transform.position, Quaternion.identity) as GameObject;
                gazeTargetsOfQuantiativeValidation[i].transform.parent = anchorValidationView.transform;
                gazeTargetsOfQuantiativeValidation[i].name = "SMIGazeTarget_Validation";
                gazeTargetsOfQuantiativeValidation[i].SetActive(false);
                gazeTargetsOfQuantiativeValidation[i].layer = calibrationLayer;
            }
        }

        /// <summary>
        /// Set the Position of the Target to the next position of the targetPositionArray or finish the Calibrationview after the last Point
        /// </summary>
        private void smi_UpdateTargetDestination()
        {

            SMIGazeController.SMIcWrapper.smi_startDetectingNewFixation();

            //CalibrationState
            if (stateOfTheCalibrationView.Equals(VisualisationState.calibration) && (targetID < targetPositions.Length))
            {
                job.AcceptCalibrationPoint(targetPositions[targetID]);
                ++targetID;
            }

            //ValidationState
            else if (stateOfTheCalibrationView.Equals(VisualisationState.quantitativeValidation) && (targetID < targetPositions.Length))
            {
                positionsOfPORForQuantiativeValidation[targetID] = SMIGazeController.Instance.smi_getSample().por;
                smi_SaveAngleBetweenPORAndTarget(targetPositions[targetID]);

                ++targetID;
            }

            if (targetID == targetPositions.Length)
            {
                //Calibration
                if (stateOfTheCalibrationView.Equals(VisualisationState.calibration))
                {
                    smi_FinishCalibration();
                }

                //Validation
                else
                {
                    smi_FinishValidation(false);
                }
            }
        }


        /// <summary>
        /// Calculate the Angle between the POR and the 
        /// </summary>
        private void smi_SaveAngleBetweenPORAndTarget(Vector2 targetPosition)
        {
            Vector2 position = SMIGazeController.Instance.smi_getSample().por;
            positionsOfPORForQuantiativeValidation[targetID] = position;
            validationItems[targetID] = Mathf.Sqrt(Mathf.Pow(position.x - targetPosition.x, 2) + Mathf.Pow(position.y - targetPosition.y, 2)) * (float)(1f / 18f);
        }

        /// <summary>
        /// Finish the Validation Screen: 
        /// - Close the Grid Validation
        /// - Opens the Final State of the Quantitative Validation
        /// </summary>
        public void smi_FinishValidation(bool startNewVisualization)
        {
            //Grid Validation: Instead Quit the View
            if (stateOfTheCalibrationView.Equals(VisualisationState.gridValidation))
            {
                gazePositionTarget.SetActive(false);
                anchorValidationGrid.SetActive(false);

                stateOfTheCalibrationView = VisualisationState.None;

                if (pauseGameWhileCalibration)
                {
                    Time.timeScale = 1;
                }
                SMICreateBackgroundCamera.Instance.DestroyBackgroundCamera(startNewVisualization);
            }

            else if (stateOfTheCalibrationView.Equals(VisualisationState.quantitativeValidation))
            {
                stateOfTheCalibrationView = VisualisationState.resultsOfQuantitativeValidation;
            }
        }

        /// <summary>
        /// Close the calibrationview
        /// </summary>
        public void smi_FinishCalibration()
        {
            calibrationTarget.SetActive(false);
            gazePositionTarget.SetActive(false);
            anchorValidationGrid.SetActive(false);

            targetID = 0;
            stateOfTheCalibrationView = VisualisationState.None;

            movementAnimationDone = false;

            try
            {
                calibrationThread.Join();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }


            if (pauseGameWhileCalibration)
            {
                Time.timeScale = 1;
            }
            SMICreateBackgroundCamera.Instance.DestroyBackgroundCamera(false);
        }

        private Vector3 smi_CalculatePositionOnGazeMappingPlane(Vector2 Position, float planeDistForMapping)
        {
            Matrix4x4 localToWorldMatrixCamera = rayCam.gameObject.transform.localToWorldMatrix;
            Matrix4x4 playerTransformMatrix = Matrix4x4.identity;

            Vector3 porAverageGaze = Position;
            Vector3 cameraPor3d = smi_TransformGazePositionToWorldPosition(porAverageGaze, planeDistForMapping);

            //Position of the GazePos
            Vector3 instancePosition = playerTransformMatrix.MultiplyPoint(localToWorldMatrixCamera.MultiplyPoint(cameraPor3d));

            return instancePosition;
        }

        /// <summary>
        /// Map the gazeposition on a virtual Screen Plane.
        /// </summary>
        /// <param name="gazePos">Position of the gazepoint</param>
        /// <param name="planeDistForMapping">Distance of the virtual Screen to the Player </param>
        /// <returns></returns>
        private Vector3 smi_TransformGazePositionToWorldPosition(Vector2 gazePos, float planeDistForMapping)
        {

            float gazeScreenWidth = 1920f;
            float gazeScreenHeight = 1080f;
            float horizFieldOfView = 87f * Mathf.Deg2Rad;
            float vertFieldOfView = horizFieldOfView;

            float xOff = planeDistForMapping * Mathf.Tan(horizFieldOfView / 2f);
            float yOff = planeDistForMapping * Mathf.Tan(vertFieldOfView / 2f);
            float zOff = planeDistForMapping;

            Vector3 gazePosInWorldSpace = new Vector3(smi_CalculateGazeOffset(gazePos.x, gazeScreenWidth, xOff), -smi_CalculateGazeOffset(gazePos.y, gazeScreenHeight, yOff), zOff);

            return gazePosInWorldSpace;
        }

        private float smi_CalculateGazeOffset(float xin, float gazeScreenWidth, float offset)
        {
            return (xin * 2f * offset) / gazeScreenWidth - offset;
        }

        /// <summary>
        /// Prints the final Accuracy in the TextView
        /// </summary>
        private void smi_ShowValidationText()
        {
            float Accuracy = 0f;

            for (int i = 0; i < validationItems.Length; i++)
            {
                Accuracy += validationItems[i];
            }

            Accuracy /= validationItems.Length;
            TextViewValidation.SetText("Average Accuracy: " + System.Math.Round(Accuracy, 3) + "°");
        }

        IEnumerator WaitForCalibration()
        {
            //Start Blurr
            yield return new WaitForSeconds(123f);
            // Start Calibration
        }

        private void ShowValidationResults()
        {
            anchorValidationView.SetActive(true);
            calibrationTarget.SetActive(false);
            gazePositionTarget.SetActive(false);

            smi_ShowValidationText();

            for (int i = 0; i < gazeTargetsOfQuantiativeValidation.Length; i++)
            {
                gazeTargetsOfQuantiativeValidation[i].transform.position = smi_CalculatePositionOnGazeMappingPlane(positionsOfPORForQuantiativeValidation[i], gazeMappingDistanceValidation);
                gazeTargetsOfQuantiativeValidation[i].SetActive(true);
            }

            anchorValidationView.SetActive(true);
            TextViewValidation.SetTextVisible(true);

            timeright -= Time.deltaTime;
            if (timeright < 0)
            {
                stateOfTheCalibrationView = VisualisationState.closeResults;
            }
        }

        private void CloseValidationResults()
        {
            targetID = 0;

            anchorValidationView.SetActive(false);
            TextViewValidation.SetTextVisible(false);

            //Remove the Targets from the Scene
            for (int i = 0; i < gazeTargetsOfQuantiativeValidation.Length; i++)
            {
                gazeTargetsOfQuantiativeValidation[i].SetActive(false);
            }
            timeright = 5.0f;
            if (pauseGameWhileCalibration)
            {
                Time.timeScale = 1;
            }
            stateOfTheCalibrationView = VisualisationState.None;
            SMICreateBackgroundCamera.Instance.DestroyBackgroundCamera(false);
        }

        public static SMICalibrationVisualizer Instance
        {
            get
            {
                if (!instance)
                {
                    instance = (SMICalibrationVisualizer)FindObjectOfType(typeof(SMICalibrationVisualizer));

                    if (!instance)
                    {
                        GameObject gameObject = new GameObject();
                        gameObject.name = "SMICalibrationVisualizer";
                        instance = gameObject.AddComponent(typeof(SMICalibrationVisualizer)) as SMICalibrationVisualizer;
                    }
                }
                return instance;
            }
        }
    }
        #endregion

    /// <summary>
    /// Task for the CalibrationThread
    /// </summary>
    public class CalibrationJob
    {
        /// <summary>
        /// Start the Calibrationmode of the System; Note that this Thread will be bocked from the Server and waits for the Selected TargetCount (AcceptCalibrationPoint)
        /// </summary>
        public void DoWork()
        {
            SMIGazeController.SMIcWrapper.smi_startDetectingNewFixation();
            SMIGazeController.SMIcWrapper.smi_calibrate();
        }
        
        /// <summary>
        /// Accept manually the current target
        /// </summary>
        public void AcceptCalibrationPoint(Vector2 targetPos)
        {
            SMI.SMIGazeController.SMIcWrapper.smi_Vec2d targetPoint = new SMIGazeController.SMIcWrapper.smi_Vec2d();
            targetPoint.x = (double)targetPos.x;
            targetPoint.y = (double)targetPos.y;

            SMIGazeController.SMIcWrapper.smi_acceptCalibrationPoint(targetPoint);
        }

        /// <summary>
        /// Stop of the Calibration. Stops the calibrationMode of the server and reset the calibration
        /// </summary>
        public void RequestStop()
        {
            SMIGazeController.SMIcWrapper.smi_AbortCalibration();
        }
    }
}