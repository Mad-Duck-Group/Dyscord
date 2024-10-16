namespace TaylorMadeCode.Core
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    
    [CustomEditor(typeof(TMC_Swap_Out_Model_Add_Scripts))]
    public class TMC_Swap_Out_Model_Add_Scripts_GUI : Editor
    {
        public TMC_Swap_Out_Model_Add_Scripts m_self;

        private void Awake()
        {
            m_self = (TMC_Swap_Out_Model_Add_Scripts)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            TMC_Editor.Begin(m_self, root, TMC.ProductCatogory.NONE);
            TMC_Editor.Create_A_TMC_Title(m_self, "Setup Object", null);
            TMC_Editor.In_Parent();

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Setup Fresh Object", false, true, "Setup To Match A Object");
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_Button(m_self.SetupFreshObject, "SetupFreshObject", "Setup Current Object For Rendering");
            TMC_Editor.Out_Parent();

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Setup To Match A Object", false, true, "Setup Fresh Object");
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_ObjectField<GameObject>(m_self.GameObjectToSwapWith, "ObjectToCopyFrom", (evt) => { m_self.GameObjectToSwapWith = evt; });
            TMC_Editor.Create_A_Button(m_self.SetupToMatchAObject, "SetupToMatchAObject");
            TMC_Editor.Out_Parent();

            TMC_Editor.Out_Parent();
            TMC_Editor.End(m_self);

            return root;
        }
    }
}