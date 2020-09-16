using System;
using SaG.GuidReferences;
using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D.CustomGuidProvider
{
    [DisallowMultipleComponent]
    public class ScriptableObjectGuidProvider : MonoBehaviour, IGuidProvider
    {
        [SerializeField] private ScriptableObjectGuidProviderAsset guidProviderAsset = default;
        
        public Guid GetGuid()
        {
            return guidProviderAsset.GetGuid();
        }

        public string GetStringGuid()
        {
            return guidProviderAsset.GetStringGuid();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (guidProviderAsset == null)
            {
                Debug.LogError($"Guid provider asset is not assigned!", this);
            }
        }
#endif
    }
}