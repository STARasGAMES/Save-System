using System.Collections.Generic;
using SaG.SaveSystem.Data;
using SaG.SaveSystem.SaveableRuntimeInstances;
using UnityEngine;

namespace SaG.SaveSystem.Core
{
    public class SaveSystem : ISaveSystem
    {
        /// <inheritdoc/>
        public IGameStateManager GameStateManager { get; set; } = new GameStateManager();
        
        /// <inheritdoc/>
        public FileUtility FileUtility { get; set; } = new FileUtility();

        /// <inheritdoc/>
        public IRuntimeInstancesManager RuntimeInstancesManager { get; set; } = new RuntimeInstancesManager();

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