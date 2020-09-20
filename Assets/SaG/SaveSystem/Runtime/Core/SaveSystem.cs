using System.Collections.Generic;
using SaG.SaveSystem.Data;
using SaG.SaveSystem.SaveableRuntimeInstances;
using UnityEngine;

namespace SaG.SaveSystem.Core
{
    public class SaveSystem : ISaveSystem
    {
        /// <inheritdoc/>
        public IGameStateManager GameStateManager { get; set; }
        
        /// <inheritdoc/>
        public FileUtility FileUtility { get; set; }

        /// <inheritdoc/>
        public IRuntimeInstancesManager RuntimeInstancesManager { get; set; }

        public SaveSystem()
        {
            GameStateManager = new GameStateManager();
            FileUtility = new FileUtility();
            RuntimeInstancesManager = new RuntimeInstancesManager(GameStateManager, new AssetResolver());
        }
        
        /// <inheritdoc/>
        public void WriteStateToDisk(string name)
        {
            FileUtility.WriteObjectToFile(name, GameStateManager.GameState);
        }

        /// <inheritdoc/>
        public void ReadStateFromDisk(string name)
        {
            GameStateManager.GameState = FileUtility.ReadObjectFromFile<GameState>(name);
        }
    }
}