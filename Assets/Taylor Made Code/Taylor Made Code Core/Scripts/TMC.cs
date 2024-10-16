namespace TaylorMadeCode.Core
{
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    #region Useful Misc Var / Functions

    //- TMC CLASSES -//
    public static class TMC
    {
        //- TMC ENUMS -//
        public enum WhenToStart
        {
            DontStartAutomatically = 0,
            Start = 1,
            Awake = 2,
            OnEnable = 3,
            OnDisable = 4
        }

        public enum ToToggle
        {
            Null = -1,
            ToggleOff = 0,
            ToggleOn = 1
        }

        public enum ProductCatogory
        {
            NONE = -1,
            Free = 0,
            Normal = 1,
            Pro = 2,
            System = 3,
        }

        public enum e_RenderPipelineVersions
        {
            NONE = 0,
            URP = 1,
            HDR = 2,
            STANDARD = 3
        }

        public static bool ON = true;
        public static bool OFF = false;

        public static List<GameObject> GetAllObjectsThatIsMasked(LayerMask mask)
        {
            //Create List
            List<GameObject> InLayer = new List<GameObject>();

            //Get All Transform Components (Only mandatory component so will get all Objects)
            Transform[] transforms = GameObject.FindObjectsOfType<Transform>();

            // if the Objects are in the layer add to InLayer List
            foreach (Transform transform in transforms)
            {
                // move the mask binary over by what layer and then binary add the mask then check
                if (mask == (mask | (1 << transform.gameObject.layer)))
                {
                    InLayer.Add(transform.gameObject);
                }
            }

            //Return whats in the layer
            return InLayer;
        }

        public static bool AreObjectsColliding(GameObject A, GameObject B)
        {
            //ToDo: See if this can be improved
            //Checks if there is some type of collider attached to the object if not then print an error
            if (A.GetComponent<Collider>() == null || B.GetComponent<Collider>() == null)
                Debug.LogError("Please Ensure both " + A.name + " and " + B.name + "Have a collider Attached");

            //POSSIBLE_ISSUE: This only returns true if the Mesh's Intercepts. Possible issue with large meshes is colliding with small meshes
            // Check if Mesh intercedes and return the response
            return A.GetComponent<Collider>().bounds.Intersects(B.GetComponent<Collider>().bounds);
        }

        public static bool HasGUISystemBeenCreated()
        {
            if ((GameObject.Find("Canvas") != null) && (GameObject.Find("Event System") != null))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static GameObject CreateGUISystem()
        {
            GameObject l_Canvas = GameObject.Find("Canvas");
            if (l_Canvas == null)
            {
                l_Canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

                Canvas l_CanvasComponent = l_Canvas.GetComponent<Canvas>();
                l_CanvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                l_CanvasComponent.pixelPerfect = false;
                l_CanvasComponent.sortingOrder = 0;
                l_CanvasComponent.targetDisplay = 0;
                l_CanvasComponent.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

                CanvasScaler l_CanvasScalerComponent = l_Canvas.GetComponent<CanvasScaler>();
                l_CanvasScalerComponent.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                l_CanvasScalerComponent.referencePixelsPerUnit = 100;
                
                GraphicRaycaster l_GraphicsRaycasterComponent = l_Canvas.GetComponent<GraphicRaycaster>();
                l_GraphicsRaycasterComponent.ignoreReversedGraphics = true;
                l_GraphicsRaycasterComponent.blockingObjects = GraphicRaycaster.BlockingObjects.None;
                l_GraphicsRaycasterComponent.blockingMask = LayerMask.NameToLayer("Everything");

                //POSSIBLE_ISSUE: This might need to be dynamically changed by unity settings (Need to do more research and understand both Unity UI and Event Systems)
            }

            //- If Event System is already created don't re-create -//
            GameObject l_EventSystem = GameObject.Find("Event System");
            if (l_EventSystem == null)
            {
                l_EventSystem = new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            return l_Canvas;
        }

        public static Vector3 VectorTimesVector(Vector3 One, Vector3 Two)
        {
            return new Vector3(One.x * Two.x, One.y * Two.y, One.z * Two.z);
        }

        public static Vector2 VectorTimesVector(Vector2 One, Vector2 Two)
        {
            return new Vector2(One.x * Two.x, One.y * Two.y);
        }

        public static bool ToBool(string input)
        {
            return (input == true.ToString());
        }

        public static void GurantieeSetDictonary<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey Key, TValue Value)
        {
            dict ??= new Dictionary<TKey, TValue>();
            dict[Key] = Value;
        }

        public static TValue GurantieeGetDictonary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            dictionary ??= new Dictionary<TKey, TValue>();
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }

        public static void OutputAllDictonaryToLog<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            string tempMessage = "";
            foreach (KeyValuePair<TKey, TValue> t in dictionary)
                tempMessage += (t.Key + " " + t.Value + " ");
                    
            Debug.Log(tempMessage);
        }
    }

    #endregion Useful Misc Var / Functions
}