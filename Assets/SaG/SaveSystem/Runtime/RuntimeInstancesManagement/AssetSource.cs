

namespace SaG.SaveSystem.RuntimeInstancesManagement
{
    /// <summary>
    /// Used to tell the AssetResolver where to pull the 
    /// Gameobject from.
    /// </summary>
    public enum AssetSource
    {
        Resources,
        Registered,
        // todo Addressables?
    }
}