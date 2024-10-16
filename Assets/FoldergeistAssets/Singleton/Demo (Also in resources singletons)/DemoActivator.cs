using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class to activate the demo singletons
/// </summary>
[RequireComponent(typeof(Text))]
public class DemoActivator : MonoBehaviour
{
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();

        text.text += DemoMBRSingleton.Instance._DemoText;
        text.text += "\n" + DemoMBSCSingleton.Instance._DemoText;
        text.text += "\n" + DemoSORSingleton.Instance._DemoText;
        text.text += "\n" + DemoSOSCSingleton.Instance._DemoText;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

#if UNITY_EDITOR

    /// <summary>
    /// This method is called in the editor automaticaly if this method is in a MonoBehaviour or a scriptableobject which derives form InspectedSO
    /// </summary>
    private void OnInspect()
    {
    }

#endif
}