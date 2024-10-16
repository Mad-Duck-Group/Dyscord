using FoldergeistAssets.Singleton;

/// <summary>
/// Demo singleton of self create monobehaviour
/// </summary>
public sealed class DemoMBSCSingleton : SCSingletonMB<DemoMBSCSingleton>
{
    public string _DemoText;

    public override void OnInstantiated()
    {
        _DemoText = "This message is from DemoMBSCSingleton on the gameobject: " + gameObject.name + " in DontDestroyOnLoad scene";
    }
}