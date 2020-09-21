using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaG.SaveSystem.RuntimeInstancesManagement
{
    public class AssetResolver : IAssetResolver
    {
        private readonly IDictionary<string, GameObject> _assetIdToAssetMap;
        public AssetResolver()
        {
            _assetIdToAssetMap = new Dictionary<string, GameObject>();
        }

        public GameObject Resolve(string assetId, AssetSource source)
        {
            switch (source)
            {
                case AssetSource.Resources:
                    var res = Resources.Load(assetId) as GameObject;
                    if (res == null)
                        throw new Exception($"Can't resolve asset with id: '{assetId}' from Resources");
                    return res;
                case AssetSource.Registered:
                    if (!_assetIdToAssetMap.TryGetValue(assetId, out var asset))
                    {
                        throw new ArgumentOutOfRangeException(nameof(assetId), $"Can't resolve asset from registered assets.");
                    }
                    return asset;
                default:
                    throw new NotImplementedException($"Instance source '{source.ToString()}' is not implemented.");
            }
        }
        
        public void RegisterAsset(string assetId, GameObject asset)
        {
            _assetIdToAssetMap[assetId] = asset;
        }
    }
}