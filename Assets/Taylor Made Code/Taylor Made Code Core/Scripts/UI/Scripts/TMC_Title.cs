#if UNITY_EDITOR

namespace TaylorMadeCode.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;
    using UnityEngine;
    using UnityEngine.UIElements;

    using TaylorMadeCode.Core.PreloadedAssets;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Utilities;

    public class TMC_Title : TMC_UXML
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

        /// <summary> /// This is actual link to the node that this TMC_UXML is located in. /// </summary>
        public override TMC_Tree_Node m_RootTmcTree { get; set; }

        /// <summary>
        /// Override function for the array accessors
        /// </summary>
        /// <param name="key">index to access the child at</param>
        /// <returns>the child element</returns>
        public override TMC_Tree_Node this[int key] { get => m_RootTmcTree.m_ListOfChildren[key]; }

        /// <summary>
        /// method to return if the title is setup
        /// </summary>
        /// <returns>Returns if the Script has been setup </returns>
        public override System.Object GetValue()
        { return m_ScriptTmcMono.mb_HasBeenSetup; }

        //- End of overridden classes -//

        public VisualElement m_EntireElement;
        public Label m_Title;
        public Button m_DocsButton;
        public Button m_TutorialButton;
        public VisualElement m_Logo;
        public VisualElement m_OptionSelectorsArea;
        public VisualElement m_OptionBodySection;
        public string ms_HeaderText;

        private List<TMC_Tree_Node> m_CreatedOptionButtons;
        private string ms_DocsURL;
        private string ms_TutorialURL;

        /// <summary>
        /// Instantiating the TMC_Title Class
        /// </summary>
        /// <param name="a_parent">Parent in the TMC_Tree_Node</param>
        /// <param name="a_self">ref to the node that contains the TMC_Tree_Node</param>
        /// <param name="a_additionalParameters">Object array that allows additional params, this is different per type of TMC_UXML</param>
        /// <returns>Its self</returns>
        public override TMC_UXML Instantiate(ref TMC_Tree_Node a_parent, ref TMC_Tree_Node a_self, params object[] a_additionalParameters)
        {
            TMC_Editor.LoadUXML(TMC_Editor_Utils.DIR_TMC_TITLE_BAR_UXML, ref a_parent, TMC_Tree_Node.e_TypeOfUI.TMC_TITLE, out VisualElement l_newVisualElement);
            TMC_Editor.LoadAndSetUSS(TMC_Editor_Utils.DIR_TMC_TITLE_BAR_USS, l_newVisualElement);

            m_RootUnity = l_newVisualElement;

            m_RootTmcEditor = new TMC_Tree_Node(m_RootUnity.Q<VisualElement>("OptionBodySection"), TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT)
            {
                m_Parent = a_parent,
                mb_IsRootTMCEditor = false,
                m_TmcUxmlClass = this
            };

            m_RootTmcTree = a_self;

            //- Custom Setup Start -//
            m_DocsButton = m_RootUnity.Q<Button>("DocsButton");
            m_DocsButton.clicked += OpenDocumentation;

            m_TutorialButton = m_RootUnity.Q<Button>("TutorialButton");
            m_TutorialButton.clicked += OpenVideoTutorial;

            //- Attempt to Use Script ICON IN TOP LEft -//
            m_Logo = m_RootUnity.Q<VisualElement>("LeftBlock");

            m_Title = m_RootUnity.Q<Label>("Title");

            m_OptionSelectorsArea = m_RootUnity.Q<VisualElement>("OptionSelectors");

            //- Option Button Generation -//
            //This list is used in the future once parent section has finished.
            m_CreatedOptionButtons = new List<TMC_Tree_Node>();

            //If Statement to handle the specific Title Additional Params
            if (a_additionalParameters.Any() && a_additionalParameters[0] is TMC_IObject)
                m_ScriptTmcMono = a_additionalParameters[0] as TMC_IObject;
            else
                Debug.LogError("Fatal error in TMC_Title Code. This issue will prevent any feedback from the UI systems");

            if (a_additionalParameters[1] is object[])
            {
                object[] l_UserAdditionalParams = a_additionalParameters[1] as object[];
                
                if (l_UserAdditionalParams?.Length > -1 && l_UserAdditionalParams[0] is Texture2D)
                {
                    m_Logo.style.backgroundImage = l_UserAdditionalParams[0] as Texture2D;
                }
                else
                    m_Logo.style.backgroundImage = new StyleBackground(TMC_Editor_Assets.m_PreLoadedSpritesList[(int)TMC_Editor_Assets.e_PreLoadedImages.TMC_LOGO]);

                //Not sure why but passed in order style classes is 2nd e.g index 1 however during passing over it gets mixed up
                //Thus the reason for the weird order for indexes
                if (l_UserAdditionalParams?.Length > 0 && l_UserAdditionalParams[1] is string)
                {
                    m_Logo.styleSheets.Add(TMC_Editor_Assets.m_PreloadedStyleSheets[(int)TMC_Editor_Assets.e_PreMadeStyles.TMC_USS]);
                    m_Logo.AddToClassList(l_UserAdditionalParams[1] as string);
                }
                else
                    Debug.LogWarning("Error Styling for TMC_Title Will not correctly display");

                if (l_UserAdditionalParams?.Length > 1 && l_UserAdditionalParams[2] is string && l_UserAdditionalParams[2] as string != "")
                    ms_TutorialURL = l_UserAdditionalParams[2] as string;
                else
                {
                    ms_TutorialURL = "https://www.youtube.com/channel/UCDwdDA6eZ573BsnJJjK0QzQ";
                }

                if (l_UserAdditionalParams?.Length > 2 && l_UserAdditionalParams[3] is string && l_UserAdditionalParams[3] as string != "")
                    ms_DocsURL = l_UserAdditionalParams[3] as string;
                else
                {
                    ms_DocsURL = "https://www.taylormadecode.com/docs";
                }
            }

            //- Custom Setup End -//

            return this;
        }

        /// <summary>
        /// This function is called once a new end of creation a new function.
        /// </summary>
        public override void EndOfCreationFunction()
        {
            m_Title.text = ms_HeaderText;
        }

        /// <summary>
        /// If parent functionality is use this function is called when Out_parent is called.
        /// This function will scan all created children for TMC_Option_Body elements, Once found relevant data will be collected to create and link the buttons above.
        /// </summary>
        public override void ParentOutFunction()
        {
            bool lb_IsItFirstLoad = false;
            List<Tuple<Button, TMC_Option_Body>> l_DefaultOnElements = new List<Tuple<Button, TMC_Option_Body>>();
            
            foreach (TMC_Tree_Node ChildTreeNode in m_RootTmcTree.m_ListOfChildren)
            {
                if (ChildTreeNode.me_Type == TMC_Tree_Node.e_TypeOfUI.TMC_OPTION_BODY &&
                    ChildTreeNode.Get_Self_As_TMC_UXML<TMC_Option_Body>().ForcedOption == false)
                {
                    //-------------------------------------------------------------- Basic Option Body to button link-up -----------------------------------------------------------------------//
                    Button l_OptionButton = new Button();
                    TMC_Option_Body l_LinkedOptionBody = ChildTreeNode.Get_Self_As_TMC_UXML<TMC_Option_Body>();
                    TMC_Tree_Node l_OptionButtonTreeNode = new TMC_Tree_Node(l_OptionButton, TMC_Tree_Node.e_TypeOfUI.UNITY_BUTTON);

                    m_CreatedOptionButtons.Add(l_OptionButtonTreeNode);

                    //Set the created style and set button to not used
                    TMC_Editor.LoadAndSetUSS(TMC_Editor_Utils.DIR_TMC_TITLE_BAR_USS, l_OptionButton);

                    //Check the style of the Dark Light Elements match the editor theme
                    TMC_Editor.MatchStyleToUnityEditor(l_OptionButton);

                    //Create and setup Option buttons and save the button GUI elements for later use.
                    l_LinkedOptionBody.HideEntireOptionBody(true, false);

                    //DARK Option not selected is the default style state of the GUI component
                    l_OptionButton.AddToClassList("DARK_OptionNotSelected");
                    l_OptionButton.text = l_LinkedOptionBody.HeaderText;
                    
                    //If this is the true first initialization, then the UIChoice wont be available. If not available write to turn the element off.
                    if (m_ScriptTmcMono.m_UIChoices.ContainsKey((l_OptionButton.text + "_Title_ScriptButtonSelected")) == false)
                    {
                        lb_IsItFirstLoad = true;
                        TMC.GurantieeSetDictonary(m_ScriptTmcMono.m_UIChoices, (l_OptionButton.text + "_Title_ScriptButtonSelected"), false);
                    }

                    l_OptionButton.RegisterCallback<MouseUpEvent>((evt) =>
                    {
                        SimulateButtonClick(this, l_OptionButton);
                        
                        //A New Option is selected so ensure that th`e Scene is marked dirty
#if UNITY_EDITOR
                        if (EditorApplication.isPlaying == false)
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
                    });

                    m_OptionSelectorsArea.Add(l_OptionButton);
                    //-------------------------------------------------------------- End Of Basic button link-up -----------------------------------------------------------------------//

                    if (l_LinkedOptionBody.StartSelected)
                        l_DefaultOnElements.Add(new Tuple<Button, TMC_Option_Body>(l_OptionButton, l_LinkedOptionBody));
                }
                else
                    TMC_Editor.MatchStyleToUnityEditor(ChildTreeNode.GetUnityVisualElement());
            }

            //Only use the default setup of should be active on first load. This is detected by checking for if we needed to set any of the button values to off in the m_UIChoices map.
            if (lb_IsItFirstLoad)
            {
                foreach (Tuple<Button, TMC_Option_Body> ElementsToTurnOn in l_DefaultOnElements)
                {
                    //WARNING: IF THE REGISTER CALL BACK EVER GETS CHANGED PLEASE CHANGE THE BELOW SETUP TO SIMULATE A BUTTON CLICK.
                    SimulateButtonClick(this, ElementsToTurnOn.Item1, true, true);
                    TMC.GurantieeSetDictonary(m_ScriptTmcMono.m_UIChoices,
                        (ElementsToTurnOn.Item1.text + "_Title_ScriptButtonSelected"), true);

                    //A New Option is selected so ensure that the Scene is marked dirty
#if UNITY_EDITOR
                    if (EditorApplication.isPlaying == false)
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
                }
            }

            SetupStyleOfTitle();

            //For some reason the changes made to m_UIChoices isnt saved so forcing it to be saved.
            m_ScriptTmcMono.OnBeforeSerialize();
        }

        public void OpenDocumentation()
        {
            TMC_Editor_Utils.OpenLink(ms_DocsURL);
        }

        public void OpenVideoTutorial()
        {
            TMC_Editor_Utils.OpenLink(ms_TutorialURL);
        }

        /// <summary>
        /// Simulate the User clicking on the GUI Button to update the respective elements
        /// </summary>
        /// <param name="title">The Title Element</param>
        /// <param name="body">The related Option Body</param>
        /// <param name="Button">The related Option Button</param>
        /// <param name="UseProvidedState">Should provided state be used, if false the state will just toggle</param>
        /// <param name="stateToUse">State to set if provided state is being used</param>
        private static void SetButtonState(TMC_Title title, TMC_Option_Body body, Button Button, bool UseProvidedState, bool stateToUse)
        {
            body.HideEntireOptionBody(UseProvidedState, stateToUse);
            ToggleButtonStyle(title, Button, UseProvidedState, stateToUse);
        }
        
        /// <summary>
        /// Hide the specific button passed in as a parameter
        /// </summary>
        /// <param name="a_title">The title element</param>
        /// <param name="a_ClickedButton">Button clicked selected</param>
        /// <param name="ab_UseProvidedState">Should state be used to set GUI =</param>
        /// <param name="ab_ShouldElementBeVisible">If state should be used to set GUI what state should it be</param>
        public static void SimulateButtonClick(TMC_Title a_title, Button a_ClickedButton, bool ab_UseProvidedState = false, bool ab_ShouldElementBeVisible = false) //Warning the last two bools arnt used. This needs to be fixed in a future version. Temp fix added to the Manual Start Active loop
        {
            //Create a map for both option body instances and button maps.
            Dictionary<string, TMC_Option_Body> l_optionBodyMap = new Dictionary<string, TMC_Option_Body>();
            Dictionary<string, Button> l_optionButtonMap = new Dictionary<string, Button>();

            //Search through the possible children To find the child that is the matching tmc_option_body
            foreach (TMC_Tree_Node PossibleChildNode in a_title.m_RootTmcTree.m_ListOfChildren.Where(PossibleChildNode => PossibleChildNode.me_Type == TMC_Tree_Node.e_TypeOfUI.TMC_OPTION_BODY))
                TMC.GurantieeSetDictonary(l_optionBodyMap, PossibleChildNode.Get_Self_As_TMC_UXML<TMC_Option_Body>().HeaderText, PossibleChildNode.Get_Self_As_TMC_UXML<TMC_Option_Body>());

            //Search through the possible children
            foreach (TMC_Tree_Node OptionButton in a_title.m_CreatedOptionButtons)
                TMC.GurantieeSetDictonary(l_optionButtonMap, OptionButton.GetUnityVisualElement<Button>().text, (OptionButton.GetUnityVisualElement() as Button));

            //Check if the option button / body exists, if it doesn't then return the function
            TMC_Option_Body l_OptionBodyToChange = TMC.GurantieeGetDictonary(l_optionBodyMap, a_ClickedButton.text, null);
            if (l_OptionBodyToChange == null)
                return;

            List<TMC_Option_Body> l_RelatedOptionBodys = new List<TMC_Option_Body>();
            List<Button> l_RelatedButtons = new List<Button>();
            string[] l_RelatedOptions = l_OptionBodyToChange.MutallyExclusiveOptionsNames.Split(l_OptionBodyToChange.MutallyExclusiveStringSeporatorChar);

            //Using Func and Actions to make the code easier to understand as otherwise the code is almost impossible to understand and I am the one who made it.
            Func<Button, bool> IsButtonAlreadySelected = (ButtonToTest) => ButtonToTest.ClassListContains("LIGHT_OptionSelected") || ButtonToTest.ClassListContains("DARK_OptionSelected");

            Func<List<TMC_Option_Body>, bool> AreAnyOfElementsMustBeSelected = (OptionList) =>
            {
                bool lb_localSave = false;
                foreach (TMC_Option_Body OptionBodyList in OptionList)
                {
                    lb_localSave = OptionBodyList.OneExclusiveMustBeSelected || lb_localSave;
                }
                return lb_localSave;
            };

            foreach (string RelatedElement in l_RelatedOptions)
            {
                l_RelatedOptionBodys.Add(TMC.GurantieeGetDictonary(l_optionBodyMap, RelatedElement));
                l_RelatedButtons.Add(TMC.GurantieeGetDictonary(l_optionButtonMap, RelatedElement));
            }

            bool lb_MustOneElementBeSelected = false;
            if (l_OptionBodyToChange.MutallyExclusiveOptionsNames != "")
                lb_MustOneElementBeSelected = AreAnyOfElementsMustBeSelected(l_RelatedOptionBodys);

            //Check for the special versions of the Title Button click, These are "One Must Be Selected", "MutuallyExclusiveGroup" and the default click. Do three different behaviour for each element
            if (lb_MustOneElementBeSelected)
            {
                if (IsButtonAlreadySelected(a_ClickedButton) && (l_OptionBodyToChange.IsOptionMutuallyExclusive))
                    return;

                //Turn Off all the other related options
                for (int i = 0; i < l_RelatedButtons.Count; i++)
                    SetButtonState(a_title, l_RelatedOptionBodys[i], l_RelatedButtons[i], true, false);

                //Turn on the clicked button, We know this is not self due to the check earlier.
                SetButtonState(a_title, l_OptionBodyToChange, a_ClickedButton, true, true);
            }
            else if (l_OptionBodyToChange.IsOptionMutuallyExclusive)
            {
                //Turn Off all the other related options
                for (int i = 0; i < l_RelatedButtons.Count; i++)
                    SetButtonState(a_title, l_RelatedOptionBodys[i], l_RelatedButtons[i], true, false);

                //Turn on the clicked button, We know this is not self due to the check earlier.
                SetButtonState(a_title, l_OptionBodyToChange, a_ClickedButton, false, false);
            }
            else
                SetButtonState(a_title, l_OptionBodyToChange, a_ClickedButton, false, false);
        }

        /// <summary>
        /// This function just sets the GUI state without going though the checks that simulating a button click would do
        /// </summary>
        /// <param name="a_title">Title element instance</param>
        /// <param name="a_ClickedButton">Button to set state of</param>
        /// <param name="ab_ShouldElementBeVisible">The state the button will be set to</param>
        public static void SetUIState(TMC_Title a_title, Button a_ClickedButton, bool ab_ShouldElementBeVisible)
        {
            //Create a map for both option body instances and button maps.
            Dictionary<string, TMC_Option_Body> l_optionBodyMap = new Dictionary<string, TMC_Option_Body>();
            Dictionary<string, Button> l_optionButtonMap = new Dictionary<string, Button>();

            //Search through the possible children To find the child that is the matching tmc_option_body
            foreach (TMC_Tree_Node ChildBeingChecked in a_title.m_RootTmcTree.m_ListOfChildren.Where(ChildBeingChecked => ChildBeingChecked.me_Type == TMC_Tree_Node.e_TypeOfUI.TMC_OPTION_BODY))
                TMC.GurantieeSetDictonary(l_optionBodyMap, ChildBeingChecked.Get_Self_As_TMC_UXML<TMC_Option_Body>().HeaderText, ChildBeingChecked.Get_Self_As_TMC_UXML<TMC_Option_Body>());

            //Search through the possible children
            foreach (TMC_Tree_Node PossibleRelatedButton in a_title.m_CreatedOptionButtons)
                TMC.GurantieeSetDictonary(l_optionButtonMap, PossibleRelatedButton.GetUnityVisualElement<Button>().text, PossibleRelatedButton.GetUnityVisualElement<Button>());

            //Check if the option button / body exists, if it doesn't then return the function
            TMC_Option_Body l_OptionBodyToChange = TMC.GurantieeGetDictonary(l_optionBodyMap, a_ClickedButton.text, null);
            if (l_OptionBodyToChange == null)
                return;

            //Turn on the clicked button, We know this is not self due to the check earlier.
            SetButtonState(a_title, l_OptionBodyToChange, a_ClickedButton, true, ab_ShouldElementBeVisible);
        }

        /// <summary>
        /// Utility function to set the Button style
        /// </summary>
        /// <param name="a_title">Title instance</param>
        /// <param name="a_Button">Button in question</param>
        /// <param name="ab_UseProvidedState">Use the provided state in ab_ShouldElementBeSelected</param>
        /// <param name="ab_ShouldElementBeSelected">The state of the button if ab_UseProvidedState is true</param>
        private static void ToggleButtonStyle(TMC_Title a_title, Button a_Button, bool ab_UseProvidedState = false, bool ab_ShouldElementBeSelected = false)
        {
            //This could be local function or class function
            Action<TMC_Title, Button, string, string, bool> ToggleButtonSelectedState = (la_title, la_Button, las_ClassNameOptionNotSelected, las_ClassNameOptionSelected, lab_isSelected) =>
            {
                if (lab_isSelected)
                {
                    la_Button.RemoveFromClassList(las_ClassNameOptionNotSelected);
                    la_Button.AddToClassList(las_ClassNameOptionSelected);
                }
                else
                {
                    la_Button.RemoveFromClassList(las_ClassNameOptionSelected);
                    la_Button.AddToClassList(las_ClassNameOptionNotSelected);
                }
                
                TMC.GurantieeSetDictonary(la_title.m_ScriptTmcMono.m_UIChoices, (la_Button.text + "_Title_ScriptButtonSelected"), lab_isSelected);
            };
            
            string ls_ClassNameOptionNotSelected = (TMC_Editor.IsDarkModeBeingUsed() ? "DARK" : "LIGHT") + "_OptionNotSelected";
            string ls_ClassNameOptionSelected = (TMC_Editor.IsDarkModeBeingUsed() ? "DARK" : "LIGHT") + "_OptionSelected";

            if (ab_UseProvidedState)
                ToggleButtonSelectedState(a_title, a_Button, ls_ClassNameOptionNotSelected, ls_ClassNameOptionSelected, ab_ShouldElementBeSelected);
            else
            {
                //If the OptionSelected is to be toggled on
                if (a_Button.ClassListContains(ls_ClassNameOptionNotSelected))
                    ToggleButtonSelectedState(a_title, a_Button, ls_ClassNameOptionNotSelected, ls_ClassNameOptionSelected, true);
                //If the OptionNotSelected is to be toggled on
                else if (a_Button.ClassListContains(ls_ClassNameOptionSelected))
                    ToggleButtonSelectedState(a_title, a_Button, ls_ClassNameOptionNotSelected, ls_ClassNameOptionSelected, false);
            }
        }

        /// <summary>
        /// This function calls TMC Core to check the product category to set the correct image and button tint for the GUI
        /// This function also ensures that the child elements are the correct style match
        /// </summary>
        private void SetupStyleOfTitle()
        {
            if (!(TMC_Editor.GetProductCatogory() < 0))
            {
                string constructedProductImageTintUSSClass = TMC_Editor.GetProductCatogory().ToString() + "_Image_Tint_Colour";

                //- Attempt to Use Tint the Link Buttons -//
                m_DocsButton.styleSheets.Add(TMC_Editor_Assets.m_PreloadedStyleSheets[(int)TMC_Editor_Assets.e_PreMadeStyles.TMC_USS]);
                m_DocsButton.AddToClassList(constructedProductImageTintUSSClass);

                m_TutorialButton.styleSheets.Add(TMC_Editor_Assets.m_PreloadedStyleSheets[(int)TMC_Editor_Assets.e_PreMadeStyles.TMC_USS]);
                m_TutorialButton.AddToClassList(constructedProductImageTintUSSClass);
            }

            //- Make Sure both Title and Script Background are using the correct elements -//
            VisualElement titleBackground = m_RootUnity.Q<VisualElement>("EntireElement");
            TMC_Editor.MatchStyleToUnityEditor(titleBackground);

            VisualElement ScriptBackground = m_RootUnity.Q<VisualElement>("HeaderBar");
            TMC_Editor.MatchStyleToUnityEditor(ScriptBackground);
        }

        /// <summary>
        /// This the function that is called before Serialization occurs
        /// </summary>
        public override void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// This function is called after deserialization occurs and updates the GUI to match the required state.
        /// </summary>
        public override void OnAfterDeSerialize()
        {
            //For every Option Button
            foreach (TMC_Tree_Node l_CreatedOptionButtons in m_CreatedOptionButtons)
            {
                //If the option contains a serialized input.
                if (m_ScriptTmcMono.m_UIChoices.ContainsKey(l_CreatedOptionButtons.GetUnityVisualElement<Button>().text + "_Title_ScriptButtonSelected"))
                {
                    if (TMC.GurantieeGetDictonary(m_ScriptTmcMono.m_UIChoices, l_CreatedOptionButtons.GetUnityVisualElement<Button>().text + "_Title_ScriptButtonSelected"))
                        SimulateButtonClick(this, l_CreatedOptionButtons.GetUnityVisualElement<Button>(), true, true);
                    //else
                    //    SetUIState(this, l_CreatedOptionButtons.GetUnityVisualElement<Button>(), false);
                }
            }
        }

        /// <summary>
        /// Function is called when reset occurs.
        /// </summary>
        public override void OnReset()
        {
        }
    }
}

#endif