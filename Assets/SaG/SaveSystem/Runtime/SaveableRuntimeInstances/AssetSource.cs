

namespace SaG.SaveSystem.SaveableRuntimeInstances
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