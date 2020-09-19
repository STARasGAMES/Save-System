using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaG.SaveSystem.SaveableRuntimeInstances
{
    public interface IRuntimeInstancesManager
    {
        GameObject Instantiate(string assetId, InstanceSource source = InstanceSource.Resources, Scene scene = default);
    }
}