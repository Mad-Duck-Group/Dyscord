In this asset there are different singleton base classes to derive from, some of the singletons will have to be put in a folder called for Singletons in
the Resources folder, while others should never be on a gameobject in the editor, the last one is a Scene singleton that needs to be in a scene it won't be made to a prefab.

These five classes are in the namespace FoldergeistSingletons and are called:
- RSingletonSO (A ScirptableObject singleton in Resources)
- SCSingletonSO (A selfcreating ScriptableObject singleton)
- RSingletonMB (A MonoBehaviour singleton in Resources)
- SCSingletonMB (A selfcreating MonoBehaviour singleton all added to the same GameObject at runtime)
- SceneSingleton (A MonoBehaviour singleton which shouldn't be made into a prefab, it should only be in a specific scene)

In this asset there are two methods which are called by the editor
- OnInspect 
- OnHierarchyInspect

OnInspect is called on MonoBehaviours automatically when you start inspecting a gameobject, it is also called
on scriptable objects that derive from InspectedSO though if you make a custom editor for your 
scriptable object you need to derive from OnInspectSOInspector.

OnHierarchyInspect will called on MonoBehaviours automatically and will be called when you start inspecting any 
gameobject in a hierarchy 

These methods need to be private and you can have multiple of these methods in a parent child class structure (Inheritance)

There are snippet scripts for these methods in the editor folder inside the snippets folder,
these files can be added to your coding environment like Visual Studio.

You can delete the Demo folder and the objects in the Resources Singletons folder, the Singletons folder needs to be in Resources for the Resource singletons to work.

