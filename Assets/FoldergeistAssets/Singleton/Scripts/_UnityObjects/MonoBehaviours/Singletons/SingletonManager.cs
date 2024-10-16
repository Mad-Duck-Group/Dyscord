using System.Collections.Generic;
using UnityEngine;

namespace FoldergeistAssets
{
    namespace Singleton
    {
        /// <summary>
        /// The singleton manager gets all singletons references
        /// </summary>
        public sealed class SingletonManager : SCSingletonMB<SingletonManager>
        {
#if UNITY_EDITOR

            [SerializeField] //The instances of the active singletons,  only used to track instances at runtime
            private List<Object> _instances = new List<Object>();

#endif

            [SerializeField] //The singletons loaded from the resources folder
            private List<Object> _resourceSingletons;

            /// <summary>
            /// Overriden the abstract method from the singleton base class which this derives from
            /// </summary>
            public override void OnInstantiated()
            {
                _resourceSingletons = new List<Object>(Resources.LoadAll("Singletons", typeof(ISingleton)));
            }

#if UNITY_EDITOR

            /// <summary>
            /// Method to add new instances of singletons when they are created
            /// </summary>
            /// <param name="instance">The instance of the singleton</param>
            public void AddInstance(Object instance)
            {
                _instances.Add(instance);
            }

#endif

            /// <summary>
            /// Method for making an instance of a singleton which is created through resources
            /// </summary>
            /// <typeparam name="T">The type of the singleton</typeparam>
            /// <returns>The instantiated clone of the resource object</returns>
            public T GetAsset<T>() where T : Object, ISingleton
            {
                T asset;

                if ((asset = (T)_resourceSingletons.Find(i => i.GetType() == typeof(T))) != null)
                {
                    return asset;
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("The asset of type: " + typeof(T).ToString() + " could not be fount, make sure there is an instance of this in the Resources/Singletons folder");
#endif

                return null;
            }
        }
    }
}