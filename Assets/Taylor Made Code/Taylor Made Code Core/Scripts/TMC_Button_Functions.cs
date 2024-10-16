namespace TaylorMadeCode.Core.Utilities
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class TMC_Button_Functions : MonoBehaviour
    {
        public KeyCode DebugButtonVisablityKey = KeyCode.Tilde;
        public List<Button> DebugButtons;

        private void Start()
        {
            foreach (Button button in DebugButtons)
                button.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyUp(DebugButtonVisablityKey))
                foreach (Button button in DebugButtons)
                    button.gameObject.SetActive(!button.gameObject.activeSelf);
        }

        public void ExitGame()
        {
            // Exit the game
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void TogglePause()
        {
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f; // Resume the game by setting timeScale to 1
                Debug.Log("Game Resumed");
            }
            else
            {
                Time.timeScale = 0f; // Pause the game by setting timeScale to 0
                Debug.Log("Game Paused");
            }
        }

        public void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen; // Toggle full screen mode
            Debug.Log("Full screen mode toggled: " + Screen.fullScreen);
        }
    }
}