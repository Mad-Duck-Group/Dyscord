namespace TaylorMadeCode.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;
    using TaylorMadeCode.Core.Utilities;

    [System.Serializable]
    public abstract class TMC_ScriptableObject : ScriptableObject, TMC_IObject, ISerializationCallbackReceiver
    {
 //--------------------- Required Variables ----------------------//
        /// <summary> /// Boolean to Track if the Script Has Been Setup /// </summary>
        public bool mb_HasBeenSetup = false;
        bool TMC_IObject.mb_HasBeenSetup { get => mb_HasBeenSetup; set => mb_HasBeenSetup = value; }

        // TMC Auto Start possibilities
        public TMC.WhenToStart me_StartScriptOn = TMC.WhenToStart.DontStartAutomatically;

        /// <summary>
        /// Script Starting events allows the client to call their code before our script runs. So if they want to alter the script setting based off difficulty level then they can.
        /// </summary>
        public UnityEvent m_ScriptStartingEvents;
        
        /// <summary>
        /// Script ending events allows client to call their code once ours is finished. For example once a Patrolling AI guard is finished its loop
        /// </summary>
        public UnityEvent m_ScriptEndingEvents;
        
        /// <summary>
        /// Script During events allow the client to call their code while ours runs. For example for a Patrolling AI Guard reaches a main control node on its loop
        /// </summary>
        public UnityEvent m_ScriptDuringEvents;

        /// <summary>
        /// Dictionary that is used to access the settings set in the custom TMC UI System
        /// </summary>
        public Dictionary<string, bool> m_UIChoices;
        Dictionary<string, bool> TMC_IObject.m_UIChoices { get => m_UIChoices; set => m_UIChoices = value; }

        /// <summary>
        /// A string that gets Json saved to it to allow the Custom UI state to be saved
        /// This is used in editor and play mode and once in a final executable file
        /// </summary>
        public string ms_UIJson = "";
        string TMC_IObject.ms_UIJson { get => ms_UIJson; set => ms_UIJson = value; }

#if UNITY_EDITOR

        /// <summary>
        /// Root of the Custom Script UI Tree
        /// </summary>
        public TMC_Tree_Node m_UIRoot;
        TMC_Tree_Node TMC_IObject.m_UIRoot { get => m_UIRoot; set => m_UIRoot = value; }
        
#endif //UNITY_EDITOR

        //------------------------- Function Definitions -----------------------//

        /// <summary>
        /// Constructor of the TMC_Mono, Declares m_UIChoices and all Starting, during, ending unity events var
        /// </summary>
        protected TMC_ScriptableObject()
        {
            m_UIChoices = new Dictionary<string, bool>();
            m_ScriptStartingEvents = new UnityEvent();
            m_ScriptEndingEvents = new UnityEvent();
            m_ScriptDuringEvents = new UnityEvent();
        }

        /// <summary>
        /// Default implementation of Start function from unity 
        /// </summary>
        protected void Start()
        {
            if (me_StartScriptOn == TMC.WhenToStart.Start)
                StartFunction();
        }

        /// <summary>
        /// Default implementation of Awake function from unity 
        /// </summary>
        protected void Awake()
        {
            if (me_StartScriptOn == TMC.WhenToStart.Awake)
                StartFunction();
        }

        /// <summary>
        /// Default implementation of OnEnable function from unity 
        /// </summary>
        protected void OnEnable()
        {
            if (me_StartScriptOn == TMC.WhenToStart.OnEnable)
                StartFunction();
        }

        /// <summary>
        /// Default implementation of OnDisable function from unity 
        /// </summary>
        protected void OnDisable()
        {
            if (me_StartScriptOn == TMC.WhenToStart.OnDisable)
                StartFunction();
        }

        /// <summary>
        /// Default Implementation of the OnDestroy Unity Function, Also added this function to be under TMC Remove Script in the context menu
        /// The TMC Remove Script Context menu is a delete method that handles everything related to the Script unlike the standard remove that will only remove the singular script and not any extra elements.
        /// </summary>
        [ContextMenu("TMC Remove Script")]
        protected void OnDestroy()
        {
            RemoveScript();
        }

        /// <summary>
        /// A function that needs to be overridden,
        /// this function should contain any code that is needed to be called to start the script. When not used it should be declared
        /// </summary>
        public virtual void StartFunction()
        {
            Debug.Log("Start Function not Implemented, its worth removing the call to StartFunction ");
        }
        
        /// <summary>
        /// This is the default implementation of Setup Script function,
        /// Basic function to allow specific setup code for each script. Not an abstract class as then you are forced to override for every script. However this isnt the best practice
        /// to do when every tmc script should be allowed to work with it. Thus virtual function is better.
        /// </summary>
        public virtual void SetupScript()
        {
            mb_HasBeenSetup = true;
        }
        
        /// <summary>
        /// This is the default implementation of the RemoveScript function.
        /// Basic function to allow specific setup code for each script. Not an abstract class as then you are forced to override for every script. However this isnt the best practice
        /// to do when every tmc script should be allowed to work with it. Thus virtual function is better.
        /// </summary>
        public virtual void RemoveScript()
        {
            mb_HasBeenSetup = false;
        }

        /// <summary>
        /// Reset Function that gets called when reset script is called, Below just handles the reset for the UI side.
        /// </summary>
        public virtual void Reset()
        {
            //Clear the dictionary the contains all the UI states
            m_UIChoices.Clear();

#if UNITY_EDITOR
            //Is there a custom editor?
            if (m_UIRoot != null)
            {
                //Try and get the two common parent elements.
                m_UIRoot.MarkDirty();
                
                m_UIRoot.CallOnResetForAllChildren();
                m_UIRoot.OnBeforeSerializeChildrenRecursively();
            }
#endif
            //Tell the scripts its no longer setup
            mb_HasBeenSetup = false;
        }

        /// <summary>
        /// This function takes in the state of the GUI and saves it to a string to be saved by unity.
        /// </summary>
        public void OnBeforeSerialize()
        {
            List<string> m_StringEffedDictionary = new List<string>();
            for (int mi_i = 0; mi_i < m_UIChoices.Count(); mi_i++)
                m_StringEffedDictionary.Add(m_UIChoices.Keys.ElementAt(mi_i) + "7C303332317C7C3036323232317C" + m_UIChoices.Values.ElementAt(mi_i));

            ms_UIJson = JsonUtility.ToJson(new JsonableListWrapper<string>(m_StringEffedDictionary));
        }

        /// <summary>
        /// Takes the string saved by unity and built by the Serialize function and coverts it back to the Dictionary Element
        /// </summary>
        public void OnAfterDeserialize()
        {
            //If the serialized elements aren't located then don't run the code and exit early.
            if (ms_UIJson == "" || JsonUtility.FromJson<JsonableListWrapper<string>>(ms_UIJson).m_list == null)
                return;

            m_UIChoices = new Dictionary<string, bool>();

            List<string> m_StringEffedDictionary = JsonUtility.FromJson<JsonableListWrapper<string>>(ms_UIJson).m_list;
            for (int i = 0; i < m_StringEffedDictionary.Count; i++)
            {
                string[] m_var = m_StringEffedDictionary[i].Split("7C303332317C7C3036323232317C");
                m_UIChoices.Add(m_var[0], TMC.ToBool(m_var[1]));
            }
        }
    }
}