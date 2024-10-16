namespace TaylorMadeCode.Core
{
    using System.Collections.Generic;

    using UnityEngine;

    using UnityEngine.UIElements;

    /// <summary> /// This Class is a wrapper that provided support to use Unity derived VisualElements and TMC Derived TMC_UXML classes together in one UI tree. /// </summary>
    public class TMC_Tree_Node
    {
        /// <summary> /// This enum list all elements that TMC_UI_TREE_NODE supports /// </summary>
        public enum e_TypeOfUI
        {
            NONE = -1,

            TMC_START,
            TMC_UXML,
            TMC_OPTION_BODY,
            TMC_TITLE,
            TMC_LIST,

            TMC_DUAL_PARENT_START,
            TMC_SETUP_SCRIPT,
            TMC_DUAL_PARENT_END,

            TMC_END,

            UNITY_START,
            UNITY_VISUALELEMENT,
            UNITY_LABEL,
            UNITY_BUTTON,
            UNITY_TOGGLE,
            UNITY_SCROLLER,
            UNITY_TEXTFIELD,
            UNITY_FOLDOUT,
            UNITY_SLIDER,
            UNITY_SLIDERINT,
            UNITY_MINMAXSLIDER,
            UNITY_PROGRESSBAR,
            UNITY_DROPDOWN,
            UNITY_RADIOBUTTON,
            UNITY_RADIOBUTTONGROUP,
            UNITY_INTERGERFIELD,
            UNITY_FLOATFIELD,
            UNITY_LONGFIELD,
            UNITY_PROGRSSBAREDITOR,
            UNITY_VECTOR2FIELD,
            UNITY_VECTOR3FIELD,
            UNITY_VECTOR4FIELD,
            UNITY_RECTFIELD,
            UNITY_BOUNDSFIELD,
            UNITY_VECTOR2INTFIELD,
            UNITY_VECTOR3INTFIELD,
            UNITY_RECTINTFIELD,
            UNITY_BOUNDSINTFIELD,
            UNITY_OBJECTFIELD,
            UNITY_COLOURFIELD,
            UNITY_CURVEFIELD,
            UNITY_GRADIANTFIELD,
            UNITY_ENUMFIELD,
            UNITY_TAGFIELD,
            UNITY_MASKFIELD,
            UNITY_LAYERFIELD,
            UNITY_LAYERMASKFIELD,
            UNITY_HELPBOX,
            UNITY_END,

            END_OF_ENUM,
        }

        /// <summary> /// The TMC_Tree_Node parent reference /// </summary>
        public TMC_Tree_Node m_Parent = null;

        /// <summary> /// Storing the wrapped element so some TMC_UXML or UNITY Visual Element class /// </summary>
        public System.Object m_Self = null;

        /// <summary> /// List of all TMC_Tree_Node Children /// </summary>
        public List<TMC_Tree_Node> m_ListOfChildren = null;

        /// <summary> /// The Type of the TMC_Tree_Node as enum /// </summary>
        public e_TypeOfUI me_Type = e_TypeOfUI.NONE;

        /// <summary> /// Bool to see if the element is Part of the TMC_Editor system /// </summary>
        public bool mb_IsRootTMCEditor = false;

        /// <summary> /// reference to the TMC_UXML class as occasionally the m_self will be part of a direct TMC_UXML Class but not be one itself. /// </summary>
        public TMC_UXML m_TmcUxmlClass = null;

        /// <summary>
        /// Constructor for TMC_Tree_Node
        /// </summary>
        /// <param name="a_Self">The UI element that this is wrapping</param>
        /// <param name="ae_Type">The type of ui that the class is wrapping</param>
        public TMC_Tree_Node(System.Object a_Self, e_TypeOfUI ae_Type = e_TypeOfUI.NONE)
        {
            m_Self = a_Self;
            me_Type = ae_Type;
            m_ListOfChildren = new List<TMC_Tree_Node>();
        }

        /// <summary>
        /// Allow access of children easier by overriding the [] operator
        /// </summary>
        /// <param name="key">the child to get from 0 - amountOfChildren </param>
        /// <returns>the wanted child instance</returns>
        public TMC_Tree_Node this[int key]
        {
            get => m_ListOfChildren[key];
        }

        /// <summary> /// This function will mark all GUI elements dirty including self and all child elements /// </summary>
        public void MarkDirty()
        {
            MarkSelfDirty();
            //Then call this function to all children.
            foreach (TMC_Tree_Node Node in m_ListOfChildren)
                Node.MarkDirty();
        }

        /// <summary> /// Marks only itself dirty. /// </summary>
        public void MarkSelfDirty()
        {
            if (m_Self == null)
                Debug.LogError("m_self is null when marking ui dirty");

            //Depending on its type mark the visual element to be dirty
            GetUnityVisualElement().MarkDirtyRepaint();
        }

        /// <summary>
        /// If this is TMC_UXML directly or just a ROOT_TMC_EDITOR then flag as a TMC_UXML
        /// </summary>
        /// <returns> true if TMC_UXML or ROOT_TMC_UXML</returns>
        public bool IsSelfTMC_UXML()
        {
            return m_Self is TMC_UXML || mb_IsRootTMCEditor;
        }

        /// <summary>
        /// This gets self as a TMC_UXML from either the TMC_UXML_Class or m_self
        /// </summary>
        /// <returns>Returns the correctly formatted TMC_UXML</returns>
        public TMC_UXML Get_Self_As_TMC_UXML()
        {
            if (mb_IsRootTMCEditor)
                return m_TmcUxmlClass;
            else
                return m_Self as TMC_UXML;
        }

        /// <summary>
        /// Templated version of Get_Self_As_TMC_UXML to ensure cleaner code going forward
        /// </summary>
        /// <typeparam name="T"> TMC_UXML derived class </typeparam>
        /// <returns>This returns the TMC_UXML derived class with an attempt to interpreted as the type provided</returns>
        public T Get_Self_As_TMC_UXML<T>() where T : TMC_UXML
        {
            if (mb_IsRootTMCEditor)
                return m_TmcUxmlClass as T;
            else
                return m_Self as T;
        }

        /// <summary>
        /// Is Self Derived From a Visual element
        /// </summary>
        /// <returns>true if m_self is derived from VisualElement</returns>
        public bool IsSelfVisualElement()
        {
            return m_Self is VisualElement;
        }

        /// <summary>
        /// Is m_self TMC_UXML or VisualElement Based? This is meant to be a wide range All or nothing check. As if this returns back false something has gone very wrong
        /// </summary>
        /// <returns>returns true if m_self is TMC_UXML or VisualElement based otherwise it returns false</returns>
        public bool IsSelfUxmlOrVisualElement()
        {
            return IsSelfTMC_UXML() || IsSelfVisualElement();
        }

        /// <summary>
        /// Function that allocated the m_parent Var and links the Visual Elements together to allow rendering.
        /// </summary>
        /// <param name="parent">The TMC_Tree_Node to be set as parent for both VisualElement and TMC_Tree_Node Structures</param>
        public void SetParentChildRelationShip(ref TMC_Tree_Node ar_parent)
        {
            //TMC Tree Parent Child Setup
            m_Parent = ar_parent;
            m_Parent.m_ListOfChildren.Add(this);

            //Unity Tree Parent Child Setup
            if (m_Parent.IsSelfTMC_UXML())
            {
                //If This is TMC Custom type then access Root_Unity from TMC_UXML base class
                if (IsSelfTMC_UXML())
                {
                    (m_Parent.m_Self as TMC_UXML).m_RootTmcEditor.GetUnityVisualElement().Add((m_Self as TMC_UXML).m_RootUnity);
                }
                //If This is a visual element then cast to Visual element
                else if (IsSelfVisualElement())
                {
                    (m_Parent.m_Self as TMC_UXML).m_RootTmcEditor.GetUnityVisualElement().Add((m_Self as VisualElement));
                }
                else
                    Debug.LogError("Critical Error Please contact TaylorMadeCode Support");
            }
            //If Parent is type of unity then handle parent as such
            else if (m_Parent.IsSelfVisualElement())
            {
                //If This is type of tmc then handle as such
                if (IsSelfTMC_UXML())
                {
                    (m_Parent.m_Self as VisualElement).Add((m_Self as TMC_UXML).m_RootUnity);
                }
                //If this is type of Visual element then treat it as such.
                else if (IsSelfVisualElement())
                {
                    (m_Parent.m_Self as VisualElement).Add((m_Self as VisualElement));
                }
                else
                    Debug.LogError("Critical Error Please contact TaylorMadeCode Support");
            }
            else
                Debug.LogError("Critical Error Please contact TaylorMadeCode Support");
        }

        /// <summary>
        /// Gets the TMC_UI_TREE_Node as a unity element, either directly or through TMC_UXML
        /// </summary>
        /// <returns>The TMC_UI_TREE_Node as a Unity Visual Element</returns>
        public VisualElement GetUnityVisualElement()
        {
            if (me_Type > e_TypeOfUI.UNITY_START && me_Type < e_TypeOfUI.UNITY_END)
                return m_Self as VisualElement;
            else if (me_Type > e_TypeOfUI.TMC_START && me_Type < e_TypeOfUI.TMC_END)
                return (m_Self as TMC_UXML).m_RootUnity;
            else
            {
                Debug.LogError("Attempted To Get TMC_UXML as a Visual element however its not a TMC UXML or Visual element type");
                return null;
            }
        }

        /// <summary>
        /// Templated Version of the GetUnityVisualElement Function
        /// </summary>
        /// <typeparam name="T">Type to return</typeparam>
        /// <returns>Returns the same as the Standard GetUnityVisualElement but does a cast to type T before returning</returns>
        public T GetUnityVisualElement<T>() where T : VisualElement
        {
            if (me_Type > e_TypeOfUI.UNITY_START && me_Type < e_TypeOfUI.UNITY_END)
                return m_Self as T;
            else if (me_Type > e_TypeOfUI.TMC_START && me_Type < e_TypeOfUI.TMC_END)
                return (m_Self as TMC_UXML).m_RootUnity as T;
            else
            {
                Debug.LogError("Attempted To Get TMC_UXML as a Visual element however its not a TMC UXML or Visual element type");
                return null;
            }
        }

        /// <summary>
        /// Function that calls all child elements OnBeforeSerialize
        /// </summary>
        public void OnBeforeSerializeChildrenRecursively()
        {
            //Then call the parents
            if (IsSelfTMC_UXML())
                (m_Self as TMC_UXML).OnBeforeSerialize();

            //Call the children first
            foreach (TMC_Tree_Node l_Node in m_ListOfChildren)
                l_Node.OnBeforeSerializeChildrenRecursively();
        }

        /// <summary>
        /// Function that calls all child elements OnAfterDeseralized
        /// </summary>
        public void OnAfterDeserializeChildrenRecursively()
        {
            //Then call the parents
            if (IsSelfTMC_UXML())
                (m_Self as TMC_UXML).OnAfterDeSerialize();

            //Call the children first
            foreach (TMC_Tree_Node l_Node in m_ListOfChildren)
                l_Node.OnAfterDeserializeChildrenRecursively();
        }

        /// <summary>
        /// Call OnReset for all UXML elements in child methods
        /// </summary>
        public void CallOnResetForAllChildren()
        {
            //Then call the parents
            if (IsSelfTMC_UXML())
                (m_Self as TMC_UXML).OnReset();

            foreach (TMC_Tree_Node l_Node in m_ListOfChildren)
                l_Node.CallOnResetForAllChildren();
        }
    }
}