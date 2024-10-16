using FoldergeistAssets.Singleton;
using UnityEngine;

/// <summary>
/// Demo singleton for resource scriptable object, with the create asset menu attribute
/// </summary>
[CreateAssetMenu(fileName = "DemoRSingleton", menuName = "Singletons/DemoRSingleton", order = 0)]
public sealed class DemoSORSingleton : RSingletonSO<DemoSORSingleton>
{
    public string _DemoText;

    public override void OnInstantiated()
    {
    }
}