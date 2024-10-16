#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

/// <summary>
/// This is an editor window which some non editor classes needs to reference for detroying specific objects (mainly singleton copies and such)
/// </summary>
public class DestructionWindow : EditorWindow
{
    private static string _msg;
    private static DestructionWindow _window;
    private static UnityEngine.Object[] _toBeDestroyed;

    /// <summary>
    /// Method for opening the window
    /// </summary>
    /// <returns></returns>
    public static void OpenWindow(string msg, UnityEngine.Object[] toBeDestroyed)
    {
        _msg = msg;
        _toBeDestroyed = toBeDestroyed;
        _window = GetWindow<DestructionWindow>("Destroying");

        _window.minSize = new Vector2(500, 60);
    }

    /// <summary>
    /// Called by unity when the window is closed, here the object targeted for destruction will be destroyed
    /// </summary>
    private void OnDestroy()
    {
        for (int i = 0; i < _toBeDestroyed.Length; i++)
        {
            if (AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_toBeDestroyed[i])))
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();
            }
            else
            {
                DestroyImmediate(_toBeDestroyed[i]);
            }
        }
    }

    /// <summary>
    /// Drawing the window to showcase a message and a button for closing the window
    /// </summary>
    private void OnGUI()
    {
        GUI.enabled = false;

        for (int i = 0; i < _toBeDestroyed.Length; i++)
        {
            EditorGUILayout.TextArea(_msg + _toBeDestroyed[i].name);
        }

        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Close"))
        {
            _window.Close();
        }
    }
}

#endif