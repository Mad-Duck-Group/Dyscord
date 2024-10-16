namespace TaylorMadeCode.FreeAudioManager
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    using TaylorMadeCode.Core;
    using TaylorMadeCode.Core.PreloadedAssets;

    /// <summary> /// This class is just used for custom GUI and should not be used by the end user /// </summary>
    [CustomEditor(typeof(TMC_Sound_Manager))]
    public class TMC_Sound_Manager_Simple_GUI : Editor
    {
        //- Allows Reference Back To Main Script -//
        public TMC_Sound_Manager m_self;

        public void Awake()
        {
            m_self = (TMC_Sound_Manager)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement l_rootInspector = new VisualElement();

            Texture2D l_Texture = new Texture2D(712, 712);
            l_Texture.LoadImage(System.IO.File.ReadAllBytes("Assets/Taylor Made Code/Free Sound Manager/Art/Sprites/TMC_FREE_AUDIO_MANAGER.png"));

            TMC_Editor.Begin(m_self, l_rootInspector, TMC.ProductCatogory.Free);
            TMC_Editor.Create_A_TMC_ScriptSetup(m_self, "Free Sound Manager", "Free Sound Manager requires some more setup. Like all TMC Products this is super easy to setup, Just click the button below this text called Setup Script");
            TMC_Editor.In_Parent();

            TMC_Editor.Create_A_TMC_Title(m_self, "Sound Manager", l_Texture, "Free_Image_Tint_Colour", "https://youtu.be/w9NV_N4kIDU", "https://docs.taylormadecode.com/Free_Sound_Manager/latest/");
            TMC_Editor.In_Parent();

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Control Panel", true);
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_Horizontal_Group(true, "Aligned_Middle|Horizontal_Group|Height_100px");
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_Button(() => { m_self.PlayWithNewRandomizedEffects(); }, "PlayButton", TMC_Editor_Assets.e_PreLoadedImages.TMC_UI_RANDOM_PLAY, true, "Column_5|Image_Scale_To_Fit|Border_Radius_25px|Colour_Design_LIGHT_1_Image_Tint_Colour");
            TMC_Editor.Create_A_Button(() => { m_self.Play(); }, "PlayButton", TMC_Editor_Assets.e_PreLoadedImages.TMC_UI_PLAY, true, "Column_5|Image_Scale_To_Fit|Border_Radius_25px|Colour_Design_LIGHT_1_Image_Tint_Colour");
            TMC_Editor.Create_A_Button(() => { m_self.Pause(); }, "PauseButton", TMC_Editor_Assets.e_PreLoadedImages.TMC_UI_PAUSE, true, "Column_5|Image_Scale_To_Fit|Border_Radius_25px|Colour_Design_LIGHT_1_Image_Tint_Colour");
            TMC_Editor.Create_A_Button(() => { m_self.Stop(); }, "StopButton", TMC_Editor_Assets.e_PreLoadedImages.TMC_UI_STOP, true, "Column_5|Image_Scale_To_Fit|Border_Radius_25px|Colour_Design_LIGHT_1_Image_Tint_Colour");
            TMC_Editor.Out_Parent();
            TMC_Editor.Create_A_Vertical_Space(25);

            TMC_Editor.Create_A_Slider(m_self.mf_WantedVolume, "Volume", (evt) => { m_self.SetVolume(evt); }, "Volume 0-1", 0, 1);
            TMC_Editor.Out_Parent();

            //---------------------------------------------------------------------------//

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Singular Audio File", false, true, "Multiple Audio File", true, true);
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_ObjectField<AudioSource>(m_self.m_Audio, "AudioToControl", (evt) => { m_self.m_Audio = evt; });
            TMC_Editor.Out_Parent();

            //---------------------------------------------------------------------------//

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Multiple Audio File", false, true, "Singular Audio File", true, false);
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_List_Of_UnityActions(m_self, serializedObject, "m_MultipleAudio");
            TMC_Editor.Out_Parent();

            //---------------------------------------------------------------------------//

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Delayed Sound Loop Control", false);
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_Float(m_self.mf_LoopTimeRangeStart, "MinimumAmountOfDelayInSeconds", (evt) => { m_self.mf_LoopTimeRangeStart = evt; });
            TMC_Editor.Create_A_Float(m_self.mf_LoopTimeRangeEnd, "MaximumAmountOfDelayInSeconds", (evt) => { m_self.mf_LoopTimeRangeEnd = evt; });
            TMC_Editor.Out_Parent();

            //---------------------------------------------------------------------------//

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Random Pitch", false);
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_Float(m_self.mf_PitchRangeStart, "RandomPitchRangeStart", (evt) => { m_self.mf_PitchRangeStart = evt; });
            TMC_Editor.Create_A_Float(m_self.mf_PitchRangeEnd, "RandomPitchRangeEnd", (evt) => { m_self.mf_PitchRangeEnd = evt; });
            TMC_Editor.Out_Parent();

            //---------------------------------------------------------------------------//

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Fade In", false);
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_Slider(m_self.mf_FadeInEnd, "FadeInEndsAt", (evt) => { m_self.mf_FadeInEnd = evt; }, "Fade In Ends at X% of the audio clips duration", 1, 99);
            TMC_Editor.Create_A_Slider(m_self.mf_SpeedOfFadeIn, "SpeedOfFadeIn", (evt) => { m_self.mf_SpeedOfFadeIn = evt; }, "Speed Of Fade In", 0, 20, 0, true);
            TMC_Editor.Out_Parent();

            //---------------------------------------------------------------------------//

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Fade Out", false);
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_Slider(m_self.mf_FadeOutStart, "FadeOutStartsAt", (evt) => { m_self.mf_FadeOutStart = evt; }, "Fade Out Starts at X% of the audio clips duration", 1, 99);
            TMC_Editor.Create_A_Slider(m_self.mf_SpeedOfFadeOut, "SpeedOfFadeOut", (evt) => { m_self.mf_SpeedOfFadeOut = evt; }, "Speed Of Fade Out", 0, 20, 0, true);
            TMC_Editor.Out_Parent();

            //---------------------------------------------------------------------------//

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Settings", true);
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_Toggle(m_self.mb_UseRandomisedPlayForWhenToStart, "UseRandomisedPlayForWhenToStart", (evt) => { m_self.mb_UseRandomisedPlayForWhenToStart = evt;});
            //is active, audio, start script on
            TMC_Editor.Create_A_Enum(m_self.me_StartScriptOn, "WhenToStart", (evt) => { m_self.me_StartScriptOn = evt; });
            TMC_Editor.Out_Parent();

            //---------------------------------------------------------------------------//

            TMC_Editor.Create_A_TMC_Option_Body(m_self, "Events", true);
            TMC_Editor.In_Parent();
            TMC_Editor.Create_A_Vertical_Space(); //m_MultipleAudio
            TMC_Editor.Create_A_List_Of_UnityActions(m_self, serializedObject, "m_ScriptStartingEvents");
            TMC_Editor.Create_A_List_Of_UnityActions(m_self, serializedObject, "m_ScriptDuringEvents");
            TMC_Editor.Create_A_List_Of_UnityActions(m_self, serializedObject, "m_ScriptEndingEvents");
            TMC_Editor.Create_A_Vertical_Space();
            TMC_Editor.Out_Parent();

            TMC_Editor.Out_Parent();
            TMC_Editor.Out_Parent();
            TMC_Editor.End(m_self);

            return l_rootInspector;
        }
    }
}