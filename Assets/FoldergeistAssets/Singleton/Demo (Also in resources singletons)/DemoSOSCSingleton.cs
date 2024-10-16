using FoldergeistAssets.Singleton;
using UnityEngine;

/// <summary>
/// A demo singleton for the self creating scriptable object
/// </summary>
[CreateAssetMenu(fileName = "DemoSCSingleton", menuName = "Singletons/DemoSCSingleton", order = 0)]
public sealed class DemoSOSCSingleton : SCSingletonSO<DemoSOSCSingleton>
{
    public string _DemoText;

    public override void OnInstantiated()
    {
        _DemoText = "This message is from DemoSOSCSingleton on the object: " + name;
    }
}