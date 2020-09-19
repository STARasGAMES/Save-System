using UnityEngine;

namespace SaG.SaveSystem.Samples.RuntimeSpawn
{
    public class Sample : MonoBehaviour
    {
        public void Save()
        {
            var saveSystem = SaveSystemSingleton.Instance;
            saveSystem.GameStateManager.SynchronizeState();
            saveSystem.WriteStateToDisk("runtime_spawn_save_sample");
        }

        public void Load()
        {
            var saveSystem = SaveSystemSingleton.Instance;
            saveSystem.ReadStateFromDisk("runtime_spawn_save_sample");
            saveSystem.GameStateManager.LoadState();
        }
    }
}