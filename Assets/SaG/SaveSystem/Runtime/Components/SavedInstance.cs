using SaG.SaveSystem.Core;
using UnityEngine;

namespace SaG.SaveSystem.Components
{
    /// <summary>
    /// Saved instances are objects that should respawn when they are not destroyed.
    /// </summary>
    [AddComponentMenu("")]
    public class SavedInstance : MonoBehaviour
    {
        private SaveInstanceManager instanceManager;
        private Saveable saveable;

        // By default, when destroyed, the saved instance will wipe itself from existance.
        private bool removeData = true;

        public void Configure(Saveable saveable, SaveInstanceManager instanceManager)
        {
            this.saveable = saveable;
            this.instanceManager = instanceManager;
        }

        public void Destroy()
        {
            saveable.ManualSaveLoad = true;
            removeData = false;
            SaveSystemSingleton.Instance.GameStateManager.UnregisterContainer(saveable, true);
            //SaveMaster.RemoveListener(saveable);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (SaveMaster.DeactivatedObjectExplicitly(gameObject))
            {
                if (removeData)
                {
                    SaveMaster.WipeSaveable(saveable);
                    instanceManager.DestroyObject(this, saveable);
                }
            }
        }
    }
}
