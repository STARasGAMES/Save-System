using SaG.SaveSystem.GameStateManagement;
using UnityEngine;

namespace SaG.SaveSystem.RuntimeInstancesManagement
{
    /// <summary>
    /// Saved instances are objects that should respawn when they are not destroyed.
    /// </summary>
    [AddComponentMenu("")]
    public class SavedInstance : MonoBehaviour
    {
        private ISceneRuntimeInstancesManager _instanceManager;
        private Saveable _saveable;

        public Saveable Saveable => _saveable;

        public bool IsDestroying { get; private set; } = false;

        public void Configure(Saveable saveable, ISceneRuntimeInstancesManager instanceManager)
        {
            _saveable = saveable;
            _instanceManager = instanceManager;
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
            _saveable.ManualSaveLoad = true;
            // if gameObject destroyed by game logic (by calling Object.Destroy(go)), then we don't need information about it anymore.
            bool destroySaveData = Utilities.IsGameObjectDisabledExplicitly(gameObject);
            if (destroySaveData)
                SaveSystemSingleton.Instance.GameStateManager.WipeSaveable(_saveable);
            SaveSystemSingleton.Instance.GameStateManager.UnregisterSaveable(_saveable, !destroySaveData);
            _instanceManager.Destroy(this);
        }
    }
}