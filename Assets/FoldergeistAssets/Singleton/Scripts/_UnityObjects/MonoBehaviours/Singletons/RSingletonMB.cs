using UnityEngine;

namespace FoldergeistAssets
{
    namespace Singleton
    {
        /// <summary>
        /// Base class for resource creating monobehaviour singletons, which other classes can derive from to become singletons.
        /// You should mark your singleton (the derived class) as sealed so you can't derive from it.
        /// </summary>
        /// <typeparam name="T">This generic type, needs to be the type of the derived class</typeparam>
        [DisallowMultipleComponent]
        public abstract class RSingletonMB<T> : MonoBehaviour, ISingleton where T : RSingletonMB<T>
        {
            //The singleton instance field
            private static T _instance;

            //Whether if the singleton has been destroyed should only happen when the game closes
            protected static bool _destroyed = false;

            /// <summary>
            /// Get accesor for the singleton Instance
            /// </summary>
            public static T Instance
            {
                get
                {
                    if (_destroyed)
                    {
                        return null;
                    }

                    if (_instance == null)
                    {
                        _instance = Instantiate(SingletonManager.Instance.GetAsset<T>());
                        _instance.OnInstantiated();
                        DontDestroyOnLoad(_instance);

#if UNITY_EDITOR
                        SingletonManager.Instance.AddInstance(_instance);
#endif
                    }

                    return _instance;
                }
            }

            /// <summary>
            /// Implementation of the ISingleton called by the Instance get right after Awake and before start use this for initialization
            /// </summary>
            public abstract void OnInstantiated();

#if UNITY_EDITOR || DEVELOPMENT_BUILD

            /// <summary>
            /// Used to check if you have set the singleton up right, don't use start for initialization use OnInstantiated
            /// </summary>
            private void Start()
            {
                if (_instance == null || _instance.GetInstanceID() != GetInstanceID())
                {
                    Destroy(gameObject);

                    Debug.LogError("The instance has not been set before start or has a different id, this indicates that the singleton is coming from a scene");
                }
            }

#endif

            /// <summary>
            /// The ondestroy call made by unity you can override this but remember to base for the _destroyed bool to be set
            /// </summary>
            protected virtual void OnDestroy()
            {
                _destroyed = true;
            }

#if UNITY_EDITOR

            /// <summary>
            /// Called when this object is inspected through the OnInspectGameObject editor script, this has to be private
            /// </summary>
            private void OnInspect()
            {
                if (!Application.isPlaying)
                {
                    var instancesOfMytype = Resources.LoadAll<T>("Singletons");

                    if (instancesOfMytype.Length > 1)
                    {
                        var toBeDestroyed = new System.Collections.Generic.List<Object>();

                        for (int i = 0; i < instancesOfMytype.Length; i++)
                        {
                            if (instancesOfMytype[i].GetInstanceID() != GetInstanceID())
                            {
                                toBeDestroyed.Add(instancesOfMytype[i]);
                            }
                        }

                        DestructionWindow.OpenWindow("There shouldn't be multiple singletons in a hierarchy on a resource singleton object, it will now be destroyed on: ", toBeDestroyed.ToArray());
                    }

                    if (this != null && gameObject != null)
                    {
                        for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
                        {
                            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);

                            var gobs = activeScene.GetRootGameObjects();

                            for (int t = 0; t < gobs.Length; t++)
                            {
                                if (gobs[t] == gameObject)
                                {
                                    Debug.LogError("The resource singletons should be in the Resources Singletons folder please move: " + gameObject.name + " to the folder");
                                }

                                foreach (Transform item in gobs[t].transform)
                                {
                                    if (item.gameObject == gameObject)
                                    {
                                        Debug.LogError("The resource singletons should be in the Resources Singletons folder please move: " + gameObject.name + " to the folder");
                                    }
                                }
                            }

                            if (GetComponentsInChildren<ISingleton>().Length > 1)
                            {
                                DestructionWindow.OpenWindow("There shouldn't be multiple singletons in a hierarchy on a resource singleton object, it will now be destroyed on: ", new Object[] { this });
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Called by unity when this object is added to a gameobject
            /// </summary>
            private void Reset()
            {
                if (!Application.isPlaying)
                {
                    var instancesOfMytype = Resources.LoadAll<T>("Singletons");

                    if (instancesOfMytype.Length > 1)
                    {
                        var toBeDestroyed = new System.Collections.Generic.List<Object>();

                        for (int i = 0; i < instancesOfMytype.Length; i++)
                        {
                            if (instancesOfMytype[i].GetInstanceID() != GetInstanceID())
                            {
                                toBeDestroyed.Add(instancesOfMytype[i]);
                            }
                        }

                        DestructionWindow.OpenWindow("There shouldn't be multiple singletons in a hierarchy on a resource singleton object, it will now be destroyed on: ", toBeDestroyed.ToArray());
                    }

                    if (this != null && gameObject != null)
                    {
                        for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
                        {
                            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);

                            var gobs = activeScene.GetRootGameObjects();

                            for (int t = 0; t < gobs.Length; t++)
                            {
                                if (gobs[t] == gameObject)
                                {
                                    Debug.LogError("The resource singletons should be in the Resources Singletons folder please move: " + gameObject.name + " to the folder");
                                }

                                foreach (Transform item in gobs[t].transform)
                                {
                                    if (item.gameObject == gameObject)
                                    {
                                        Debug.LogError("The resource singletons should be in the Resources Singletons folder please move: " + gameObject.name + " to the folder");
                                    }
                                }
                            }

                            if (GetComponentsInChildren<ISingleton>().Length > 1)
                            {
                                DestructionWindow.OpenWindow("There shouldn't be multiple singletons in a hierarchy on a resource singleton object, it will now be destroyed on: ", new Object[] { this });
                            }
                        }
                    }
                }
            }

#endif
        }
    }
}