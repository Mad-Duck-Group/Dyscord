namespace TaylorMadeCode.Core
{
    using UnityEngine.UIElements;

    /// <summary>
    /// The class is the basis of all UXML scripts with all the required classes that all UXML classes need to work with TMC_EditorV2
    /// </summary>
    public abstract class TMC_UXML
    {
        /// <summary>
        /// Root_Unity is the Visual element that unity considers the root and is the "true root of the UI tree". Use this for rendering and ending the Unity Visual Element Tree
        /// </summary>
        public abstract VisualElement m_RootUnity { get; set; }

        /// <summary>
        /// This Root TMC_Editor is the element that is used as the root for Parent Child relations ships. This can be the same as Root_TMC_Script or can be different depending on the TMC_UXML derived class
        /// </summary>
        public abstract TMC_Tree_Node m_RootTmcEditor { get; set; }

        /// <summary>
        /// This function provides reference the Scripts TMC Mono class to alter and store UI changes in the m_UIChoices dictionary
        /// </summary>
        protected abstract TMC_IObject m_ScriptTmcMono { get; set; }

        /// <summary>
        /// Root TMC Tree is the tree node to use when traversing the TMC UI Tree
        /// </summary>
        public abstract TMC_Tree_Node m_RootTmcTree { get; set; }

        /// <summary>
        /// This overridden function allows the same assess to child elements as the standard visual elements do.
        /// </summary>
        /// <param name="ai_key">what child to assess index 0 - AmountOfChildren</param>
        /// <returns>the TMC_Tree_Node instance that is the child element</returns>
        public abstract TMC_Tree_Node this[int ai_key] { get; }

        /// <summary>
        /// The instantiate function is where all the initialization code for TMC_UXML classes go
        /// </summary>
        /// <param name="ar_Parent">The parent TMC_Tree_Node instance</param>
        /// <param name="ar_Self">its own TMC_Tree_Node class</param>
        /// <param name="a_AdditionalParameters">This is different for all TMC_UXML scripts</param>
        /// <returns> the finalized TMC_UXML class</returns>
        public abstract TMC_UXML Instantiate(ref TMC_Tree_Node ar_Parent, ref TMC_Tree_Node ar_Self, params object[] a_AdditionalParameters);

        /// <summary>
        /// This function gets called automatically when the TMC_UXML element is finished.
        /// </summary>
        public abstract void EndOfCreationFunction();

        /// <summary>
        /// This function gets called if there are any built child elements. This allows the TMC_UXML to adapt to child elements an example is TMC_Title
        /// </summary>
        public abstract void ParentOutFunction();

        /// <summary>
        /// The function allows the assess of the state of the UI. For example Option body returns if its minimized or maximized
        /// </summary>
        /// <returns></returns>
        public abstract System.Object GetValue();

        /// <summary>
        /// This function is called when the Script reset is called
        /// </summary>
        public abstract void OnReset();

        /// <summary>
        /// This functions is called before the script is serialized
        /// </summary>
        /// <param name="Json"> The Json that the UI states will be saved to </param>
        /// <returns></returns>
        public abstract void OnBeforeSerialize();

        /// <summary>
        /// This function is called after the script is DeSerialized
        /// </summary>
        /// <param name="Json">The Json that the UI states are saved to</param>
        /// <returns></returns>
        public abstract void OnAfterDeSerialize();
    }
}