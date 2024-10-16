using FoldergeistAssets.Singleton;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This class is used so you can force singleton scriptable object assets to be used instead of a clone, by changing the bool value for use asset and checking for singleton instances in the resources folder
/// </summary>
public class SingletonSettings : AssetPostprocessor
{
    /// <summary>
    /// Called to cleanup all duplicates of resource singleton assets
    /// </summary>
    /// <param name="importedAssets"></param>
    /// <param name="deletedAssets"></param>
    /// <param name="movedAssets"></param>
    /// <param name="movedFromAssetPaths"></param>
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var assets = Resources.LoadAll("Singletons", typeof(ISingleton));
        List<UnityEngine.Object> approvedAssets = new List<UnityEngine.Object>();
        List<UnityEngine.Object> assetsToDestroy = new List<UnityEngine.Object>();

        for (int i = 0; i < assets.Length; i++)
        {
            if (!approvedAssets.Any(a => a.GetType() == assets[i].GetType()))
            {
                approvedAssets.Add(assets[i]);
            }
            else
            {
                Debug.LogError("You have multiple copies of the singleton of type: " + assets[i].GetType() + " in your resources folder, deleting the duplicates, remember to clean up the files");
                assetsToDestroy.Add(assets[i]);
            }
        }

        for (int i = 0; i < assetsToDestroy.Count; i++)
        {
            if (assetsToDestroy[i] is MonoBehaviour)
            {
                if (!AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath((assetsToDestroy[i] as MonoBehaviour).gameObject)))
                {
                    UnityEngine.Object.DestroyImmediate((assetsToDestroy[i] as MonoBehaviour).gameObject, true);
                }
            }
            else
            {
                if (!AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assetsToDestroy[i])))
                {
                    UnityEngine.Object.DestroyImmediate(assetsToDestroy[i], true);
                }
            }
        }
    }

    /// <summary>
    /// Method to be called when you press the menu item Prebuild
    /// </summary>
    [MenuItem("SingletonSettings/Prebuild")]
    public static void ChangeSettingsBeforeBuild()
    {
        var singletonAssets = Resources.LoadAll("Singletons", typeof(ISingleton));

        using (StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/SingletonSettings.txt"))
        {
            for (int i = 0; i < singletonAssets.Length; i++)
            {
                if (singletonAssets[i] is ScriptableObject)
                {
                    var useAsset = singletonAssets[i].GetType().BaseType.GetField("_useAsset", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    writer.WriteLine(singletonAssets[i].name + "=" + useAsset.GetValue(singletonAssets[i]).ToString());
                    useAsset.SetValue(singletonAssets[i], true);
                    new SerializedObject(singletonAssets[i]).ApplyModifiedProperties();
                    EditorUtility.SetDirty(singletonAssets[i]);
                }
            }
        }
    }

    /// <summary>
    /// Method to be called when you press the menu item PostBuild
    /// </summary>
    [MenuItem("SingletonSettings/PostBuild")]
    public static void ChangeSettingsAfterBuild()
    {
        var singletonAssets = Resources.LoadAll("Singletons", typeof(ISingleton));

        using (StreamReader reader = new StreamReader(Application.persistentDataPath + "/SingletonSettings.txt"))
        {
            var dataValues = new List<string>(reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None));

            for (int i = 0; i < singletonAssets.Length; i++)
            {
                if (singletonAssets[i] is ScriptableObject)
                {
                    var useAsset = singletonAssets[i].GetType().BaseType.GetField("_useAsset", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    useAsset.SetValue(singletonAssets[i], bool.Parse(dataValues.Find(d => d.Contains(singletonAssets[i].name)).Split('=')[1]));

                    new SerializedObject(singletonAssets[i]).ApplyModifiedProperties();
                    EditorUtility.SetDirty(singletonAssets[i]);
                }
            }
        }
    }
}