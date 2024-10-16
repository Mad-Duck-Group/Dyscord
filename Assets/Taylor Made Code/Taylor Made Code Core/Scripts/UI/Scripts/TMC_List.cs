#if UNITY_EDITOR

//Version 1.0.0.0
//Auto Document 1

using System.Collections;
using System.Linq;
using TaylorMadeCode.Core.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaylorMadeCode.Core
{
    public class TMC_List : TMC_UXML
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
        /// Override function for the array accessors
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
            return null; ////--------------------------------------------------------------------NEEDS UPDATING--------------------------------------------------------------------
        }

        //- End of overridden classes -//

        public VisualElement EntireElement;

        /// <summary>
        /// This is the function to place instantiation code
        /// </summary>
        /// <param name="Parent">The parent object of the TMC_UXML element</param>
        /// <returns>The TMC_UXML Class</returns>
        public override TMC_UXML Instantiate(
            ref TMC_Tree_Node a_parent,
            ref TMC_Tree_Node a_self,
            params object[] a_additionalParameters)
        {
            if (a_additionalParameters.Count() <= 0)
            {
                if (a_additionalParameters[0] is TMC_IObject)
                    m_ScriptTmcMono = a_additionalParameters[0] as TMC_IObject;
                else
                    Debug.LogError("Fatal error in TMC_Title Code. This issue will prevent any feedback from the UI systems");
            }

            //----------------------------------------------------------------------------------//
            //Get the TMC_Option UI Design then load it and Instantiate it
            VisualElement newVisualElement;

            //Change DIR_TMC and TMC_UXML to the relevant types.
            TMC_Editor.LoadUXML(TMC_Editor_Utils.DIR_TMC, ref a_parent, TMC_Tree_Node.e_TypeOfUI.TMC_UXML, out newVisualElement);
            TMC_Editor.LoadAndSetUSS(TMC_Editor_Utils.DIR_TMC, newVisualElement);

            m_RootUnity = newVisualElement;

            //------------------------------------IMPORTANT-------------------------------//
            //   This section of code will need to be altered for each TMC_UXML derived   //
            //   Class. If the Root_TMC_Editor Functionality is being used then below     //
            //   Is A suitable example. Just change the Root_Unity to be the Required     //
            //   Value. Then Alter the type to match.                                     //
            //                                                                            //
            //   If Root_TMC_Editor functionality isn't being used then Remove the last   //
            //   Three lines and set Root_TMC_Editor to = TMC_Tree_Node(Root_Unity...  //
            //----------------------------------------------------------------------------//

            m_RootTmcEditor = new TMC_Tree_Node(m_RootUnity, TMC_Tree_Node.e_TypeOfUI.UNITY_VISUALELEMENT);
            m_RootTmcEditor.m_Parent = a_parent;
            m_RootTmcEditor.mb_IsRootTMCEditor = false;
            m_RootTmcEditor.m_TmcUxmlClass = this;

            //Root_TMC_Editor = new TMC_Tree_Node(Root_Unity, TMC_Tree_Node.TypeOfUI.UNITY_VISUALELEMENT);

            //----------------------------------------------------------------------------//

            m_RootTmcTree = a_self;

            //- Custom Setup start -//

            //- Custom Setup end -//

            return this;
        }

        /// <summary>
        /// This function is called once a new end of creation a new function.
        /// </summary>
        public override void EndOfCreationFunction()
        {
            //Var setting Here.
        }

        /// <summary>
        /// If parent functionality is use this function is called when Out_parent is called.
        /// </summary>
        public override void ParentOutFunction()
        {
        }

        public override void OnBeforeSerialize()
        {
        }

        public override void OnAfterDeSerialize()
        {
        }

        public override void OnReset()
        {
        }
    }
}

#endif