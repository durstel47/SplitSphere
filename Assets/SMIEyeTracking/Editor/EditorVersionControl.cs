
// -----------------------------------------------------------------------
//
// (c) Copyright 1997-2016, SensoMotoric Instruments GmbH
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
using UnityEditor;
using System; 


/// <summary>
/// Check the Version of the Editor and Show a Menu to download the 32 Bit Version
/// </summary>
[InitializeOnLoad]
public class EditorVersionControl {

    static EditorVersionControl ()
    {
     //Check the used Unity Engine 
#if !(UNITY_5_4)
        showWarningWindowWrongUnityVersion();
#endif
        CreateLayer();
    }

    private static void showWarningWindowWrongUnityVersion()
    {

        bool openWebpage = EditorUtility.DisplayDialog("Older Version of Unity detected!", "The SMI-Eye Tracking SDK only supports Unity 5_4. Do you want to download the newest Version of the Unity Engine?", "Download the newest Version", "Skip Download");

        if (openWebpage)
        {
            System.Diagnostics.Process.Start("http://unity3d.com/get-unity/download?ref=personal");
            EditorApplication.Exit(0);
        }
    }

    //creates a new layer
    static void CreateLayer(){
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty it = tagManager.GetIterator();

        bool showChildren = true;

        while (it.NextVisible(showChildren))
        {

                //Set the Layer30 as CalibrationView
                if (it.name == "User Layer 30")
                {
                    it.stringValue = "CalibrationView";
                }
        }
        tagManager.ApplyModifiedProperties();
    }

}