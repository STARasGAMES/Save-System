using UnityEngine;

namespace SaG.SaveSystem.RuntimeInstancesManagement
{
    public interface IAssetResolver
    {
        GameObject Resolve(string assetId, AssetSource source);

        void RegisterAsset(string assetId, GameObject asset);
    }
}