using SaG.SaveSystem.Components;
using UnityEngine;

namespace SaG.SaveSystem.SaveableRuntimeInstances
{
    public interface ISceneRuntimeInstancesManager : ISaveable
    {
        GameObject Instantiate(string assetId, InstanceSource source);

        void Destroy(SavedInstance savedInstance);

        void DestroyAllObjects();
    }
}