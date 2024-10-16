using FoldergeistAssets.Singleton;

/// <summary>
/// A demo singleton of resource monobehaviour
/// </summary>
public sealed class DemoMBRSingleton : RSingletonMB<DemoMBRSingleton>
{
    public string _DemoText = "";

    public override void OnInstantiated()
    {
    }
}