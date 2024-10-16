#if UNITY_EDITOR

namespace TaylorMadeCode.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEditor.SceneManagement;

    using TaylorMadeCode.Core.PreloadedAssets;
    using TaylorMadeCode.Core.Utilities;

    //----------------------------------------------//

    public static class TMC_Editor
    {
        //-------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//

        #region TMC_Editor_Parent_Code

        //- Parent System Var -//

        ///<summary> This is the root of the UI tree, This will allow full traversal and saving of all data. </summary>
        public static VisualElement m_UiTreeRoot = null;

        ///<summary> The current node of the tree to act as current. </summary>
        public static TMC_Tree_Node m_CurrentUITreeNode = null;

        ///<summary> The current node that should be treated as the parent node.</summary>
        public static TMC_Tree_Node m_CurrentParentUITreeNode = null;

        ///<summary> These valued are not used for accessing the tree but keeping track of the current building of the tree, These are then used to calculate a unique id for the elements if no name is given during the Create_A_XXXX process. </summary>
        public static int mi_ParentCount = -1, mi_ChildCount = -1;

        /// <summary>
        /// Begins the initialization of the TMC_Editor.
        /// This function sets up the initial state of the editor and the UI tree.
        /// </summary>
        /// <param name="a_Self">The TMC_IObject instance to which this editor belongs.</param>
        /// <param name="a_Element">The VisualElement that represents the root of the UI tree.</param>
        /// <param name="ae_ProductCatogory">The product category associated with the editor.</param>
        public static void Begin(TMC_IObject a_Self, VisualElement a_Element, TMC.ProductCatogory ae_ProductCatogory)
        {
            // Check if any previous TMC_Editor End was called incorrectly, and log an error if so.
            if (m_UiTreeRoot != null ||
                m_CurrentUITreeNode != null ||
                m_CurrentParentUITreeNode != null ||
                mi_ParentCount != -1 ||
                mi_ChildCount != -1)
            {
                Debug.LogError("Last created TMC_Editor End was called incorrectly.");
            }

            // Load pre-loaded assets for the TMC_Editor.
            TMC_Editor_Assets.LoadPreLoadedAssets();

            // Determine the style type based on the editor's skin.
            DecideStyleType();

            // Set the product category for the editor.
            me_ProductCatogory = ae_ProductCatogory;

            // Check if the editor's skin is the Pro skin.
            mb_IsCurrentEditorSkinPro = EditorGUIUtility.isProSkin;

            // Check if the Unity editor is in play mode.
            mb_IsEditorPlaying = EditorApplication.isPlaying;

            // Save the root of the UI tree and set it as the first parent.
            m_UiTreeRoot = a_Element;
            m_CurrentParentUITreeNode = new TMC_Tree_Node(a_Element, TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT);
            a_Self.m_UIRoot = m_CurrentParentUITreeNode;
            mi_ParentCount++;
        }

        /// <summary>
        /// This function sets the last made Visual element to be the parent element until the parent Out function is called.
        /// </summary>
        public static void In_Parent()
        {
            // Check if there is a current parent and current node, and if the current parent is not derived from TMC_UXML or VisualElement, throw an error.
            if (m_CurrentParentUITreeNode != null && m_CurrentUITreeNode != null && !m_CurrentParentUITreeNode.IsSelfUxmlOrVisualElement())
            {
                Debug.LogError("A critical error has occurred in the In_Parent function.");
            }
            else
            {
                //Ensure the type that is becoming parent is a UI element of unity or TMC making
                if (m_CurrentParentUITreeNode.IsSelfUxmlOrVisualElement())
                    m_CurrentParentUITreeNode = m_CurrentUITreeNode;
                else
                    Debug.LogError("TMC_Editor InParent(): Error: A non valid type was attempted to be used as a parent please add type support or change type");

                // Clear the current node as it moves to its parent.
                m_CurrentUITreeNode = null;

                // Since a new parent element is being created, reset child count and increment the parent count.
                mi_ParentCount++;
                mi_ChildCount = -1;
            }
        }

        /// <summary>
        /// The sister function to In_parent. This sets the current parent to back to be the current node.
        /// This function calls the Parent Out function in TMC_UXML Classes.
        /// </summary>
        public static void Out_Parent()
        {
            if (m_CurrentUITreeNode != null)
            {
                if (m_CurrentParentUITreeNode.IsSelfTMC_UXML())
                {
                    (m_CurrentParentUITreeNode.Get_Self_As_TMC_UXML()).ParentOutFunction();
                }

                // When Stepping out of the parent, the parent becomes the current node, and the parents parent becomes the new parent.
                // X -> Y -> Z -> 1
                // If Z is the current element, and we want to step back up one level of the tree then Y is the new current and X is Y's parent.

                m_CurrentUITreeNode = m_CurrentParentUITreeNode;
                m_CurrentParentUITreeNode = m_CurrentParentUITreeNode.m_Parent;

                //Since we are stepping back from a parent the parent selection is decremented.
                mi_ParentCount--;
            }
            else
                Debug.LogError("Out_Parent Must only be called when there is a child element created within. Otherwise remove the calls");
        }

        /// <summary>
        /// This function is the sister function to TMC_Editor.Begin and cleans up the left over resources from building the UI
        /// This function also handles the EndOfCreaton function for the last TMC_UXML function (if it is one)
        /// </summary>
        public static void End(TMC_IObject a_Self)
        {
            if (m_CurrentUITreeNode.m_Parent.m_Parent == null)
            {
                if (m_CurrentUITreeNode.IsSelfTMC_UXML())
                    m_CurrentUITreeNode.Get_Self_As_TMC_UXML().EndOfCreationFunction();

                Mark_All_Dirty();

                //This section is setting everything back to defaults. Deleting all previous data in C# is just setting it to null as all var are like smart pointers.
                m_UiTreeRoot = null;
                m_CurrentUITreeNode = null;
                m_CurrentParentUITreeNode = null;
                mi_ParentCount = -1;
                mi_ChildCount = -1;

                ms_DefaultStyleClasses = "";

                a_Self.OnAfterDeserialize();
                a_Self.m_UIRoot.OnAfterDeserializeChildrenRecursively();
            }
            else
                Debug.LogError("Incorrect Amount On In_Parent And Out_Parent calls. Please rectify");
        }

        #endregion TMC_Editor_Parent_Code

        //-------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//

        #region TMC_Editor_Render_Code

        /// <summary>
        /// Request a VisualElement To be graphically Updated or all of them
        /// </summary>
        /// <param name="a_VisualElementToUpdate">standard null when provided the code will only update the given element otherwise it updates all of them</param>
        public static void Mark_All_Dirty(VisualElement a_VisualElementToUpdate = null)
        {
            if (a_VisualElementToUpdate == null)
                m_UiTreeRoot.MarkDirtyRepaint();
            else
                a_VisualElementToUpdate.MarkDirtyRepaint();
        }

        /// <summary>
        /// function to mark the current visual element as dirty causing it to be re-rendered
        /// </summary>
        public static void Current_Mark_Dirty()
        {
            m_CurrentUITreeNode.MarkSelfDirty();
        }

        #endregion TMC_Editor_Render_Code

        //-------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//

        #region Style_Code

        /// <summary>
        /// Current Selected default style, (Dark Mode or Light Mode)
        /// </summary>
        private static string ms_DefaultStyleClasses = "";

        /// <summary>
        /// Product category, this defines what the accent colors are used as each category has a specific color
        /// </summary>
        private static TMC.ProductCatogory me_ProductCatogory = TMC.ProductCatogory.NONE;

        /// <summary>
        /// Flags that copy the editor flags, You cant assess this during UI building that doesn't work for TMC_EditorV2 so they are defined at begin
        /// This allows me to check the states safely with out errors from flagging.
        /// </summary>
        private static bool mb_IsCurrentEditorSkinPro = true, mb_IsEditorPlaying = false;

        /// <summary>
        /// This function sets the default classes for the all the TMC_Editor Classes.
        /// </summary>
        /// <param name="ab_DefaultClasses"> This is the default style that the TMC_EdtiorV2 should used when rendering</param>
        /// <param name="ab_ReplaceCurrentClasses"> False = Should the new class be appended to the current list or true should the new class replace everything currently set</param>
        public static void SetDefaultClasses(string ab_DefaultClasses, bool ab_ReplaceCurrentClasses = false)
        {
            if (ab_ReplaceCurrentClasses)
                ms_DefaultStyleClasses = ab_DefaultClasses;
            else
                ms_DefaultStyleClasses += ab_DefaultClasses;
        }

        /// <summary>
        /// This adds a new style to a specific style sheet.
        /// </summary>
        /// <param name="as_Stylesheet">StyleSheet instance to add the style classes too</param>
        /// <param name="as_ClassToAdd">This string is the class names, multiple are supported in one call by separating with '|' or your selected char by passing in SplitCharacter</param>
        /// <param name="ac_SplitCharacter">The character that is used to split the ClassToAdd string into separate class names</param>
        public static void AddNewStyleSheetAndClass(StyleSheet as_Stylesheet, string as_ClassToAdd, char ac_SplitCharacter = '|')
        {
            AddNewStyleSheet(as_Stylesheet);
            AddStyleClass(as_ClassToAdd, ac_SplitCharacter);
        }

        /// <summary>
        /// Adds a new style sheet to the newest made ui element
        /// </summary>
        /// <param name="as_StyleSheetToAdd"> the style sheet instance to add</param>
        public static void AddNewStyleSheet(StyleSheet as_StyleSheetToAdd)
        {
            m_CurrentUITreeNode.GetUnityVisualElement().styleSheets.Add(as_StyleSheetToAdd);
        }

        /// <summary>
        /// Add a new Style class to the visual element. (There is a difference between adding a style-sheet instance which contains the data and the class name that tells unity to that style on this element)
        /// </summary>
        /// <param name="as_ClassToAdd">A string that can contain multiple style class's separated by the ac_SplitCharacter param</param>
        /// <param name="ac_SplitCharacter">The character that will be used to split the as_ClassToAdd string defaults to '|'</param>
        public static void AddStyleClass(string as_ClassToAdd, char ac_SplitCharacter = '|')
        {
            List<string> l_ClassNames = as_ClassToAdd.Split(ac_SplitCharacter).ToList<string>();
            foreach (string ls_className in l_ClassNames)
                m_CurrentUITreeNode.GetUnityVisualElement().AddToClassList(ls_className);
        }

        /// <summary>
        /// Poll to see what the default TMC_EditorV2 style should be
        /// </summary>
        public static void DecideStyleType()
        {
            if (IsDarkModeBeingUsed())
                SetDefaultClasses("DarkMode");
            else
                SetDefaultClasses("LightMode");
        }

        /// <summary>
        /// Check to see if dark mode is used, this is behind a function to allow easy TMC_Editor Changes in the future with out having to change all dependent scripts.
        /// </summary>
        /// <returns>True is dark mode is being used or how unity calls it ProMode false if its light mode</returns>
        public static bool IsDarkModeBeingUsed()
        {
            return mb_IsCurrentEditorSkinPro;
        }

        /// <summary>
        /// Check to see if the editor is in playmode, this is behind a function to allow easy TMC_Editor Changes in the future without having to change all the dependent scripts
        /// </summary>
        /// <returns>returns true if editor is in playmode</returns>
        public static bool isEditorPlaying()
        {
            return mb_IsEditorPlaying;
        }

        /// <summary>
        /// Check to see what product category the current script is set to.
        /// </summary>
        /// <returns>return the enum of the category type.</returns>
        public static TMC.ProductCatogory GetProductCatogory()
        {
            return me_ProductCatogory;
        }

        /// <summary>
        /// The function takes all the Classes and replaces all instances of DARK to LIGHT or LIGHT to DARK.]
        /// This method allows all UI Design to be part of USS and not any C# encapsulating the UI visuals to UXML
        /// </summary>
        /// <param name="a_VisualElement"> this visual element to modify the style off</param>
        public static void MatchStyleToUnityEditor(VisualElement a_VisualElement)
        {
            List<string> l_SwappedClasses = new List<string>();

            foreach (string as_ClassName in a_VisualElement.GetClasses())
            {
                if (TMC_Editor.IsDarkModeBeingUsed())
                    l_SwappedClasses.Add(as_ClassName.Replace("LIGHT", "DARK"));
                else
                    l_SwappedClasses.Add(as_ClassName.Replace("DARK", "LIGHT"));
            }

            a_VisualElement.ClearClassList();

            foreach (string ls_SwappedClass in l_SwappedClasses)
                a_VisualElement.AddToClassList(ls_SwappedClass);

            a_VisualElement.MarkDirtyRepaint();
        }

        /// <summary>
        /// This function allows all child elements to be updated to match the current unity style editors
        /// </summary>
        /// <param name="a_VisualElement">The visual element to traverse from</param>
        public static void MatchStyleToUnityEditorForAllChildren(VisualElement a_VisualElement)
        {
            MatchStyleToUnityEditor(a_VisualElement);
            foreach (VisualElement l_child in a_VisualElement.Children())
            {
                MatchStyleToUnityEditorForAllChildren(l_child);
            }
        }

        #endregion Style_Code

        //-------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//

        #region TMC_Editor_Helper_Functions&Var

        /// <summary>
        /// Function provides functionality to hide specific UI elements based on a state.
        /// </summary>
        /// <typeparam name="T">Value of the UI element that it should check</typeparam>
        /// <param name="a_ShouldEqual">The values the UI should be to show the element</param>
        /// <param name="a_StartingValue">The starting value of UI element</param>
        /// <param name="a_AmountOfUIElements">The amount of visual elements to toggle, Don't change this unless you know what you are doing, Use a visual element and do ParentIn to contain everything then Parent Out</param>
        public static void OnlyShowNewestWhenXNewestEquals<T>(T a_ShouldEqual, T a_StartingValue, int a_AmountOfUIElements = 2)
        {
            //Need to check that there is a previously created Element to attach the event to.
            if (m_CurrentParentUITreeNode.m_ListOfChildren.Count < a_AmountOfUIElements && a_AmountOfUIElements >= 2)
            {
                Debug.LogError("Need at 2 previous child to OnlyShowWhenElementsMatch to work");
                throw new Exception("OnlyShowChildrenWhenElementsMatch was attempted to be used with no previous elements on the same level, please rectify");
            }

            //Need to revert to 2 parents
            VisualElement l_SecondNewest = m_CurrentParentUITreeNode.m_ListOfChildren[m_CurrentParentUITreeNode.m_ListOfChildren.Count - a_AmountOfUIElements].GetUnityVisualElement();
            VisualElement l_NewestElement = m_CurrentUITreeNode.GetUnityVisualElement();

            if (a_StartingValue.ToString() == a_ShouldEqual.ToString())
                l_NewestElement.style.display = DisplayStyle.Flex;
            else
                l_NewestElement.style.display = DisplayStyle.None;

            //When the previous element is changed (The Element that is important) Then Toggle the current Element to show / hide.
            l_SecondNewest.RegisterCallback<ChangeEvent<T>>((evt) =>
            {
                //Cant compare directly due to it being a T so Convert to string This will work for basic types
                if (evt.newValue.ToString() == a_ShouldEqual.ToString())
                    l_NewestElement.style.display = DisplayStyle.Flex;
                else
                {
                    l_NewestElement.style.display = DisplayStyle.None;
                }
            });
        }

        /// <summary>
        /// Function to add callbacks to a TMC_Editor Elements.
        /// </summary>
        /// <typeparam name="T">Type of Callback to add</typeparam>
        /// <param name="a_CallBackToAdd"> Callback to add</param>
        public static void Current_Add_Call_Back<T>(EventCallback<T> a_CallBackToAdd) where T : EventBase<T>, new()
        {
            //if the element is a unity type then add the call back to it.
            if (m_CurrentUITreeNode.me_Type > TMC_Tree_Node.e_TypeOfUI.UNITY_START && m_CurrentUITreeNode.me_Type < TMC_Tree_Node.e_TypeOfUI.UNITY_END)
                (m_CurrentUITreeNode.m_Self as VisualElement).RegisterCallback<T>(a_CallBackToAdd);
        }

        public static void Add_Tool_Tip(string as_ToolTip)
        {
            if (m_CurrentUITreeNode.IsSelfVisualElement())
                m_CurrentUITreeNode.GetUnityVisualElement().tooltip = as_ToolTip;
        }

        #region System_Object_Get_Set

        /// <summary>
        /// Takes in the uxml and loads the element and adds it to the given parent and outputs the root of the created Uxml
        /// </summary>
        /// <param name="as_UxmlFileDir">Directory of the UXML File</param>
        /// <param name="ar_Parent"> Parent Visual element that the UXML will be added to</param>
        /// <param name="a_node"> output of the root VisualElement of the UXML file</param>
        public static void LoadUXML(string as_UxmlFileDir, ref TMC_Tree_Node ar_Parent, TMC_Tree_Node.e_TypeOfUI ae_Type, out VisualElement a_node)
        {
            //----------------------------------------------------------------------------------//
            //Get the TMC_Option UI Design then load it and Instantiate it
            VisualTreeAsset l_NewVisualElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(as_UxmlFileDir); //REPLACE THIS WITH CORRECT DIR

            if (l_NewVisualElement == null)
                Debug.LogError("TMC_Editor Cannot find: " + as_UxmlFileDir); //REPLACE WITH CORRECT DIR

            a_node = l_NewVisualElement.Instantiate();
        }

        /// <summary>
        /// Takes in USS Directory and Visual elements and instantiates the USS Style-sheet and adds it to the passed in Visual Element
        /// </summary>
        /// <param name="as_UssDir">USS Directory location</param>
        /// <param name="a_ElementToAddUSSTo">Visual element to add Style-sheet to</param>
        public static void LoadAndSetUSS(string as_UssDir, VisualElement a_ElementToAddUSSTo)
        {
            StyleSheet l_styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(as_UssDir);
            if (l_styleSheet == null)
                Debug.LogError("TMC_Editor Cannot find: " + as_UssDir);

            a_ElementToAddUSSTo.styleSheets.Add(l_styleSheet);
        }

        /// <summary>
        /// Allows the searching of TMC_UI_Tree Nodes to find an get TMC_UXML derived classes.
        /// </summary>
        /// <typeparam name="Type">What class should be found</typeparam>
        /// <param name="a_TreeRoot">what node should be searched from</param>
        /// <returns>null if not found instance if found</returns>
        public static Type GetFistInstanceOfTMC_UXML<Type>(TMC_Tree_Node a_TreeRoot) where Type : TMC_UXML
        {
            //If self is the searched class then return early.
            if (a_TreeRoot.m_Self is Type)
                return (Type)a_TreeRoot.m_Self;

            //Loop Through all children classes
            Type l_Instance = null;
            for (int i = 0; i < a_TreeRoot.m_ListOfChildren.Count; i++)
            {
                //Check child class by recursively calling this function
                l_Instance = GetFistInstanceOfTMC_UXML<Type>(a_TreeRoot.m_ListOfChildren[i]);

                //If instance is found and returned then stop processing
                if (l_Instance != null)
                    return l_Instance;
            }

            //If no instance is found don't return anything.
            return null;
        }

        #endregion System_Object_Get_Set

        #region VisualElement_Get_Set

        /// <summary>
        /// Gets the current visual elements value,
        /// an example of this is TMC_Option_Body where a bool is returned based on if its showing its elements.
        /// </summary>
        /// <returns> returns can return anything, however user is required to correctly convert to the correct data format, be that a standard data type or a custom class</returns>
        public static System.Object Get_Value()
        {
            //Is the current node UXML based.
            if (m_CurrentUITreeNode.IsSelfTMC_UXML())
                return (m_CurrentParentUITreeNode.Get_Self_As_TMC_UXML().GetValue());
            //--------------------BASE_UI------------------------//
            else if (m_CurrentUITreeNode.IsSelfVisualElement())
            {
                switch (m_CurrentUITreeNode.me_Type)
                {
                    case TMC_Tree_Node.e_TypeOfUI.UNITY_LABEL:
                        return (m_CurrentUITreeNode.m_Self as Label).text;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_BUTTON:
                        return (m_CurrentUITreeNode.m_Self as Button).clickable;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_TOGGLE:
                        return (m_CurrentUITreeNode.m_Self as Toggle).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_TEXTFIELD:
                        return (m_CurrentUITreeNode.m_Self as TextField).text;

                    //TODO: Encase the creation to a value that is saved in TMC_Tree_Node
                    //case TMC_Tree_Node.TypeOfUI.UNITY_FOLDOUT:
                    //    return (m_currentUITreeNode.m_self as Foldout);

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_SLIDER:
                        return (m_CurrentUITreeNode.m_Self as Slider).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_SLIDERINT:
                        return (m_CurrentUITreeNode.m_Self as SliderInt).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_MINMAXSLIDER:
                        return (m_CurrentUITreeNode.m_Self as MinMaxSlider).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_PROGRESSBAR:
                        return (m_CurrentUITreeNode.m_Self as ProgressBar).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_DROPDOWN:
                        return (m_CurrentUITreeNode.m_Self as DropdownField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_RADIOBUTTON:
                        return (m_CurrentUITreeNode.m_Self as RadioButton).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_RADIOBUTTONGROUP:
                        return (m_CurrentUITreeNode.m_Self as RadioButtonGroup).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_INTERGERFIELD:
                        return (m_CurrentUITreeNode.m_Self as IntegerField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_FLOATFIELD:
                        return (m_CurrentUITreeNode.m_Self as FloatField).text;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_LONGFIELD:
                        return (m_CurrentUITreeNode.m_Self as LongField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_PROGRSSBAREDITOR:
                        return (m_CurrentUITreeNode.m_Self as ProgressBar).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_VECTOR2FIELD:
                        return (m_CurrentUITreeNode.m_Self as Vector2Field).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_VECTOR3FIELD:
                        return (m_CurrentUITreeNode.m_Self as Vector3Field).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_VECTOR4FIELD:
                        return (m_CurrentUITreeNode.m_Self as Vector4Field).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_RECTFIELD:
                        return (m_CurrentUITreeNode.m_Self as RectField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_BOUNDSFIELD:
                        return (m_CurrentUITreeNode.m_Self as BoundsField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_VECTOR2INTFIELD:
                        return (m_CurrentUITreeNode.m_Self as Vector2IntField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_VECTOR3INTFIELD:
                        return (m_CurrentUITreeNode.m_Self as Vector3IntField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_RECTINTFIELD:
                        return (m_CurrentUITreeNode.m_Self as RectIntField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_BOUNDSINTFIELD:
                        return (m_CurrentUITreeNode.m_Self as BoundsIntField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_OBJECTFIELD:
                        return (m_CurrentUITreeNode.m_Self as ObjectField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_COLOURFIELD:
                        return (m_CurrentUITreeNode.m_Self as ColorField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_CURVEFIELD:
                        return (m_CurrentUITreeNode.m_Self as CurveField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_GRADIANTFIELD:
                        return (m_CurrentUITreeNode.m_Self as GradientField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_ENUMFIELD:
                        return (m_CurrentUITreeNode.m_Self as EnumField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_TAGFIELD:
                        return (m_CurrentUITreeNode.m_Self as TagField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_MASKFIELD:
                        return (m_CurrentUITreeNode.m_Self as MaskField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_LAYERFIELD:
                        return (m_CurrentUITreeNode.m_Self as LayerField).value;

                    case TMC_Tree_Node.e_TypeOfUI.UNITY_LAYERMASKFIELD:
                        return (m_CurrentUITreeNode.m_Self as LayerMaskField).value;

                    default:
                        Debug.LogError("Get_Value() isn't supported for type: " + Convert.ToString(m_CurrentUITreeNode.me_Type));
                        return null;
                }
            }
            else
            {
                Debug.Log("m_currentUITreeNode is not of value, TMC_UXML or Visual Element, Something horribly wrong has occurred");
                return null;
            }
        }

        #endregion VisualElement_Get_Set

        #endregion TMC_Editor_Helper_Functions&Var

        //-------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//

        #region TMC_Editor_Create_Element

        //- Game and editor GUI -//

        /// <summary>
        /// Default code that will get called every time during the value change callback.
        /// </summary>
        public static void BasicOnChangeCallbackFunction()
        {
            if (!mb_IsEditorPlaying)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        /// <summary>
        /// This function is called every-time a Create_A_ function for a TMC_UXML is called. It handles a bunch of default behaviors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a_TmcMono">Instance of TMC_Mono that is being used to make the UI</param>
        /// <param name="ar_Node">Instance of the treenode that is being used to wrap the UI element</param>
        /// <param name="a_AdditionalParameters">Extra parameters that are different for every TMC_UXML derived class</param>
        public static void Basic_UXML_Create_A_SETUP<T>(TMC_IObject a_TmcMono, ref TMC_Tree_Node ar_Node, params object[] a_AdditionalParameters) where T : TMC_UXML
        {
            //If previous element created was TMC_UXML then call is end of creation func.
            if (m_CurrentUITreeNode != null && m_CurrentUITreeNode.IsSelfTMC_UXML())
                m_CurrentUITreeNode.Get_Self_As_TMC_UXML().EndOfCreationFunction();
            else if (m_CurrentParentUITreeNode != null && m_CurrentParentUITreeNode.IsSelfTMC_UXML())
                m_CurrentParentUITreeNode.Get_Self_As_TMC_UXML().EndOfCreationFunction();

            (ar_Node.m_Self as T).Instantiate(ref m_CurrentParentUITreeNode, ref ar_Node, a_TmcMono, a_AdditionalParameters);
            ar_Node.SetParentChildRelationShip(ref m_CurrentParentUITreeNode);
            m_CurrentUITreeNode = ar_Node;
            mi_ChildCount++;
        }

        /// <summary>
        /// This allows any TMC_UXML classes to be created and have the default initializations occur. This allows visual representation but doesn't provide full functionality. For this use the specific Create_A_ function that is made for the UI element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a_TmcMono">Instance of TMC_Mono that is being used to make the UI</param>
        /// <param name="a_Type">The type of TMC_UXML that you are trying to create</param>
        public static void Create_A_UXML<T>(TMC_IObject a_TmcMono, TMC_Tree_Node.e_TypeOfUI a_Type) where T : TMC_UXML, new()
        {
            TMC_Tree_Node l_Node = new TMC_Tree_Node(new T(), a_Type);
            Basic_UXML_Create_A_SETUP<T>(a_TmcMono, ref l_Node);
        }

        /// <summary>
        /// Create a TMC Option Body UXML
        /// </summary>
        /// <param name="a_TmcMono">Instance of TMC_Mono that is being used to make the UI</param>
        /// <param name="as_OptionBodyTitle">Option Body Title Text</param>
        /// <param name="ab_isItForcedFeature">Is the Option Body a forced show element (So not a selectable script feature)</param>
        /// <param name="ab_MutuallyExclusive">Does this Option Body conflict / cannot be used when another script option is selected</param>
        /// <param name="as_MutiallyExclusivestring">name of the Optionbodys that this option body cant be selected with. This string can contain multiple by option body names separated by the selected separator char </param>
        /// <param name="as_MutallyExclusiveSeporatorChar">The char that separates the string above default is | </param>
        public static void Create_A_TMC_Option_Body(TMC_IObject a_TmcMono, string as_OptionBodyTitle, bool ab_isItForcedFeature, bool ab_MutuallyExclusive = false, string as_MutiallyExclusivestring = "", bool ab_OneExclusiveMustBeSelected = false, bool ab_StartSelected = false, char as_MutallyExclusiveSeporatorChar = '|')
        {
            TMC_Tree_Node l_Node = new TMC_Tree_Node(new TMC_Option_Body(), TMC_Tree_Node.e_TypeOfUI.TMC_OPTION_BODY);

            (l_Node.m_Self as TMC_Option_Body).IsOptionMutuallyExclusive = ab_MutuallyExclusive;
            (l_Node.m_Self as TMC_Option_Body).MutallyExclusiveOptionsNames = as_MutiallyExclusivestring;
            (l_Node.m_Self as TMC_Option_Body).MutallyExclusiveStringSeporatorChar = as_MutallyExclusiveSeporatorChar;
            (l_Node.m_Self as TMC_Option_Body).OneExclusiveMustBeSelected = ab_OneExclusiveMustBeSelected;
            (l_Node.m_Self as TMC_Option_Body).StartSelected = ab_StartSelected;

            (l_Node.m_Self as TMC_Option_Body).HeaderText = as_OptionBodyTitle;
            (l_Node.m_Self as TMC_Option_Body).ForcedOption = ab_isItForcedFeature;

            Basic_UXML_Create_A_SETUP<TMC_Option_Body>(a_TmcMono, ref l_Node);
        }

        /// <summary>
        /// Create a TMC Title TMC_UXML element
        /// </summary>
        /// <param name="a_TmcMono">Instance of TMC_Mono that is being used to make the UI</param>
        /// <param name="as_OptionBodyTitle">The text to be shown as the title</param>
        /// <param name="a_ScriptLogo">The Asset Logo Texture2d, This gets shown in the top left. If left blank then it will default to TMC logo</param>
        /// <param name="as_styleClassesForImage">The StyleClasses to be added to the Image this can be left blank</param>
        /// <param name="as_VideoURL">Full URL address for the tutorial video button to go to</param>
        /// <param name="as_DocsURL">Full URL address for the documents button to go to</param>
        public static void Create_A_TMC_Title(TMC_IObject a_TmcMono, string as_OptionBodyTitle, Texture2D a_ScriptLogo = null, string as_styleClassesForImage = "", string as_VideoURL = "", string as_DocsURL = "")
        {
            TMC_Tree_Node l_Node = new TMC_Tree_Node(new TMC_Title(), TMC_Tree_Node.e_TypeOfUI.TMC_TITLE);
            (l_Node.m_Self as TMC_Title).ms_HeaderText = as_OptionBodyTitle;
            Basic_UXML_Create_A_SETUP<TMC_Title>(a_TmcMono, ref l_Node, a_ScriptLogo, as_styleClassesForImage, as_VideoURL, as_DocsURL);
        }

        /// <summary>
        /// Create a TMC Setup Script TMC_UXML element.
        /// </summary>
        /// <param name="a_TmcMono">Instance of TMC_Mono that is being used to make the UI</param>
        /// <param name="as_ScriptSetupTitle">The text that should be shown as the title of the element</param>
        /// <param name="as_Description">The small descriptive text that is shown under the title</param>
        public static void Create_A_TMC_ScriptSetup(TMC_IObject a_TmcMono, string as_ScriptSetupTitle, string as_Description)
        {
            TMC_Tree_Node l_Node = new TMC_Tree_Node(new TMC_Setup_Script(), TMC_Tree_Node.e_TypeOfUI.TMC_SETUP_SCRIPT);
            Basic_UXML_Create_A_SETUP<TMC_Setup_Script>(a_TmcMono, ref l_Node);

            (l_Node.Get_Self_As_TMC_UXML() as TMC_Setup_Script).HeaderText = as_ScriptSetupTitle;
            (l_Node.Get_Self_As_TMC_UXML() as TMC_Setup_Script).DescriptionText = as_Description;
        }

        #region TMC_Editor_Create_Element_Standard

        /// <summary>
        /// This function handles the default code required for all Visual Element Based Create_A_X functions
        /// </summary>
        /// <typeparam name="T">Type of visual element</typeparam>
        /// <param name="ar_l_NewVisualElement">Visual Element being created</param>
        /// <param name="ae_Type">Type of element created as enum</param>
        /// <param name="as_ElementName">Name of the element (This gets a unique ID added to the end of it)</param>
        /// <param name="ab_IsVisible">Is the element visible</param>
        /// <param name="as_Style">The style classes to be added to this specific element. Supports multi class add via a split character specified in next param</param>
        /// <param name="ac_SplitCharacter">The split character fro the above string</param>
        private static void Basic_VisualElement_Create_A_Setup<T>(ref T ar_l_NewVisualElement,
                                            TMC_Tree_Node.e_TypeOfUI ae_Type,
                                            string as_ElementName,
                                            bool ab_IsVisible,
                                            string as_Style,
                                            char ac_SplitCharacter = '|') where T : VisualElement
        {
            //If previous element created was TMC_UXML then call is end of creation func.
            if (m_CurrentUITreeNode != null && m_CurrentUITreeNode.IsSelfTMC_UXML())
                m_CurrentUITreeNode.Get_Self_As_TMC_UXML().EndOfCreationFunction();
            else if (m_CurrentParentUITreeNode != null && m_CurrentParentUITreeNode.IsSelfTMC_UXML())
                m_CurrentParentUITreeNode.Get_Self_As_TMC_UXML().EndOfCreationFunction();

            //Create the new node, set it as current and add it to the parent object.
            TMC_Tree_Node l_Node = new TMC_Tree_Node(ar_l_NewVisualElement, ae_Type);
            m_CurrentUITreeNode = l_Node;

            m_CurrentUITreeNode.SetParentChildRelationShip(ref m_CurrentParentUITreeNode);
            mi_ChildCount++;
            //if name is provided then use it otherwise generate a new name ID from parentCount and VisualElementCount (T)
            ar_l_NewVisualElement.name = ((as_ElementName == null) ? TMC_Editor_Utils.GetUniqueID(mi_ParentCount, mi_ChildCount) : as_ElementName + "_" + TMC_Editor_Utils.GetUniqueID(mi_ParentCount, mi_ChildCount));

            //Load On Basic TMC_USS, Set add the current default classes.
            AddNewStyleSheetAndClass(TMC_Editor_Assets.GetStyle(TMC_Editor_Assets.e_PreMadeStyles.TMC_USS), ms_DefaultStyleClasses + ac_SplitCharacter + as_Style, ac_SplitCharacter);
            
            //Set the visibility
            ar_l_NewVisualElement.visible = ab_IsVisible;
        }

        /// <summary>
        /// A templated function to allow a quick creation of any Visual element derived class. However this lacks subtle control, so for any new element a specific Create_A_X function should be added
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="as_ElementName">Element name</param>
        /// <param name="ae_Type">Type of element as a e_TypeOfUI Enum</param>
        /// <param name="as_Style">A String of style class names that will be added to the visual element, Multi add is supported by separating class names by a char selected in the next param</param>
        /// <param name="ab_IsVisible">The char that indicated the a new class name has started in the as_Style parameter</param>
        public static void Create_A_VisualElement<T>(string as_ElementName,
                                                        TMC_Tree_Node.e_TypeOfUI ae_Type = TMC_Tree_Node.e_TypeOfUI.NONE,
                                                        string as_Style = "",
                                                        bool ab_IsVisible = true)
                                                        where T : VisualElement, new()
        {
            T l_NewVisualElement = new T();
            Basic_VisualElement_Create_A_Setup<T>(ref l_NewVisualElement, ae_Type, as_ElementName, ab_IsVisible, as_Style);
        }
        
        /// <summary>
        /// Create a label visual element.
        /// </summary>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="as_Text">The Text that is shown on screen</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Label(string as_ElementName,
                                            string as_Text = " ",
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            Label l_NewVisualElement = new Label();

            l_NewVisualElement.text = as_Text;

            Basic_VisualElement_Create_A_Setup<Label>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_LABEL, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create a Help Box UI Element
        /// </summary>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="as_Text">The Test Shown as part of the Help Box</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_HelpBox(string as_ElementName,
                                            string as_Text = " ",
                                            HelpBoxMessageType ae_typeOfMessage = HelpBoxMessageType.Info,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            HelpBox l_NewVisualElement = new HelpBox();

            l_NewVisualElement.text = as_Text;
            l_NewVisualElement.messageType = ae_typeOfMessage;

            Basic_VisualElement_Create_A_Setup<HelpBox>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_HELPBOX, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create A button visual element
        /// </summary>
        /// <param name="a_ButtonAction">What function will be called after the user clicks the button </param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="as_Text">The Text shown on the button</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Button(System.Action a_ButtonAction,
                                            string as_ElementName,
                                            string as_Text = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.Button l_NewVisualElement = new UnityEngine.UIElements.Button();

            //Element Specific Code
            l_NewVisualElement.text = (as_Text == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Text;
            l_NewVisualElement.clickable.clicked += a_ButtonAction;

            Basic_VisualElement_Create_A_Setup<Button>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_BUTTON, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create a Button with an pre loaded icon instead of text
        /// </summary>
        /// <param name="a_ButtonAction">What function will be called after the user clicks the button</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="ae_Image">The Preloaded image that can be shown instead of text</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Button(System.Action a_ButtonAction,
                                            string as_ElementName,
                                            TMC_Editor_Assets.e_PreLoadedImages ae_Image,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.Button l_NewVisualElement = new UnityEngine.UIElements.Button();

            //Element Specific Code
            l_NewVisualElement.style.backgroundImage = new StyleBackground(TMC_Editor_Assets.m_PreLoadedSpritesList[(int)ae_Image]);
            l_NewVisualElement.clickable.clicked += a_ButtonAction;

            Basic_VisualElement_Create_A_Setup<Button>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_BUTTON, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create a Toggle Visual Element
        /// </summary>
        /// <param name="ab_Variable">The Starting State of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Text">The text that is shown next to the toggle element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Toggle(bool ab_Variable,
                                            string as_ElementName,
                                            Action<bool> a_CallbackMethod,
                                            string as_Text = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.Toggle l_NewVisualElement = new UnityEngine.UIElements.Toggle();

            //Element Specific Code
            l_NewVisualElement.text = (as_Text == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Text;
            l_NewVisualElement.value = ab_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                TMC_Editor.BasicOnChangeCallbackFunction();
                a_CallbackMethod((bool)evt.newValue);
            });

            Basic_VisualElement_Create_A_Setup<Toggle>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_TOGGLE, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create A scrolling Visual Element
        /// </summary>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="ai_LowValue">Lowest Value of the slider</param>
        /// <param name="ai_HighValue">The highest value of the slider</param>
        /// <param name="ae_SliderDir">Vertical or horizontal scroll bar</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Scroller(string as_ElementName,
                                            int ai_LowValue = 0,
                                            int ai_HighValue = 100,
                                            SliderDirection ae_SliderDir = SliderDirection.Vertical,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.Slider l_NewVisualElement = new UnityEngine.UIElements.Slider();

            Basic_VisualElement_Create_A_Setup<Slider>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_SLIDER, as_ElementName, ab_IsVisible, as_Style);

            //Element Specific Code
            l_NewVisualElement.lowValue = ai_LowValue;
            l_NewVisualElement.highValue = ai_HighValue;
            l_NewVisualElement.direction = ae_SliderDir;
        }

        /// <summary>
        /// Create A text Field Element
        /// </summary>
        /// <param name="as_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown along side the text field element</param>
        /// <param name="ai_MaxLength">Max length of inputted text</param>
        /// <param name="ab_IsPasswordField"> is the text used for a password field</param>
        /// <param name="ac_MaskChar"> If password field what should the characters be shown as to keep password safe</param>
        /// <param name="ab_IsReadOnly"> If the UI read only</param>
        /// <param name="ab_IsDelayed">is the UI input delayed</param>
        /// <param name="ab_IsMultiLine"> Can you write over multiple lines for this UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_TextField(string as_Variable,
                                                string as_ElementName,
                                                Action<string> a_CallbackMethod,
                                                string as_Label = null,
                                                int ai_MaxLength = -1,
                                                bool ab_IsPasswordField = false,
                                                char ac_MaskChar = ' ',
                                                bool ab_IsReadOnly = false,
                                                bool ab_IsDelayed = false,
                                                bool ab_IsMultiLine = false,
                                                bool ab_IsVisible = true,
                                                string as_Style = "")
        {
            UnityEngine.UIElements.TextField l_NewVisualElement = new UnityEngine.UIElements.TextField();

            //Element Specific Code
            l_NewVisualElement.value = as_Variable;
            l_NewVisualElement.maxLength = ai_MaxLength;
            l_NewVisualElement.isPasswordField = ab_IsPasswordField;
            l_NewVisualElement.maskChar = ac_MaskChar;
            l_NewVisualElement.isReadOnly = ab_IsReadOnly;
            l_NewVisualElement.isDelayed = ab_IsDelayed;
            l_NewVisualElement.multiline = ab_IsMultiLine;
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;

            l_NewVisualElement.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                a_CallbackMethod((string)evt.newValue);
            });

            Basic_VisualElement_Create_A_Setup<TextField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_TEXTFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create A Foldout Visual Element
        /// </summary>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="as_LabelText">The text that shown as the drop down title</param>
        /// <param name="ab_Value">The starting state of the drop down</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Foldout(string as_ElementName,
                                            string as_LabelText = "labelText",
                                            bool ab_Value = true,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.Foldout l_NewVisualElement = new UnityEngine.UIElements.Foldout();

            //element specific code
            l_NewVisualElement.text = as_LabelText;
            l_NewVisualElement.value = ab_Value;

            Basic_VisualElement_Create_A_Setup<Foldout>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_FOLDOUT, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create A slider Visual Element
        /// </summary>
        /// <param name="af_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="af_LowValue">The lowest value the slider can be</param>
        /// <param name="af_HighValue">The Highest value the slider can be</param>
        /// <param name="af_PageSize"> What size the page should be</param>
        /// <param name="ab_ShowInputField">Show the input field</param>
        /// <param name="ae_SliderDirection">What direction is the slider for</param>
        /// <param name="ab_Inverted"> Is the movement inverted</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Slider(float af_Variable,
                                            string as_ElementName,
                                            Action<float> a_CallbackMethod,
                                            string as_Label = null,
                                            float af_LowValue = 0,
                                            float af_HighValue = 100,
                                            float af_PageSize = 25,
                                            bool ab_ShowInputField = true,
                                            SliderDirection ae_SliderDirection = SliderDirection.Horizontal,
                                            bool ab_Inverted = false,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.Slider l_NewVisualElement = new UnityEngine.UIElements.Slider();

            //element specific code
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.lowValue = af_LowValue;
            l_NewVisualElement.highValue = af_HighValue;
            l_NewVisualElement.pageSize = af_PageSize;
            l_NewVisualElement.showInputField = ab_ShowInputField;
            l_NewVisualElement.direction = ae_SliderDirection;
            l_NewVisualElement.inverted = ab_Inverted;

            l_NewVisualElement.RegisterCallback<ChangeEvent<float>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<Slider>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_SLIDER, as_ElementName, ab_IsVisible, as_Style);
            
            l_NewVisualElement.value = af_Variable;
        }

        /// <summary>
        /// Create A slider Int Visual Element
        /// </summary>
        /// <param name="ai_Variable">The Starting State of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the visual element</param>
        /// <param name="ai_Low"> The Lowest Possible value</param>
        /// <param name="ai_High">The highest possible value</param>
        /// <param name="ab_ShowInputField">Allow text input instead of just the slider</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_SliderInt(int ai_Variable,
                                                string as_ElementName,
                                                Action<int> a_CallbackMethod,
                                                string as_Label = null,
                                                int ai_Low = 0,
                                                int ai_High = 10,
                                                bool ab_ShowInputField = true,
                                                bool ab_IsVisible = true,
                                                string as_Style = "")
        {
            SliderInt l_NewVisualElement = new SliderInt();

            //element specific code
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.lowValue = ai_Low;
            l_NewVisualElement.highValue = ai_High;
            l_NewVisualElement.showInputField = ab_ShowInputField;

            l_NewVisualElement.RegisterCallback<ChangeEvent<int>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<SliderInt>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_SLIDERINT, as_ElementName, ab_IsVisible, as_Style);
            
            l_NewVisualElement.value = ai_Variable;
        }

        /// <summary>
        /// Create a MinMaxSlider visual element
        /// </summary>
        /// <param name="a_Variable">Starting Value</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="af_LowLimit">The Lowest limit of the MinMax Slider</param>
        /// <param name="af_HighLimit">The Highest limit of the MinMax Slider</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_MinMaxSlider(Vector2 a_Variable,
                                                    string as_ElementName,
                                                    Action<Vector2> a_CallbackMethod,
                                                    string as_Label = null,
                                                    float af_LowLimit = 0,
                                                    float af_HighLimit = 10,
                                                    bool ab_IsVisible = true,
                                                    string as_Style = "")
        {
            UnityEngine.UIElements.MinMaxSlider l_NewVisualElement = new UnityEngine.UIElements.MinMaxSlider();

            //Element Specific Code
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            
            l_NewVisualElement.lowLimit = af_LowLimit;
            l_NewVisualElement.highLimit = af_HighLimit;

            l_NewVisualElement.minValue = a_Variable.x;
            l_NewVisualElement.maxValue = a_Variable.y;
            
            l_NewVisualElement.RegisterCallback<ChangeEvent<Vector2>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); Debug.Log("Callback working"); });

            Basic_VisualElement_Create_A_Setup<MinMaxSlider>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_MINMAXSLIDER, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Creates A ProgressBar Visual Element
        /// </summary>
        /// <param name="af_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="af_LowValue">Bar is fully empty when the variable equals this value</param>
        /// <param name="af_HighValue">Bar is full when the variable equals this value</param>
        /// <param name="as_Title">The title of the progress bar</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_ProgressBar(float af_Variable,
                                                    string as_ElementName,
                                                    float af_LowValue = 0,
                                                    float af_HighValue = 100,
                                                    string as_Title = null,
                                                    bool ab_IsVisible = true,
                                                    string as_Style = "")
        {
            UnityEngine.UIElements.ProgressBar l_NewVisualElement = new UnityEngine.UIElements.ProgressBar();

            //element specific code
            l_NewVisualElement.lowValue = af_LowValue;
            l_NewVisualElement.highValue = af_HighValue;
            l_NewVisualElement.value = af_Variable;
            l_NewVisualElement.title = (as_Title == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Title;

            Basic_VisualElement_Create_A_Setup<ProgressBar>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_PROGRESSBAR, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Creates A Drop Down Visual Element
        /// </summary>
        /// <param name="as_Variable">The starting value of the drop down</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ai_Index">The selected index of the DropDown Element</param>
        /// <param name="as_Choices">A string of all the DropDown Choices</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_DropDown(string as_Variable,
                                                string as_ElementName,
                                                Action<string> a_CallbackMethod,
                                                bool ab_IsVisible = true,
                                                string as_Label = null,
                                                int ai_Index = -1,
                                                string as_Choices = "",
                                                string as_Style = "")
        {
            UnityEngine.UIElements.DropdownField l_NewVisualElement = new UnityEngine.UIElements.DropdownField();

            //element specific code
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.index = ai_Index;
            l_NewVisualElement.choices = as_Choices.Split(',').ToList<string>();
            l_NewVisualElement.value = as_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<string>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<DropdownField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_DROPDOWN, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Creates A Radio Button Visual Element
        /// </summary>
        /// <param name="ab_Variable">The Starting State of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallBackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_RadioButton(bool ab_Variable,
                                                string as_ElementName,
                                                Action<bool> a_CallBackMethod,
                                                string as_Label = null,
                                                bool ab_IsVisible = true,
                                                string as_Style = "")
        {
            UnityEngine.UIElements.RadioButton l_NewVisualElement = new UnityEngine.UIElements.RadioButton();
            //element specific code
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = ab_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<bool>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallBackMethod((bool)evt.newValue); });

            Basic_VisualElement_Create_A_Setup<RadioButton>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_RADIOBUTTON, as_ElementName, ab_IsVisible, as_Style);
        }

        #endregion TMC_Editor_Create_Element_Standard

        ////- Editor Only GUI -//

        #region TMC_Editor_Create_Element_Editor_Only

        /// <summary>
        /// Creates a Radio button group
        /// </summary>
        /// <param name="ai_Variable">The Starting State of the created Visual Element</param>
        /// <param name="as_ElementName">Name of the element if null a unique id will be created</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Choices">List of choices by name separated by a comma</param>
        /// <param name="as_Label">This is the text that will shown as the label</param>
        /// <param name="ab_IsVisible">Is the visual element visible</param>
        /// <param name="as_Style">Style used for the element if null the latest stated style will be used</param>
        public static void Create_A_RadioButtonGroup(int ai_Variable,
                                                    string as_ElementName,
                                                    Action<int> a_CallbackMethod,
                                                    string as_Choices,
                                                    string as_Label = null,
                                                    bool ab_IsVisible = true,
                                                    string as_Style = "")
        {
            UnityEngine.UIElements.RadioButtonGroup l_NewVisualElement = new UnityEngine.UIElements.RadioButtonGroup();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = ai_Variable;
            l_NewVisualElement.choices = as_Choices.Split(',').ToList<string>();

            l_NewVisualElement.RegisterCallback<ChangeEvent<int>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<RadioButtonGroup>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_RADIOBUTTONGROUP, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create An Integer Input Visual Element
        /// </summary>
        /// <param name="ai_Variable">The Starting State of the created Visual Element</param>
        /// <param name="as_ElementName">Name of the element if null a unique id will be created</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsReadOnly">Is the Integer Input Read Only</param>
        /// <param name="ab_IsDelayed">is the Input Delayed</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Integer(int ai_Variable,
                                            string as_ElementName,
                                            Action<int> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsReadOnly = false,
                                            bool ab_IsDelayed = false,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.IntegerField l_NewVisualElement = new UnityEngine.UIElements.IntegerField();

            //Element specific code
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = ai_Variable;
            l_NewVisualElement.isReadOnly = ab_IsReadOnly;
            l_NewVisualElement.isDelayed = ab_IsDelayed;

            l_NewVisualElement.RegisterCallback<ChangeEvent<int>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<IntegerField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_INTERGERFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create a float input visual element
        /// </summary>
        /// <param name="af_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsReadOnly">Is the Integer Input Read Only</param>
        /// <param name="ab_IsDelayed">is the Input Delayed</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Float(float af_Variable,
                                            string as_ElementName,
                                            Action<float> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsReadOnly = false,
                                            bool ab_IsDelayed = false,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.FloatField l_NewVisualElement = new UnityEngine.UIElements.FloatField();

            //element specific code
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = af_Variable;
            l_NewVisualElement.isReadOnly = ab_IsReadOnly;
            l_NewVisualElement.isDelayed = ab_IsDelayed;

            l_NewVisualElement.RegisterCallback<ChangeEvent<float>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<FloatField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_FLOATFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create A long Input field visual element
        /// </summary>
        /// <param name="al_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsReadOnly">Is the Integer Input Read Only</param>
        /// <param name="ab_IsDelayed">is the Input Delayed</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Long(long al_Variable,
                                            string as_ElementName,
                                            Action<long> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsReadOnly = false,
                                            bool ab_IsDelayed = false,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.LongField l_NewVisualElement = new UnityEngine.UIElements.LongField();

            //Element specific code
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = al_Variable;
            l_NewVisualElement.isReadOnly = ab_IsReadOnly;
            l_NewVisualElement.isDelayed = ab_IsDelayed;

            l_NewVisualElement.RegisterCallback<ChangeEvent<long>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<LongField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_LONGFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create Vector2 input visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Vector2(Vector2 a_Variable,
                                            string as_ElementName,
                                            Action<Vector2> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.Vector2Field l_NewVisualElement = new UnityEngine.UIElements.Vector2Field();

            //element specific code
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<Vector2>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<Vector2Field>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_VECTOR2FIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create a Vector3 Input Visual Element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Vector3(Vector3 a_Variable,
                                            string as_ElementName,
                                            Action<Vector3> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.Vector3Field l_NewVisualElement = new UnityEngine.UIElements.Vector3Field();

            //Element Specific Code
            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<Vector3>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<Vector3Field>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_VECTOR3FIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create a Vector4 input visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Vector4(Vector4 a_Variable,
                                            string as_ElementName,
                                            Action<Vector4> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.Vector4Field l_NewVisualElement = new UnityEngine.UIElements.Vector4Field();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<Vector4>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<Vector4Field>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_VECTOR4FIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create A rect input visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Rect(Rect a_Variable,
                                            string as_ElementName,
                                            Action<Rect> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.RectField l_NewVisualElement = new UnityEngine.UIElements.RectField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<Rect>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<RectField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_RECTFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create A Bound Input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Bounds(Bounds a_Variable,
                                            string as_ElementName,
                                            Action<Bounds> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.BoundsField l_NewVisualElement = new UnityEngine.UIElements.BoundsField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<Bounds>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<BoundsField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_BOUNDSFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create A vector2 int input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Vector2Int(Vector2Int a_Variable,
                                                string as_ElementName,
                                                Action<Vector2Int> a_CallbackMethod,
                                                string as_Label = null,
                                                bool ab_IsVisible = true,
                                                string as_Style = "")
        {
            UnityEngine.UIElements.Vector2IntField l_NewVisualElement = new UnityEngine.UIElements.Vector2IntField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<Vector2Int>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<Vector2IntField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_VECTOR2INTFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        /// Create A vector3 int input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Vector3Int(Vector3Int a_Variable,
                                                string as_ElementName,
                                                Action<Vector3Int> a_CallbackMethod,
                                                string as_label = null,
                                                bool ab_IsVisible = true,
                                                string as_Style = "")
        {
            UnityEngine.UIElements.Vector3IntField l_NewVisualElement = new UnityEngine.UIElements.Vector3IntField();

            l_NewVisualElement.label = (as_label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<Vector3Int>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<Vector3IntField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_VECTOR3INTFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A Rect int input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_RectInt(RectInt a_Variable,
                                            string as_ElementName,
                                            Action<RectInt> a_CallbackMethod,
                                            string as_label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.RectIntField l_NewVisualElement = new UnityEngine.UIElements.RectIntField();

            l_NewVisualElement.label = (as_label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<RectInt>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<RectIntField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_RECTINTFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A bounds int input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Bounds(BoundsInt a_Variable,
                                            string as_ElementName,
                                            Action<BoundsInt> a_CallbackMethod,
                                            string as_label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            UnityEngine.UIElements.BoundsIntField l_NewVisualElement = new UnityEngine.UIElements.BoundsIntField();

            l_NewVisualElement.label = as_label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<BoundsInt>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<BoundsIntField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_BOUNDSINTFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A Objectfield input Visual element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_AllowSceneObjects">Allow Scene Objects to be selected as the Selected Object</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_ObjectField<T>(T a_Variable,
                                                    string as_ElementName,
                                                    Action<T> a_CallbackMethod,
                                                    string as_Label = null,
                                                    bool ab_AllowSceneObjects = true,
                                                    bool ab_IsVisible = true,
                                                    string as_Style = "") where T : UnityEngine.Object
        {
            UnityEditor.UIElements.ObjectField l_NewVisualElement = new UnityEditor.UIElements.ObjectField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.allowSceneObjects = ab_AllowSceneObjects;
            l_NewVisualElement.objectType = typeof(T);
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<UnityEngine.Object>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod((T)evt.newValue); });
            
            Basic_VisualElement_Create_A_Setup<ObjectField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_OBJECTFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A Color input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_ShowEyeDropper">Allow the EyeDropper functionality</param>
        /// <param name="ab_ShowAlpha">Show Alpha values</param>
        /// <param name="ab_HDR">Is the color HDR standard</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Colour(Color a_Variable,
                                                string as_ElementName,
                                                Action<Color> a_CallbackMethod,
                                                string as_Label = null,
                                                bool ab_ShowEyeDropper = true,
                                                bool ab_ShowAlpha = true,
                                                bool ab_HDR = false,
                                                bool ab_IsVisible = true,
                                                string as_Style = "")
        {
            UnityEditor.UIElements.ColorField l_NewVisualElement = new UnityEditor.UIElements.ColorField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;
            l_NewVisualElement.showEyeDropper = ab_ShowEyeDropper;
            l_NewVisualElement.showAlpha = ab_ShowAlpha;
            l_NewVisualElement.hdr = ab_HDR;

            l_NewVisualElement.RegisterCallback<ChangeEvent<Color>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<ColorField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_COLOURFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A Curve input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Curve(AnimationCurve a_Variable,
                                            string as_ElementName,
                                            Action<AnimationCurve> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            CurveField l_NewVisualElement = new CurveField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<AnimationCurve>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });
            Basic_VisualElement_Create_A_Setup<CurveField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_CURVEFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A Gradient input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Gradient(Gradient a_Variable,
                                                string as_ElementName,
                                                Action<Gradient> a_CallbackMethod,
                                                string as_Label = null,
                                                bool ab_IsVisible = true,
                                                string as_Style = "")
        {
            GradientField l_NewVisualElement = new GradientField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<Gradient>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });
            Basic_VisualElement_Create_A_Setup<GradientField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_GRADIANTFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A enum input Visual element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Enum<T>(T a_Variable,
                                            string as_ElementName,
                                            Action<T> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = ""
                                            ) where T : struct, Enum
        {
            EnumField enumField = new EnumField();
            enumField.Init(a_Variable);

            enumField.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;

            enumField.RegisterValueChangedCallback(evt => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod((T)evt.newValue); });

            Basic_VisualElement_Create_A_Setup<EnumField>(ref enumField, TMC_Tree_Node.e_TypeOfUI.UNITY_ENUMFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A Tag input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Tag(string a_Variable,
                                            string as_ElementName,
                                            Action<string> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            TagField l_NewVisualElement = new TagField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<string>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<TagField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_TAGFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A Mask int input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Mask(int a_Variable,
                                            string as_ElementName,
                                            Action<int> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            MaskField l_NewVisualElement = new MaskField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<int>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<MaskField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_MASKFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A layer input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Layer(int a_Variable,
                                            string as_ElementName,
                                            Action<int> a_CallbackMethod,
                                            string as_Label = null,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            LayerField l_NewVisualElement = new LayerField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<int>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<LayerField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_LAYERFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A Layer Mask input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="a_CallbackMethod">The callback that sets the Script Var to be equal to the Changed UI value</param>
        /// <param name="as_Label">The text shown next to the UI element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_LayerMask(int a_Variable,
                                                string as_ElementName,
                                                Action<LayerMask> a_CallbackMethod,
                                                string as_Label = null,
                                                bool ab_IsVisible = true,
                                                string as_Style = "")
        {
            LayerMaskField l_NewVisualElement = new LayerMaskField();

            l_NewVisualElement.label = (as_Label == null) ? TMC_Editor_Utils.ProcessCamelCaseToString(as_ElementName) : as_Label;
            l_NewVisualElement.value = a_Variable;

            l_NewVisualElement.RegisterCallback<ChangeEvent<int>>((evt) => { TMC_Editor.BasicOnChangeCallbackFunction(); a_CallbackMethod(evt.newValue); });

            Basic_VisualElement_Create_A_Setup<LayerMaskField>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_LAYERMASKFIELD, as_ElementName, ab_IsVisible, as_Style);
        }

#pragma warning disable CS1072

        /// <summary>
        ///  Create A List of Unity Actions input Visual element
        /// </summary>
        /// <param name="a_Variable">The Starting value of the created Visual Element</param>
        /// <param name="a_SerializedObject">The UI Serialized object</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_List_Of_UnityActions(TMC_IObject a_Variable,
                                            SerializedObject a_SerializedObject,
                                            string as_ElementName,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            Func<VisualElement> CreateUnityActionVisualElement = () =>
            {
                return new IMGUIContainer(() =>
                {
                    if (a_SerializedObject != null && a_SerializedObject.targetObject != null)
                    {
                        a_SerializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        var property = a_SerializedObject.FindProperty(as_ElementName);
                        if (property != null)
                        {
                            EditorGUILayout.PropertyField(property);
                            if (EditorGUI.EndChangeCheck())
                            {
                                a_SerializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                });
            };

            VisualElement temp = CreateUnityActionVisualElement();

            Basic_VisualElement_Create_A_Setup<VisualElement>(ref temp, TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT, as_ElementName, ab_IsVisible, as_Style);
        }

#pragma warning restore CS1072

        /// <summary>
        ///  Create A DearImGui Section Visual element
        /// </summary>
        /// <param name="a_SerializedObject">The UI Serialized object</param>
        /// <param name="as_ElementName">The name of the constructed Visual Element</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void DisplayVarViaDearImGui(SerializedObject a_SerializedObject,
                                            string as_ElementName,
                                            bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            Func<VisualElement> CreateUnityActionVisualElement = () =>
            {
                return new IMGUIContainer(() =>
                {
                    if (a_SerializedObject != null && a_SerializedObject.targetObject != null)
                    {
                        a_SerializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        var property = a_SerializedObject.FindProperty(as_ElementName);
                        if (property != null)
                        {
                            EditorGUILayout.PropertyField(property);
                            if (EditorGUI.EndChangeCheck())
                            {
                                a_SerializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                });
            };

            VisualElement temp = CreateUnityActionVisualElement();

            Basic_VisualElement_Create_A_Setup<VisualElement>(ref temp, TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT, as_ElementName, ab_IsVisible, as_Style);
        }

        /// <summary>
        ///  Create A Vertical space Visual element
        /// </summary>
        /// <param name="ai_Size">The Amount of space to have default = 10</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Vertical_Space(int ai_Size = 10,
                                                bool ab_IsVisible = true,
                                                string as_Style = "")
        {
            VisualElement l_NewVisualElement = new VisualElement();

            Basic_VisualElement_Create_A_Setup<VisualElement>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT, "SpaceElement", ab_IsVisible, as_Style);

            l_NewVisualElement.style.paddingTop = ai_Size;
        }

        /// <summary>
        ///  Create A Horizontal Space Visual element
        /// </summary>
        /// <param name="ai_Size">The Amount of space to have default = 10</param>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Horizontal_Space(int ai_Size = 10,
                                                bool ab_IsVisible = true,
                                                string as_Style = "")
        {
            VisualElement l_NewVisualElement = new VisualElement();

            Basic_VisualElement_Create_A_Setup<VisualElement>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT, "SpaceElement", ab_IsVisible, as_Style);

            l_NewVisualElement.style.paddingLeft = ai_Size;
        }

        /// <summary>
        ///  Create Horizontal Group Visual element
        /// </summary>
        /// <param name="ab_IsVisible">Is the Visual Element visible in the UI</param>
        /// <param name="as_Style">The names of the style classes to be added to the visual element, Multi add supported but class names need to be separated by |</param>
        public static void Create_A_Horizontal_Group(bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            VisualElement l_NewVisualElement = new VisualElement();
            l_NewVisualElement.name = "";

            Basic_VisualElement_Create_A_Setup<VisualElement>(ref l_NewVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT, "HorizontalGroup", ab_IsVisible, as_Style);

            l_NewVisualElement.style.flexDirection = FlexDirection.Row;
        }

        #endregion TMC_Editor_Create_Element_Editor_Only

        public static void Create_A_Vertical_Divider(bool ab_IsVisible = true,
                                            string as_Style = "")
        {
            VisualElement l_newVisualElement = new VisualElement();

            as_Style += "|Colour_Design_DARK_1_Background_Colour";

            Basic_VisualElement_Create_A_Setup<VisualElement>(ref l_newVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT, "LineDivider", ab_IsVisible, as_Style);

            l_newVisualElement.style.width = 2.0f;
        }

        public static void Create_A_Horizontal_Divider(bool ab_IsVisible = true,
                                    string as_Style = "")
        {
            VisualElement l_newVisualElement = new VisualElement();

            as_Style += "|Colour_Design_DARK_1_Background_Colour";

            Basic_VisualElement_Create_A_Setup<VisualElement>(ref l_newVisualElement, TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT, "LineDivider", ab_IsVisible, as_Style);

            l_newVisualElement.style.height = 2.0f;
        }

        #endregion TMC_Editor_Create_Element
    }
}

#endif