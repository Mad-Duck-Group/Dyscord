namespace TaylorMadeCode.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface that needs to be used for any code to utilize the TMC_Editor System.
    /// </summary>
    public interface TMC_IObject
    {
        /// <summary> /// Boolean to Track if the Script Has Been Setup /// </summary>
        bool mb_HasBeenSetup { get; set; }

        /// <summary> /// Dictionary that is used to access the settings set in the custom TMC UI System /// </summary>
        Dictionary<string, bool> m_UIChoices { get; set; }

        /// <summary> /// A string that gets Json saved to it to allow the Custom UI state to be saved /// </summary>
        string ms_UIJson { get; set; }

#if UNITY_EDITOR
        /// <summary> /// Root of the Custom Script UI Tree /// </summary>
        TMC_Tree_Node m_UIRoot { get; set; }
#endif

        /// <summary> /// Every Custom Editor Script should contain some element of setup /// </summary>
        abstract void StartFunction();

        abstract void OnBeforeSerialize();

        abstract void OnAfterDeserialize();

        abstract void SetupScript();

        abstract void RemoveScript();

        /// <summary> /// Reset Function that gets called when reset script is called, Below just handles the reset for the UI side. /// </summary>
        public void Reset()
        {
            //Clear the dictionary the contains all the UI states
            m_UIChoices.Clear();

#if UNITY_EDITOR
            //Is there a custom editor?
            if (m_UIRoot != null)
            {
                //Try and get the two common parent elements.
                TMC_UXML l_temp = TMC_Editor.GetFistInstanceOfTMC_UXML<TMC_UXML>(m_UIRoot);
                m_UIRoot.CallOnResetForAllChildren();
                m_UIRoot.OnBeforeSerializeChildrenRecursively();
                m_UIRoot.MarkDirty();
            }
#endif
            //Tell the scripts its no longer setup
            mb_HasBeenSetup = false;
        }
    }
}