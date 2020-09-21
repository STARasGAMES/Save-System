using SaG.SaveSystem.GameStateManagement;
using SaG.SaveSystem.RuntimeInstancesManagement;
using SaG.SaveSystem.Storages;

namespace SaG.SaveSystem
{
    public class SaveSystem : ISaveSystem
    {
        /// <inheritdoc/>
        public IGameStateManager GameStateManager { get; set; }
        
        /// <inheritdoc/>
        public IStorage Storage { get; set; }

        /// <inheritdoc/>
        public IRuntimeInstancesManager RuntimeInstancesManager { get; set; }

        public SaveSystem()
        {
            GameStateManager = new GameStateManager();
            Storage = new FileSystemStorage();
            RuntimeInstancesManager = new RuntimeInstancesManager(GameStateManager, new AssetResolver());
        }
        
        /// <inheritdoc/>
        public void WriteStateToStorage(string name)
        {
            Storage.Set(name, GameStateManager.GameState);
        }

        /// <inheritdoc/>
        public void ReadStateFromStorage(string name)
        {
            GameStateManager.GameState = Storage.Get<GameState>(name);
        }
    }
}