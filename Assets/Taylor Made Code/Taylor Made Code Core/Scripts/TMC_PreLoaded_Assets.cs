#if UNITY_EDITOR

namespace TaylorMadeCode.Core.PreloadedAssets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utilities;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Class that handles loading in assets used for UI.
    /// </summary>
    public static class TMC_Editor_Assets
    {
        /// <summary>
        /// enum of every pre-loaded image that is apart of TMC_CORE.
        /// </summary>
        public enum e_PreLoadedImages
        {
            NONE = -1,

            TMC_CORE,

            TMC_LOGO,

            TMC_UI_ARROW_RIGHT,
            TMC_UI_DOCUMENTATION,
            TMC_UI_LANGUAGES_SELECTOR,
            TMC_UI_MINUS,
            TMC_UI_PLUS,
            TMC_UI_REFRESH,
            TMC_UI_SAVE,
            TMC_UI_TUTORIAL_VIDEOS,
            TMC_UI_STOP,
            TMC_UI_RANDOM_PLAY,
            TMC_UI_PLAY,
            TMC_UI_PAUSE,
            TMC_UI_ON,
            TMC_UI_OFF,

            TMC_UI_DARK_SCRIPT_BASE,
            TMC_UI_DARK_TITLE_HEADER_BASE,
            TMC_UI_DARK_TITLE_OPTION_BUTTON_SELECTED,
            TMC_UI_DARK_TITLE_OPTION_BUTTON_UNSELECTED,
            TMC_UI_DARK_OPTION_BODY_BASE,
            TMC_UI_DARK_OPTION_BODY_HEADER_BASE,

            TMC_UI_LIGHT_SCRIPT_BASE,
            TMC_UI_LIGHT_TITLE_HEADER_BASE,
            TMC_UI_LIGHT_TITLE_OPTION_BUTTON_SELECTED,
            TMC_UI_LIGHT_TITLE_OPTION_BUTTON_UNSELECTED,
            TMC_UI_LIGHT_OPTION_BODY_BASE,
            TMC_UI_LIGHT_OPTION_BODY_HEADER_BASE,

            END_OF_ENUM,
        }

        /// <summary>
        /// Enum containing all pre-loaded fonts contained in TMC_Core
        /// </summary>
        private enum e_PreLoadedFonts
        {
            NONE = -1,

            ROBOTO_BLACK = 0,
            ROBOTO_BLACK_ITALIC,
            ROBOTO_BOLD_ITALIC,
            ROBOTO_BOLD,
            ROBOTO_ITALIC,
            ROBOTO_LIGHT_ITALIC,
            ROBOTO_LIGHT,
            ROBOTO_MEDIUM_ITALIC,
            ROBOTO_MEDIUM,
            ROBOTO_REGULAR,
            ROBOTO_THIN_ITALIC,
            ROBOTO_THIN,

            END_OF_ENUM,
        }

        /// <summary>
        /// Pre-made style sheets Taylor made code contains
        /// </summary>
        public enum e_PreMadeStyles
        {
            NONE = -1,

            TMC_USS,

            END_OF_ENUM,
        }

        /// <summary>
        /// A list of all loaded in sprites
        /// </summary>
        public static List<Sprite> m_PreLoadedSpritesList { get; private set; } = new List<Sprite>();

        /// <summary>
        /// A list of all loaded in Fonts
        /// </summary>
        public static List<Font> m_PreLoadedFont { get; private set; } = new List<Font>();

        /// <summary>
        /// A list of all loaded in style sheets
        /// </summary>
        public static List<StyleSheet> m_PreloadedStyleSheets { get; private set; } = new List<StyleSheet>();

        /// <summary>
        /// This function calls all the separate asset loading functions, Each of said functions load Sprites, Fonts or StyleSheets
        /// </summary>
        /// <returns>True if loading is successful false if any assets fail</returns>
        public static bool LoadPreLoadedAssets()
        {
            if (!LoadSprites())
            {
                Debug.LogWarning("Issue with loading sprites, please contact Taylor Made Code support for assistance");
                return false;
            }

            if (!LoadFonts())
            {
                Debug.LogWarning("Issue with loading fonts, please contact Taylor Made Code support for assistance");
                return false;
            }

            if (!LoadStyleSheets())
            {
                Debug.LogWarning("Issue with loading StyleSheets, please contact Taylor Made Code support for assistance");
                return false;
            }

            //If everything has been successfully loaded then return true
            return true;
        }

        /// <summary>
        /// Specific function loading all TMC_Editor Sprite Assets into memory
        /// </summary>
        /// <returns>true if loaded assets or if they are already loaded in. False if any issues are encountered</returns>
        public static bool LoadSprites()
        {
            //Check to see if there is populated list and ensure that list doesn't equal null
            if (m_PreLoadedSpritesList.Count > 0 && m_PreLoadedSpritesList.Any(element => element != null))
                return true;
            else
                m_PreLoadedSpritesList.Clear();

            //-- Load All Wanted Images --//
            for (int li_i = 0; li_i < (int)e_PreLoadedImages.END_OF_ENUM; li_i++)
            {
                //Process the file name to load
                String ls_FileToLoadName = "/" + ((e_PreLoadedImages)li_i).ToString() + ".png";
                String ls_FileToLoadLocation = TMC_Editor_Utils.DIR_TMC_IMAGES + ls_FileToLoadName;

                //Load in the asset size and set the texture to be 1024 by 1024
                Texture2D l_Texture = new Texture2D(1024, 1024);
                l_Texture.LoadImage(System.IO.File.ReadAllBytes(ls_FileToLoadLocation));

                //Fail Out if no data is present
                if (l_Texture.GetRawTextureData() == null)
                {
                    Debug.LogWarning("TMC Core has encountered an issue looking for file:" + ls_FileToLoadName + " Please ensure this file hasn't been deleted. \n This error can be ignored as this will only effect TMC tools and editor script GUI");
                    return false;
                }

                Sprite l_sprite = Sprite.Create(l_Texture, new Rect(0, 0, l_Texture.width, l_Texture.height), Vector2.zero, 200, 1, SpriteMeshType.FullRect);

                // if successful then save sprite to an array
                m_PreLoadedSpritesList.Add(l_sprite);
            }

            return true;
        }

        /// <summary>
        /// Specific function loading all TMC_Editor Fonts Assets into memory
        /// </summary>
        /// <returns>true if loaded assets or if they are already loaded in. False if any issues are encountered</returns>
        public static bool LoadFonts()
        {
            //Check to see if there is populated list and ensure that list doesn't equal null
            if (m_PreLoadedFont.Count > 0 && m_PreLoadedFont.Any(element => element != null))
                return true;
            else
                m_PreLoadedFont.Clear();

            //-- Load All Wanted Fonts --/
            for (int li_i = 0; li_i < (int)e_PreLoadedFonts.END_OF_ENUM; li_i++)
            {
                //construct file name
                String ls_FileToLoadName = "/" + ((e_PreLoadedFonts)li_i).ToString() + ".tff";
                String ls_FileToLoadLocation = TMC_Editor_Utils.DIR_TMC_FONTS + ls_FileToLoadName;

                Font ls_Font = new Font(ls_FileToLoadLocation);

                //Fail out if no data is present
                if (ls_Font.characterInfo == null)
                {
                    Debug.LogWarning("TMC Core has encountered an issue looking for file: " + ls_FileToLoadLocation + " Please ensure this file hasn't been deleted \n This error can be ignored as it will only effect TMC tools and editor Script GUI");
                    return false;
                }

                //add font to the preloaded array
                m_PreLoadedFont.Add(new Font(ls_FileToLoadLocation));
            }

            return true;
        }

        /// <summary>
        /// Specific function loading all TMC_Editor Style-sheet Assets into memory
        /// </summary>
        /// <returns>true if loaded assets or if they are already loaded in. False if any issues are encountered</returns>
        public static bool LoadStyleSheets()
        {
            //Check to see if there is populated list and ensure that list doesn't equal null
            if (m_PreloadedStyleSheets.Count > 0 && m_PreloadedStyleSheets.Any(element => element != null))
                return true;
            else
                m_PreloadedStyleSheets.Clear();

            //-- Load all wanted Style-sheets --//
            for (int li_i = 0; li_i < (int)e_PreMadeStyles.END_OF_ENUM; li_i++)
            {
                //construct file name
                String ls_FileToLoadName = "/" + ((e_PreMadeStyles)li_i).ToString() + ".uss";
                String ls_FileToLoadLocation = TMC_Editor_Utils.DIR_TMC_USS + ls_FileToLoadName;

                StyleSheet l_StyleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(ls_FileToLoadLocation);
                if (l_StyleSheet == null)
                    Debug.LogError("TMC_OptionBody Cannot find: " + ls_FileToLoadLocation);

                //add font to the preloaded array
                m_PreloadedStyleSheets.Add(l_StyleSheet);
            }

            //If everything has been successfully loaded then return true
            return true;
        }

        /// <summary>
        /// Gets the passed in style sheet
        /// </summary>
        /// <param name="ae_Style">The Enum of the wanted Preloaded Style Sheet</param>
        /// <returns>The Loaded StyleSheet Instance</returns>
        public static StyleSheet GetStyle(e_PreMadeStyles ae_Style)
        {
            return m_PreloadedStyleSheets[Convert.ToInt32(ae_Style)];
        }
    }
}

#endif