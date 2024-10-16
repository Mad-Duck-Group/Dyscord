using UnityEngine;

namespace FoldergeistAssets
{
    namespace Singleton
    {
        /// <summary>
        /// Base class for resource creating scribtable object singletons, which other classes can derive from to become singletons.
        /// You should mark your singleton (the derived class) as sealed so you can't derive from it.
        /// </summary>
        /// <typeparam name="T">This generic type, needs to be the type of the derived class</typeparam>
        public abstract class RSingletonSO<T> : InspectedSO, ISingleton where T : RSingletonSO<T>
        {
            //The singleton instance field
            private static T _instance;

            //Whether if the singleton has been destroyed should only happen when the game closes
            protected static bool _destroyed = false;

            //Whether you want to use the asset as the singleton
            [SerializeField]
            private bool _useAsset = false;

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
                        T asset = SingletonManager.Instance.GetAsset<T>();

                        _instance = asset._useAsset ? asset : Instantiate(asset);

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

#if UNITY_EDITOR

            /// <summary>
            /// Called by the OnInspectSOInspector editor script, this method has to be private
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
                }
            }

#endif
        }
    }
}