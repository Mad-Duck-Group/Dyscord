using UnityEngine;

namespace FoldergeistAssets
{
    /// <summary>
    /// This is the base class for self create singletons
    /// </summary>
    public abstract class SelfCreateSingletonBaseMB : MonoBehaviour
    {
        //The gameobject which will have all the selfcreated singeltons added
        protected static GameObject _gameObject;
    }
}