using SaG.SaveSystem.Core;
using SaG.SaveSystem.SaveableRuntimeInstances;
using UnityEngine;

namespace SaG.SaveSystem.Components
{
    /// <summary>
    /// Saved instances are objects that should respawn when they are not destroyed.
    /// </summary>
    [AddComponentMenu("")]
    public class SavedInstance : MonoBehaviour
    {
        private ISceneRuntimeInstancesManager instanceManager;
        private Saveable saveable;

        public Saveable Saveable => saveable;

        public bool IsDestroying { get; private set; } = false;

        public void Configure(Saveable saveable, ISceneRuntimeInstancesManager instanceManager)
        {
            this.saveable = saveable;
            this.instanceManager = instanceManager;
        }

        public void Destroy()
        {
            if (!IsDestroying)
            {
                IsDestroying = true;
                Destroy(gameObject);
                DestroyInternal();
            }
        }

        private void OnDestroy()
        {
            if (!IsDestroying)
            {
                IsDestroying = true;
                DestroyInternal();
            }
        }

        private void DestroyInternal()
        {
            saveable.ManualSaveLoad = true;
            // if gameObject destroyed by game logic (by calling Object.Destroy(go)), then we don't need information about it anymore.
            bool destroySaveData = SaveMaster.IsGameObjectDisabledExplicitly(gameObject);
            if (destroySaveData)
                SaveSystemSingleton.Instance.GameStateManager.WipeSaveable(saveable);
            SaveSystemSingleton.Instance.GameStateManager.UnregisterSaveable(saveable, !destroySaveData);
            instanceManager.Destroy(this);
        }
    }
}