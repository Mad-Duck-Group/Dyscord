using UnityEngine;

namespace FoldergeistAssets
{
    namespace Singleton
    {
        /// <summary>
        /// Base class for singletons which only work when they are in a scene and not instantiated
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [DisallowMultipleComponent]
        public abstract class SceneSingletonMB<T> : MonoBehaviour, ISingleton where T : SceneSingletonMB<T>
        {
            //The singleton instance field
            private static T _instance;

            /// <summary>
            /// Get accesor for the singleton Instance
            /// </summary>
            public static T Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                            Debug.LogError("There was not found a scene singleton of type: " + typeof(T));
#endif

                            return null;
                        }

                        _instance.OnInstantiated();
                    }

                    return _instance;
                }
            }

            /// <summary>
            /// Called as a part of when the instance is set
            /// </summary>
            public abstract void OnInstantiated();

            /// <summary>
            /// Called by unty when this is created to set the instance of the singleton
            /// </summary>
            private void Awake()
            {
                if (_instance != null)
                {
                    if (_instance.GetInstanceID() != GetInstanceID())
                    {
                        Destroy(gameObject);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogError("An instance of scene singleton already exists, it has the type of: " + typeof(T));
#endif
                    }

                    return;
                }

                _instance = (T)this;

                OnInstantiated();
            }

#if UNITY_EDITOR

            /// <summary>
            /// Called when inspecting a gameobject
            /// </summary>
            private void OnInspect()
            {
                if (!Application.isPlaying)
                {
                    var isAsset = true;

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
                                    isAsset = false;
                                }

                                foreach (Transform item in gobs[t].transform)
                                {
                                    if (item.gameObject == gameObject)
                                    {
                                        isAsset = false;
                                    }
                                }
                            }

                            if (GetComponentsInChildren<ISingleton>().Length > 1)
                            {
                                DestructionWindow.OpenWindow("There shouldn't be multiple singletons in a hierarchy on a resource singleton object, it will now be destroyed on: ", new Object[] { this });
                            }
                        }
                    }

                    if (isAsset)
                    {
                        DestructionWindow.OpenWindow("This is a scene singleton and should only be in a scene: ", new Object[] { gameObject });
                    }
                }
            }

            /// <summary>
            /// Called by the editor when a this is added to a gameobject in the inspector
            /// </summary>
            private void Reset()
            {
                if (!Application.isPlaying)
                {
                    var isAsset = true;

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
                                    isAsset = false;
                                }

                                foreach (Transform item in gobs[t].transform)
                                {
                                    if (item.gameObject == gameObject)
                                    {
                                        isAsset = false;
                                    }
                                }
                            }

                            if (GetComponentsInChildren<ISingleton>().Length > 1)
                            {
                                DestructionWindow.OpenWindow("There shouldn't be multiple singletons in a hierarchy on a resource singleton object, it will now be destroyed on: ", new Object[] { this });
                            }
                        }
                    }

                    if (isAsset)
                    {
                        DestructionWindow.OpenWindow("This is a scene singleton and should only be in a scene: ", new Object[] { gameObject });
                    }
                }
            }

#endif
        }
    }
}