using UnityEngine;
using UnityEditor;

/// <summary>
/// This class is used to call the OnInspect method on components of gameobjects
/// </summary>
[CustomPreview(typeof(GameObject))]
public class OnInspectGameObject : ObjectPreview
{
    /// <summary>
    /// This method is called by the unity editor when gameobjects are being previewed
    /// </summary>
    /// <param name="targets">Contains the gameobject which is previewed</param>
    public override void Initialize(Object[] targets)
    {
        base.Initialize(targets);

        var goTarget = (targets[0] as GameObject);
        var components = goTarget.GetComponents<MonoBehaviour>();

        //Going through all monobehaviours on the gameobject to see if any implement the method OnInspect
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] != null)
            {
                var mInfo = components[i].GetType().GetMethod("OnInspect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (mInfo != null && mInfo.IsPrivate)
                {
                    mInfo.Invoke(components[i], null);
                }

                var type = components[i].GetType().BaseType;

                //Ascending the inheritance of the monobehaviour to see if any of the parent types implements the mehtod
                while (type != typeof(MonoBehaviour))
                {
                    mInfo = type.GetMethod("OnInspect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    if (mInfo != null && mInfo.IsPrivate)
                    {
                        mInfo.Invoke(components[i], null);
                    }

                    type = type.BaseType;
                }
            }
            else
            {
                Debug.LogError(goTarget.name + " has an empty component script");
            }
        }

        var root = goTarget.transform.root;

        var rComponents = root.GetComponents<MonoBehaviour>();

        //Going through all monobehaviours on the root gameobject to see if any implement the method OnHierarchyInspect
        for (int i = 0; i < rComponents.Length; i++)
        {
            if (rComponents[i] != null)
            {
                var mInfo = rComponents[i].GetType().GetMethod("OnHierarchyInspect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (mInfo != null && mInfo.IsPrivate)
                {
                    mInfo.Invoke(rComponents[i], null);
                }

                var type = rComponents[i].GetType().BaseType;

                //Ascending the inheritance of the monobehaviour to see if any of the parent types implements the mehtod
                while (type != typeof(MonoBehaviour))
                {
                    mInfo = type.GetMethod("OnHierarchyInspect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    if (mInfo != null && mInfo.IsPrivate)
                    {
                        mInfo.Invoke(rComponents[i], null);
                    }

                    type = type.BaseType;
                }
            }
            else
            {
                Debug.LogError(root.gameObject.name + " has an empty component script");
            }
        }

        //Going through the entire hierarchy of the root transform
        foreach (Transform transform in root)
        {
            var tComponents = transform.GetComponents<MonoBehaviour>();

            //Going through all monobehaviours on the transform in the root to see if any implement the method OnHierarchyInspect
            for (int i = 0; i < tComponents.Length; i++)
            {
                if (tComponents[i] != null)
                {
                    var mInfo = tComponents[i].GetType().GetMethod("OnHierarchyInspect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    if (mInfo != null && mInfo.IsPrivate)
                    {
                        mInfo.Invoke(tComponents[i], null);
                    }

                    var type = tComponents[i].GetType().BaseType;

                    //Ascending the inheritance of the monobehaviour to see if any of the parent types implements the mehtod
                    while (type != typeof(MonoBehaviour))
                    {
                        mInfo = type.GetMethod("OnHierarchyInspect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                        if (mInfo != null && mInfo.IsPrivate)
                        {
                            mInfo.Invoke(tComponents[i], null);
                        }

                        type = type.BaseType;
                    }
                }
                else
                {
                    Debug.LogError(transform.gameObject.name + " has an empty component script");
                }
            }
        }
    }
}