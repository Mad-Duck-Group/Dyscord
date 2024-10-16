using UnityEngine;

namespace FoldergeistAssets
{
    namespace Singleton
    {
        /// <summary>
        /// Base class for self creating scribtable object singletons, which other classes can derive from to become singletons.
        /// You should mark your singleton (the derived class) as sealed so you can't derive from it.
        /// </summary>
        /// <typeparam name="T">This generic type, needs to be the type of the derived class</typeparam>
        public abstract class SCSingletonSO<T> : ScriptableObject, ISingleton where T : SCSingletonSO<T>
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
                        _instance = CreateInstance<T>();
                        _instance.OnInstantiated();
                        DontDestroyOnLoad(_instance);

#if UNITY_EDITOR
                        SingletonManager.Instance.AddInstance(_instance);
#endif
                    }

                    return _instance;
                }
            }

#if UNITY_EDITOR

            private void Awake()
            {
                if (!Application.isPlaying)
                {
                    DestructionWindow.OpenWindow("A self creating scriptable object singleton should not have an instance on editor time, it will now be destroyed ", new Object[] { this });
                }
            }

#endif

            /// <summary>
            /// Implementation of the ISingleton called by the Instance get right after Awake
            /// </summary>
            public abstract void OnInstantiated();

            /// <summary>
            /// The ondestroy call made by unity you can override this but remember to base for the _destroyed bool to be set
            /// </summary>
            protected virtual void OnDestroy()
            {
                _destroyed = true;
            }
        }
    }
}