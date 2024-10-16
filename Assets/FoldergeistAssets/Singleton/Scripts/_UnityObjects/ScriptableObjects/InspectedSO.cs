using UnityEngine;

namespace FoldergeistAssets
{
    /// <summary>
    /// Derive from this class when you want to have an OnInspect functionality for scriptable objects, this is done through the editor script called OnInspectSOInspector.
    /// Make a method called OnInspect for the call make the method private.
    /// </summary>
    public abstract class InspectedSO : ScriptableObject
    {
    }
}