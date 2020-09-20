using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaG.SaveSystem.SaveableRuntimeInstances
{
    public interface IRuntimeInstancesManager
    {
        GameObject Instantiate(string assetId, AssetSource source = AssetSource.Resources, Scene scene = default);
    }
}