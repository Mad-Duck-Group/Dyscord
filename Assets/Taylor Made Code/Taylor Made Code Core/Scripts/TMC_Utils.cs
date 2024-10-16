namespace TaylorMadeCode.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class JsonableListWrapper<T>
    {
        public List<T> m_list;

        public JsonableListWrapper(List<T> a_list) => this.m_list = a_list;
    }

    public class TMC_Lerp_Basic_Internal
    {
        private float mf_StartTime = 0.0f;
        private float mf_Speed = 0.0f;
        private float mf_TotalDistance = 0.0f;
        private Vector3 m_DestinationPosition;
        private Vector3 m_InitialPosition;
        private bool mb_IsLerpActive = false;
        private bool mb_ShouldRotateToSeeDestination = true;
        private float mf_MaxRotationPerFrame = 5.0f;
        private GameObject m_GameObjectToLerp;

        /// <summary>
        /// This function setups and starts the internal lerp class.
        /// </summary>
        /// <param name="af_MovementSpeed">The movement speed along the path of the lerp</param>
        /// <param name="a_Start">Start point of the lerp</param>
        /// <param name="a_End">End point of the lerp</param>
        /// <param name="ar_gameObject">Game Object to move when Lerping</param>
        /// <param name="af_MaxRotationPerFrame">How fast should the object rotate per frame to face the end node. 0 and below means the rotation is off 360 is max speed</param>
        public void StartLerp(float af_MovementSpeed, Vector3 a_Start, Vector3 a_End, ref GameObject ar_gameObject, float af_MaxRotationPerFrame = 10.0f)
        {
            mb_IsLerpActive = true;
            mf_Speed = af_MovementSpeed;
            mf_MaxRotationPerFrame = af_MaxRotationPerFrame;
            m_DestinationPosition = a_End;
            m_InitialPosition = a_Start;
            //If the max rotation per frame is below 0 aka a negative then the user doesnt want rotation to occur. 
            mb_ShouldRotateToSeeDestination = af_MaxRotationPerFrame > 0;

            mf_StartTime = Time.time;
            mf_TotalDistance = Vector3.Distance(m_DestinationPosition, m_InitialPosition);
            m_GameObjectToLerp = ar_gameObject;
        }

        /// <summary>
        /// Call this function to stop the lerp instantly
        /// </summary>
        public void StopLerp() => mb_IsLerpActive = false;

        /// <summary>
        /// Update call for the lerp class, this function actually calculates the movement and rotation for the object. 
        /// </summary>
        public void Update()
        {
            if (m_GameObjectToLerp != null)
            {
                if (mb_IsLerpActive)
                {
                    float lf_currentDuration = (Time.time - mf_StartTime) * mf_Speed;
                    float lf_positionFraction = lf_currentDuration / mf_TotalDistance;
                    m_GameObjectToLerp.transform.position = Vector3.Lerp(m_InitialPosition, m_DestinationPosition, lf_positionFraction);

                    // Calculate the direction from the current position to the destination position
                    Vector3 l_targetDirection = m_DestinationPosition - m_GameObjectToLerp.transform.position;

                    if (mb_ShouldRotateToSeeDestination && l_targetDirection != Vector3.zero)
                    {
                        // Smoothly rotate towards the target direction
                        Quaternion targetRotation = Quaternion.LookRotation(l_targetDirection);
                        m_GameObjectToLerp.transform.rotation = Quaternion.RotateTowards(m_GameObjectToLerp.transform.rotation, targetRotation, mf_MaxRotationPerFrame);
                    }
                }

                if (Vector3.Distance(m_DestinationPosition, m_GameObjectToLerp.transform.position) < 0.01f)
                    StopLerp();
            }
        }

        public bool IsLerpActive() => mb_IsLerpActive;
    }

    public static class TMC_Editor_Utils
    {
        //Constant directory's used to get assets. This allow code to be neater and allows file structure to be changed without breaking everything
        //This is the one exception to the rule as its designed for the code names to read as the selected directories while allowing quick changes to be possible.
        public const string DIR_TMC = "Assets/Taylor Made Code/";

        public const string DIR_TMC_CORE = DIR_TMC + "/Taylor Made Code Core";
        public const string DIR_TMC_ASSETS = DIR_TMC_CORE + "/Art";
        public const string DIR_TMC_UI = DIR_TMC_CORE + "/Scripts/UI";

        public const string DIR_TMC_IMAGES = DIR_TMC_ASSETS + "/Sprites";
        public const string DIR_TMC_FONTS = DIR_TMC_ASSETS + "/Fonts";

        public const string DIR_TMC_UXML = DIR_TMC_UI + "/UXML";
        public const string DIR_TMC_USS = DIR_TMC_UI + "/USS";

        public const string DIR_TMC_OPTION_BODY_UXML = DIR_TMC_UXML + "/TMC_Option_Body.uxml";
        public const string DIR_TMC_OPTION_BODY_USS = DIR_TMC_USS + "/TMC_Option_Body.uss";

        public const string DIR_TMC_TITLE_BAR_UXML = DIR_TMC_UXML + "/TMC_Title_Bar.uxml";
        public const string DIR_TMC_TITLE_BAR_USS = DIR_TMC_USS + "/TMC_Title_Bar.uss";

        public const string DIR_TMC_SETUP_SCRIPT_UXML = DIR_TMC_UXML + "/TMC_Setup_Script.uxml";
        public const string DIR_TMC_SETUP_SCRIPT_USS = DIR_TMC_USS + "/TMC_Setup_Script.uss";

        /// <summary>
        /// Provides a completely unique id using the Cantor Paring Method
        /// </summary>
        /// <returns> returns a unique id where a,b != b,a </returns>
        public static string GetUniqueID(int ai_ParentCount, int ai_ChildCount)
        {
            string ls_ID = new string("");

            int li_a = ai_ParentCount, li_b = ai_ChildCount;
            float lf_ParingFunctionOutput = 0;

            //Cantor Pairing Function to generate a unique number from a,b where b,a doesn't produce the same output.
            //This is also the most efficient method for computers as its 4 adds, 1 Times and one bit shift
            lf_ParingFunctionOutput = (((li_a + li_b) * (li_a + li_b + 1)) / 2) + li_b;

            ls_ID = lf_ParingFunctionOutput.ToString();

            return ls_ID;
        }

        /// <summary>
        /// Converts Cantor Paring method number to the original a,b numbers
        /// </summary>
        /// <param name="ai_Name">The VisualElements Name aka the ID</param>
        /// <param name="ai_ParentCount">Output of the location in the ParentList</param>
        /// <param name="ai_VisualElementCount">Output of the location in the VisualElementList</param>
        public static void GetRevertedID(int ai_Name, out int ai_ParentCount, out int ai_VisualElementCount)
        {
            //Revert the Cantor calculation done in GetUniqueID
            int li_Temp = (int)Mathf.Floor((-1 + Mathf.Sqrt(1 + 8 * ai_Name) / 2));
            ai_ParentCount = li_Temp * (li_Temp + 3) / 2 - ai_Name;
            ai_VisualElementCount = ai_Name - li_Temp * (li_Temp + 1) / 2;
        }

        /// <summary>
        /// Function to take in two visual element names and spit out the which on is earler in the tree.
        /// </summary>
        /// <param name="as_ID1">Visual element 1 name</param>
        /// <param name="as_ID2">Visual element 2 name</param>
        /// <returns> 0 if error, 1 in firsts element is first, 2 if second element is first.</returns>
        public static int WhatIDComeFirst(string as_ID1, string as_ID2)
        {
            int parentCount1 = 0, SiblingCount1 = 0;
            string[] splitName1 = as_ID1.Split('_');
            string IDcharacters1 = splitName1[splitName1.Length - 1];
            GetRevertedID(Int32.Parse(IDcharacters1), out parentCount1, out SiblingCount1);

            int parentCount2 = 0, SiblingCount2 = 0;
            string[] splitName2 = as_ID1.Split('_');
            string IDcharacters2 = splitName2[splitName2.Length];
            GetRevertedID(Int32.Parse(IDcharacters2), out parentCount2, out SiblingCount2);

            if (parentCount1 < parentCount2)
                return 1;
            else if (parentCount2 < parentCount1)
                return 2;
            else
            {
                if (SiblingCount1 < SiblingCount2)
                    return 1;
                else if (SiblingCount2 < SiblingCount1)
                    return 2;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Allows easy method to open Internet link
        /// </summary>
        /// <param name="as_URL">url to open</param>
        public static void OpenLink(string as_URL)
        {
            System.Diagnostics.Process.Start(as_URL);
        }

        /// <summary>
        /// Process, a camelCase var name to a English var name.
        /// This just makes setting the name easier as 99% of the time they are the same just need to convert.
        /// </summary>
        /// <param name="as_camelCaseInput">Camel Case String to be converted</param>
        /// <returns>Converted String</returns>
        public static string ProcessCamelCaseToString(string as_camelCaseInput)
        {
            if (as_camelCaseInput.Length < 1)
            {
                Debug.LogError("A string that was passed into ProcessCamelCaseToString didn't contain anything, thus the call to the function is useless. Please remove.");
                return as_camelCaseInput;
            }

            string ls_name = "";

            foreach (char lc_letter in as_camelCaseInput)
            {
                //Is the letterCapital based on the ASCII code where A = 41 and Z = 90. Lower case letters are from a = 91 and z = 122
                //if the letter is capital add a space then the letter otherwise just add the letter
                if (lc_letter > 40 && lc_letter < 91)
                {
                    ls_name += " " + lc_letter;
                }
                else
                    ls_name += lc_letter;
            }

            return ls_name;
        }

        /// <summary>
        /// Loads in a sprite from a directory and return the created sprite. 
        /// </summary>
        /// <param name="as_imageLocation">Directory of the Image to use as a sprite</param>
        /// <param name="as_name">Name of the constructed sprite</param>
        /// <returns>the created sprite</returns>
        public static Sprite LoadSprite(string as_imageLocation, string as_name = "Sprite")
        {
            Texture2D l_Texture = new Texture2D(1024, 1024);
            l_Texture.LoadImage(System.IO.File.ReadAllBytes(as_imageLocation));

            //Fail Out if no data is present
            if (l_Texture.GetRawTextureData() == null)
            {
                Debug.LogError("TMC Core has encountered an issue looking for file:" + as_imageLocation + " Please ensure this file hasn't been deleted. \n This error can be ignored as this will only effect TMC tools and editor script GUI");
                return null;
            }

            Sprite l_sprite = Sprite.Create(l_Texture, new Rect(0, 0, l_Texture.width, l_Texture.height), Vector2.zero, 200, 1, SpriteMeshType.FullRect);
            l_sprite.name = as_name;
            
            return l_sprite;
        }
    }
}