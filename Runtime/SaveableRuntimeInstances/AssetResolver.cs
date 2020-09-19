using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaG.SaveSystem.SaveableRuntimeInstances
{
    public class AssetResolver : IAssetResolver
    {
        private readonly IDictionary<string, GameObject> _assetIdToAssetMap;
        public AssetResolver()
        {
            _assetIdToAssetMap = new Dictionary<string, GameObject>();
        }

        public GameObject Resolve(string assetId, InstanceSource source)
        {
            // Implement more spawn methods here.
            // Such as usage for Asset Bundles & Adressables
            switch (source)
            {
                case InstanceSource.Resources:
                    return Resources.Load(assetId) as GameObject;
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