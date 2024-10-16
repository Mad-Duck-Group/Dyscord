using FoldergeistAssets;
using UnityEditor;

/// <summary>
/// This editor is used to call the OnInspect method for scriptable objects deriving from InSpectedSO.
/// If you want to make custom inspectors for other scriptable objects, which derive from InSpectedSO, let the inspector derive from this and remember to base OnEnable
/// </summary>
[CustomEditor(typeof(InspectedSO), true, isFallback = true)]
public class OnInspectSOInspector : Editor
{
    /// <summary>
    /// Method called by Unity, in this method the types are searched for the method called OnInspect
    /// </summary>
    protected virtual void OnEnable()
    {
        var mInfo = target.GetType().GetMethod("OnInspect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (mInfo != null && mInfo.IsPrivate)
        {
            mInfo.Invoke(target, null);
        }

        var type = target.GetType().BaseType;

        while (type != typeof(InspectedSO))
        {
            mInfo = type.GetMethod("OnInspect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (mInfo != null && mInfo.IsPrivate)
            {
                mInfo.Invoke(target, null);
            }

            type = type.BaseType;
        }
    }
}