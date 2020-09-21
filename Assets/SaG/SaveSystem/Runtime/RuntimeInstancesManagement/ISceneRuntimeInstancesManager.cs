using SaG.SaveSystem.GameStateManagement;
using UnityEngine;

namespace SaG.SaveSystem.RuntimeInstancesManagement
{
    public interface ISceneRuntimeInstancesManager : ISaveable
    {
        GameObject Instantiate(string assetId, AssetSource source);

        void Destroy(SavedInstance savedInstance);

        void DestroyAllObjects();
    }
}