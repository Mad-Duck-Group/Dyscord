namespace TaylorMadeCode.Core
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [AddComponentMenu("Taylor Made Code/Core/Swap Out Model And Scripts")]
    public class TMC_Swap_Out_Model_Add_Scripts : TMC_MonoBehaviour
    {
        public GameObject GameObjectToSwapWith;

        public override void StartFunction()
        {
            throw new System.NotImplementedException();
        }

        public void SetupToMatchAObject()
        {
            if (GameObjectToSwapWith == null)
            {
                Debug.LogError("Please select an GameObject is selected to copy before pressing Setup To Match A Object Button");
                return;
            }

            Instantiate(GameObjectToSwapWith, this.transform);

            DestroyImmediate(this);
        }

        public void SetupFreshObject()
        {
            this.gameObject.AddComponent<MeshFilter>();
            this.GetComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            this.gameObject.AddComponent<MeshRenderer>();

            DestroyImmediate(this);
        }
    }
}