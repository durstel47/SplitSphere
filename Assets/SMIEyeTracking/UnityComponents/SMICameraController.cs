
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
using UnityStandardAssets.ImageEffects;
using System.Collections;
using SMI;
using VR = UnityEngine.VR;

[RequireComponent(typeof(UnityStandardAssets.ImageEffects.ColorCorrectionCurves))]
[RequireComponent(typeof(UnityStandardAssets.ImageEffects.DepthOfField))]
[RequireComponent(typeof(Camera))]
public class SMICameraController : MonoBehaviour {

    private Camera cameraComponent;

    private float fadeTime = 0.1f;
    
    private ColorCorrectionCurves colorCurves;
    private DepthOfField depthOfField;

    [HideInInspector]
    public Color destinationCalibrationColorAt1 = Color.white;

    [HideInInspector]
    public Color destinationCalibrationColorAt0 = Color.black;

    [HideInInspector]
    public float backgroundAlpha = 1.0f;

    private float initValueFocalDistance = 11.5f;
    private float initValueFocalSize = 1f;
    private float initValueAperture = 0f;

    [HideInInspector]
    public float dofFrom = 0f;
    [HideInInspector]
    public float dofTo = 0f;

    /// <summary>
    /// Set the static Value for the "Field of View" 
    /// </summary>
    void Start ()
    {
          
    }

    /// <summary>
    /// Initialize the objects
    /// </summary>
    void Awake()
    {
        colorCurves = GetComponent<ColorCorrectionCurves>();
        colorCurves.enabled = true;

        depthOfField = GetComponent<DepthOfField>();
        depthOfField.focalSize = initValueFocalSize;
        depthOfField.focalLength = initValueFocalSize;
        depthOfField.aperture = initValueAperture;
    }
	
    /// <summary>
    /// Set the static Value for the "Field of View" and adjust the Colorcurves
    /// </summary>
	void Update () {
        Keyframe redKeyAt1 = (Keyframe)colorCurves.redChannel.keys.GetValue(1);
        Keyframe blueKeyAt1 = (Keyframe)colorCurves.blueChannel.keys.GetValue(1);
        Keyframe greenKeyAt1 = (Keyframe)colorCurves.greenChannel.keys.GetValue(1);

        colorCurves.redChannel.MoveKey(1, new Keyframe(1, Mathf.Lerp(redKeyAt1.value, destinationCalibrationColorAt1.r, fadeTime)));
        colorCurves.blueChannel.MoveKey(1, new Keyframe(1, Mathf.Lerp(blueKeyAt1.value, destinationCalibrationColorAt1.b, fadeTime)));
        colorCurves.greenChannel.MoveKey(1, new Keyframe(1, Mathf.Lerp(greenKeyAt1.value, destinationCalibrationColorAt1.g, fadeTime)));
        
        Keyframe redKeyAt0 = (Keyframe)colorCurves.redChannel.keys.GetValue(0);
        Keyframe blueKeyAt0 = (Keyframe)colorCurves.blueChannel.keys.GetValue(0);
        Keyframe greenKeyAt0 = (Keyframe)colorCurves.greenChannel.keys.GetValue(0);

        colorCurves.redChannel.MoveKey(0, new Keyframe(0, Mathf.Lerp(redKeyAt0.value, destinationCalibrationColorAt0.r * backgroundAlpha, fadeTime)));
        colorCurves.blueChannel.MoveKey(0, new Keyframe(0, Mathf.Lerp(blueKeyAt0.value, destinationCalibrationColorAt0.b * backgroundAlpha, fadeTime)));
        colorCurves.greenChannel.MoveKey(0, new Keyframe(0, Mathf.Lerp(greenKeyAt0.value, destinationCalibrationColorAt0.g * backgroundAlpha, fadeTime)));
        colorCurves.UpdateParameters();

        depthOfField.aperture = Mathf.Lerp(dofFrom, dofTo, 1f);
    }
}
