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
using System.Threading;

namespace SMI
{
    public class SMICreateBackgroundCamera : MonoBehaviour
    {

        private GameObject smiBackgroundCamera;

        // Instance of the Gameobject
        private static SMICreateBackgroundCamera instance;

        void Awake()
        {
            if (!instance)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetUpBackgroundCamera()
        {
            if (smiBackgroundCamera == null)
            {
                smiBackgroundCamera = Instantiate(Resources.Load("smiBackgroundCamera"), transform.position, transform.rotation) as GameObject;

                smiBackgroundCamera.transform.parent = transform;
                smiBackgroundCamera.name = smiBackgroundCamera.name.Replace("(Clone)", "");
                smiBackgroundCamera.GetComponent<SMIUpdateCameraPositionAndRotation>().MainCamera = gameObject.GetComponent<Camera>();
            }
        }

        public void DestroyBackgroundCamera(bool startNewVisualization)
        {
            if (smiBackgroundCamera != null && !startNewVisualization)
            {
                Destroy(smiBackgroundCamera);
                smiBackgroundCamera = null;
                Thread.Sleep(15);
            }
        }
        
        /// <summary>
        /// Access to the singleton instance
        /// </summary>
        public static SMICreateBackgroundCamera Instance
        {
            get
            {
                if (!instance)
                {
                    instance = (SMICreateBackgroundCamera)FindObjectOfType(typeof(SMICreateBackgroundCamera));

                    if (!instance)
                    {
                        GameObject gameObject = new GameObject();
                        gameObject.name = "SMICreateBackgroundCamera";
                        instance = gameObject.AddComponent(typeof(SMICreateBackgroundCamera)) as SMICreateBackgroundCamera;
                    }
                }
                return instance;
            }
        }
    }
}