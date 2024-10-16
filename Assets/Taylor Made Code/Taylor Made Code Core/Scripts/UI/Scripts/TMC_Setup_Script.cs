#if UNITY_EDITOR

using System.Collections;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using TaylorMadeCode.Core.Utilities;

namespace TaylorMadeCode.Core
{
    public class TMC_Setup_Script : TMC_UXML
    {
        /// <summary>
        /// This is the VERY ROOT OF OPTION BODY
        /// </summary>
        public override VisualElement m_RootUnity { get; set; }

        /// <summary>
        /// This is the visual element that should be treated as the parent for any child elements
        /// </summary>
        public override TMC_Tree_Node m_RootTmcEditor { get; set; }

        /// <summary>
        /// This is a connection back to the TMC_IObject derived class that allows the UI to set the required settings of the script
        /// </summary>
        protected override sealed TMC_IObject m_ScriptTmcMono { get; set; }

        /// <summary>
        /// This is actual link to the node that this TMC_UXML is located in.
        /// </summary>
        public override TMC_Tree_Node m_RootTmcTree { get; set; }

        /// <summary>
        /// Override function for the array accessors
        /// </summary>
        /// <param name="key">index to access the child at</param>
        /// <returns>the child element</returns>
        public override TMC_Tree_Node this[int key]
        {
            get => m_RootTmcTree.m_ListOfChildren[key];
        }

        /// <summary>
        /// A function to check the current state of the object.
        /// </summary>
        /// <returns>Returns true if not setup and false if script is setup </returns>
        public override System.Object GetValue()
        {
            return (m_RootUnity.Q("Title").style.display == DisplayStyle.None &&
                    m_RootUnity.Q("Body").style.display == DisplayStyle.None &&
                    m_RootTmcTree.m_ListOfChildren[0].GetUnityVisualElement().style.display == DisplayStyle.None);
        }

        public VisualElement EntireElement;
        public Button m_SetupButton;
        public string HeaderText = "";
        public string DescriptionText = "";

        /// <summary>
        /// This is the function to place instantiation code
        /// </summary>
        /// <param name="a_parent">The parent object of the TMC_UXML element</param>
        /// <param name="a_self">The TMC UI Treenode that this class uses</param>
        /// <param name="a_additionalParameters">Additional Parameters that will change from Class to class to generic Params Objects[] is used</param>
        /// <returns>The constructed TMC_UXML Class</returns>
        public override TMC_UXML Instantiate(ref TMC_Tree_Node a_parent, ref TMC_Tree_Node a_self, params object[] a_additionalParameters)
        {
            if (a_additionalParameters.Any())
            {
                if (a_additionalParameters[0] is TMC_IObject)
                    m_ScriptTmcMono = a_additionalParameters[0] as TMC_IObject;
                else
                    Debug.LogError("Fatal error in TMC_Title Code. This issue will prevent any feedback from the UI systems");
            }

            TMC_Editor.LoadUXML(TMC_Editor_Utils.DIR_TMC_SETUP_SCRIPT_UXML, ref a_parent, TMC_Tree_Node.e_TypeOfUI.TMC_SETUP_SCRIPT, out VisualElement l_NewVisualElement);
            TMC_Editor.LoadAndSetUSS(TMC_Editor_Utils.DIR_TMC_SETUP_SCRIPT_USS, l_NewVisualElement);

            m_RootUnity = l_NewVisualElement;
            
            m_RootTmcEditor = new TMC_Tree_Node(m_RootUnity.Q<VisualElement>("EntireBody"), TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT)
            {
                m_Parent = a_parent,
                mb_IsRootTMCEditor = false,
                m_TmcUxmlClass = this
            };

            //Ensure All elements are correctly style.
            TMC_Editor.MatchStyleToUnityEditorForAllChildren(m_RootUnity.Q<VisualElement>("EntireBody"));

            m_RootTmcTree = a_self;
            m_SetupButton = m_RootUnity.Q<Button>("Setup_Button");

            return this;
        }

        /// <summary>
        /// This function is called once a new end of creation a new function.
        /// </summary>
        public override void EndOfCreationFunction()
        {
            //Var setting Here.
            m_RootUnity.Q<Label>("Header").text = HeaderText;
            m_RootUnity.Q<Label>("SetupText").text = DescriptionText;
        }

        /// <summary>
        /// If parent functionality is use this function is called when Out_parent is called.
        /// </summary>
        public override void ParentOutFunction()
        {
            if (m_ScriptTmcMono.mb_HasBeenSetup == false)
            {
                //Ensure only one child element is attached to Setup Script.
                if (m_RootTmcTree.m_ListOfChildren.Count < 1)
                {
                    Debug.LogError("TMC_Setup_Script Requires a singular child. You have created " + m_RootTmcEditor.m_ListOfChildren.Count + " Please ensure there is only one child attached");
                    return;
                }

                //Hide the first child UI
                if (m_RootTmcTree.m_ListOfChildren.Any())
                    m_RootTmcTree.m_ListOfChildren[0].GetUnityVisualElement().style.display = DisplayStyle.None;
            }
            else
            {
                m_RootUnity.Q("Title").style.display = DisplayStyle.None;
                m_RootUnity.Q("Body").style.display = DisplayStyle.None;
            }
            
            if (m_SetupButton != null)
            {
                m_SetupButton.clickable.clicked += HandleUIChange;
                m_SetupButton.clickable.clicked += m_ScriptTmcMono.SetupScript;
            }
        }

        /// <summary>
        /// The function that is called when the UI Needs to be altered to show the Standard Script GUI
        /// </summary>
        protected void HandleUIChange()
        {
            m_RootTmcTree.m_ListOfChildren[0].GetUnityVisualElement().style.display = DisplayStyle.Flex;
            m_RootUnity.Q("Title").style.display = DisplayStyle.None;
            m_RootUnity.Q("Body").style.display = DisplayStyle.None;
            
            m_RootTmcEditor.MarkDirty();
        }

        /// <summary>
        /// This function is called before serialization, for Setup Script this isn't used
        /// </summary>
        public override void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// This function is called after serialization, for setup script this isn't used
        /// </summary>
        public override void OnAfterDeSerialize()
        {
        }

        /// <summary>
        /// This function is called when Reset of the script is occurring, it allows the GUI element to reset its own state
        /// </summary>
        public override void OnReset()
        { 
            m_RootTmcTree.m_ListOfChildren[0].GetUnityVisualElement().style.display = DisplayStyle.None;
            m_RootUnity.Q("Title").style.display = DisplayStyle.Flex;
            m_RootUnity.Q("Body").style.display = DisplayStyle.Flex;
        }
    }
}

#endif