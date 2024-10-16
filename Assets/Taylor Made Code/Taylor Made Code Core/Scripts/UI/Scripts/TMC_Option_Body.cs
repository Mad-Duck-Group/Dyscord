#if UNITY_EDITOR

using System.Linq;
using TaylorMadeCode.Core.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaylorMadeCode.Core
{
    public class TMC_Option_Body : TMC_UXML
    {
        //- Required Classes to Override -//
        /// <summary>
        /// This is the VERY ROOT OF OPTION BODY
        /// </summary>
        public override VisualElement m_RootUnity { get; set; }

        /// <summary>
        /// This is the visual element that should be treated as the parent for any child elements
        /// </summary>
        public override TMC_Tree_Node m_RootTmcEditor { get; set; }

        protected override sealed TMC_IObject m_ScriptTmcMono { get; set; }

        /// <summary>
        /// This is actual link to the node that this TMC_UXML is located in.
        /// </summary>
        public override TMC_Tree_Node m_RootTmcTree { get; set; }

        /// <summary>
        /// Override function for the array accessor
        /// </summary>
        /// <param name="key">index to access the child at</param>
        /// <returns>the child element</returns>
        public override TMC_Tree_Node this[int key]
        {
            get => m_RootTmcTree.m_ListOfChildren[key];
            //get => null;
        }

        /// <summary>
        /// method to return if the TMC_OPTION_BODY is hiding or displaying content.
        /// </summary>
        /// <returns>Returns true hiding content and false if displaying content </returns>
        public override System.Object GetValue()
        {
            return (OptionHeaderButton.style.rotate == new Rotate(90));
        }

        //- End of overridden classes -//

        public bool ForcedOption = false;

        public bool OneExclusiveMustBeSelected = false;
        public bool StartSelected = false;

        //Does this Option Require some other options to be turned off?
        public bool IsOptionMutuallyExclusive = false;

        //If the above is true the below is the name of the bodys that it cannot be run with.
        public string MutallyExclusiveOptionsNames = "";

        //The Separator Char for the above string
        public char MutallyExclusiveStringSeporatorChar = '|';

        public string HeaderText { get; set; }

        public Label OptionHeaderLabel { private set; get; }
        public Button OptionHeaderButton { private set; get; }
        public VisualElement OptionBody { private set; get; }
        public VisualElement EntireElement;

        private StyleLength OptionBodyStartingLength;

        public override TMC_UXML Instantiate(ref TMC_Tree_Node a_parent, ref TMC_Tree_Node a_self, params object[] a_additionalParameters)
        {
            if (a_additionalParameters.Count() > 0)
            {
                if (a_additionalParameters[0] is TMC_IObject)
                    m_ScriptTmcMono = a_additionalParameters[0] as TMC_IObject;
                else
                    Debug.LogError("Fatal error in TMC_Title Code. This issue will prevent any feedback from the UI systems");
            }

            VisualElement newVisualElement;
            TMC_Editor.LoadUXML(TMC_Editor_Utils.DIR_TMC_OPTION_BODY_UXML, ref a_parent, TMC_Tree_Node.e_TypeOfUI.TMC_OPTION_BODY, out newVisualElement);
            TMC_Editor.LoadAndSetUSS(TMC_Editor_Utils.DIR_TMC_OPTION_BODY_USS, newVisualElement);

            m_RootUnity = newVisualElement;

            m_RootTmcEditor = new TMC_Tree_Node(m_RootUnity.Q<VisualElement>("OptionBody"), TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT);
            m_RootTmcEditor.m_Parent = a_parent;
            m_RootTmcEditor.mb_IsRootTMCEditor = true;
            m_RootTmcEditor.m_TmcUxmlClass = this;

            m_RootTmcTree = a_self;

            //- Custom Setup Start -//
            OptionHeaderLabel = m_RootUnity.Q<Label>("HeaderLabel");
            OptionHeaderButton = m_RootUnity.Q<Button>("DropDownButton");
            OptionBody = m_RootUnity.Q<VisualElement>("OptionBody");
            EntireElement = m_RootUnity.Q<VisualElement>("EntireElement");

            TMC_Editor.MatchStyleToUnityEditor(m_RootUnity.Q<VisualElement>("EntireElement"));
            TMC_Editor.MatchStyleToUnityEditor(m_RootUnity.Q<VisualElement>("OptionHeader"));
            TMC_Editor.MatchStyleToUnityEditor(OptionHeaderButton);

            OptionHeaderButton.clicked += BodyShowOrMinimize;

            //- Custom Setup End -//

            return this;
        }

        /// <summary>
        /// This function is called once a new end of creation a new function.
        /// </summary>
        public override void EndOfCreationFunction()
        {
            OptionHeaderLabel.text = HeaderText;
        }

        /// <summary>
        /// This function is called when TMC_OutParent is called and allows for any required information to be accessed and saved for TMC_UXML classed that surround Other Elements.
        /// </summary>
        public override void ParentOutFunction()
        {
            //This function saved the total length of the UI once its been created. This allows the minimize animation.
            OptionBodyStartingLength = OptionBody.resolvedStyle.height;
        }

        /// <summary>
        /// This function will hide the child elements with a smooth animation.
        /// </summary>
        public void BodyShowOrMinimize()
        {
            //Hide the body
            if (OptionHeaderButton.style.rotate == new Rotate(90))
                Minimised();
            else
                Maximised();

            //Only need to set editor system when the Editor is active. This ensures that this function can be use for all elements including editor and game scripts
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false && m_ScriptTmcMono.m_UIChoices.ContainsKey(HeaderText + "_Body_IsMaxamised") && m_ScriptTmcMono.m_UIChoices[HeaderText + "_Body_IsMaxamised"] != false)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
#endif
        }

        /// <summary>
        /// This function hides the OptionBody that this is called from
        /// </summary>
        /// <param name="UseProvidedState">Should toggle state or set a state</param>
        /// <param name="ShouldElementBeVisible">If Provided state is true then this is the state the GUI will be set to</param>
        public void HideEntireOptionBody(bool UseProvidedState = false, bool ShouldElementBeVisible = false)
        {
            //invert the visibility of the object
            if (UseProvidedState)
            {
                if (ShouldElementBeVisible)
                    m_RootUnity.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                else
                    m_RootUnity.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
            else
            {
                //Toggle them back and forth from current state if not hard values are provided
                m_RootUnity.style.display = (m_RootUnity.style.display == DisplayStyle.Flex) ? new StyleEnum<DisplayStyle>(DisplayStyle.None) : new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
        }

        //TODO: Function def
        public void ToggleOptionBodyState(VisualElement parent, bool UseProvidedState, bool ShouldElementBeVisible, bool firstElement = true)
        {
            if (UseProvidedState)
            {
                if (ShouldElementBeVisible)
                    Maximised();
                else
                    Minimised();
            }
            else
            {
                if (parent.style.display != DisplayStyle.None)
                    Minimised();
                else
                    Maximised();
            }
        }

        private void Minimised()
        {
            OptionHeaderButton.style.rotate = new Rotate(0);
            EntireElement.AddToClassList("MinState");
            EntireElement.RemoveFromClassList("MaxState");

            TMC.GurantieeSetDictonary(m_ScriptTmcMono.m_UIChoices, HeaderText + "_Body_IsMaxamised", false);
        }

        private void Maximised()
        {
            OptionHeaderButton.style.rotate = new Rotate(90);
            EntireElement.AddToClassList("MaxState");
            EntireElement.RemoveFromClassList("MinState");

            TMC.GurantieeSetDictonary(m_ScriptTmcMono.m_UIChoices, HeaderText + "_Body_IsMaxamised", true);
        }

        public override void OnBeforeSerialize()
        {
        }

        public override void OnAfterDeSerialize()
        {
            if (TMC.GurantieeGetDictonary(m_ScriptTmcMono.m_UIChoices, (HeaderText + "_Body_IsMaxamised"), false))
                Maximised();
            else
                Minimised();
        }

        public override void OnReset()
        {
            Minimised();
        }
    }
}

#endif