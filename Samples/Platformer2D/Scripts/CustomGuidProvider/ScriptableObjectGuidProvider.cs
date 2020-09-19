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

        private void Awake()
        {
            if (guidProviderAsset == null)
            {
                Debug.LogError("Guid provider asset is missing.", this);
                return;
            }

            if (!GuidManagerSingleton.Add(guidProviderAsset.GetGuid(), gameObject))
            {
                Debug.LogError($"There are another game object with the same Guid: {guidProviderAsset.GetGuid()}. " +
                               $"Guid provider asset: {guidProviderAsset.name}.", this);
            }
        }

        private void OnDestroy()
        {
            GuidManagerSingleton.Remove(guidProviderAsset.GetGuid());
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